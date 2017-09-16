using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using EPiServer;
using EPiServer.DataAccess;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Shared.Models;
using Web1.Business;
using Web1.Models.Media;

namespace Web1.ScheduledJobs
{
    [ScheduledPlugIn(DisplayName = "ScheduledJob1")]
    public class ScheduledJob1 : ScheduledJobBase
    {
        private readonly IContentRepository _repository;
        private bool _stopSignaled;

        public ScheduledJob1(IContentRepository repository)
        {
            _repository = repository;
            IsStoppable = true;
        }

        public override void Stop()
        {
            _stopSignaled = true;
        }

        public override string Execute()
        {
            OnStatusChanged($"Starting execution of {GetType()}");

            var account = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["MyStorageConnection"].ConnectionString);

            var queueClient = account.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference("3-done");
            var draftMsg = queue.GetMessage();

            if(draftMsg == null)
                return "No mesages found in the queue";

            var message = JsonConvert.DeserializeObject<AsciiArtResult>(draftMsg.AsString);

            var blobClient = account.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("out-container");
            var asciiBlob = container.GetBlobReference(message.BlobRef);

            var image = _repository.Get<ImageFile>(Guid.Parse(message.BlobRef));
            var writable = image.MakeWritable<ImageFile>();

            using (var stream = new MemoryStream())
            {
                asciiBlob.DownloadToStream(stream);
                var asciiArt = Encoding.UTF8.GetString(stream.ToArray());

                writable.AsciiArt = asciiArt;
                writable.Description = message.Description;
                writable.Tags = string.Join(",", message.Tags);

                _repository.Save(writable, SaveAction.Publish);
            }

            queue.DeleteMessage(draftMsg);

            if(_stopSignaled)
                return "Stop of job was called";

            return "Images updated successfully.";
        }
    }
}
