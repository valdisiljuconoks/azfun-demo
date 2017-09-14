using System;
using System.Configuration;
using System.IO;
using System.Text;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Shared.Models;
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

        /// <summary>
        ///     Called when a user clicks on Stop for a manually started job, or when ASP.NET shuts down.
        /// </summary>
        public override void Stop()
        {
            _stopSignaled = true;
        }

        /// <summary>
        ///     Called when a scheduled job executes
        /// </summary>
        /// <returns>A status message to be stored in the database log and visible from admin mode</returns>
        public override string Execute()
        {
            //Call OnStatusChanged to periodically notify progress of job for manually started jobs
            OnStatusChanged($"Starting execution of {GetType()}");

            //Add implementation

            var account = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["MyStorageConnection"].ConnectionString);

            var queueClient = account.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference("3-done");
            var draftMsg = queue.GetMessage();
            var message = JsonConvert.DeserializeObject<AsciiArtResult>(draftMsg.AsString);

            var blobClient = account.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("out-container");
            var asciiBlob = container.GetBlobReference(message.BlobRef);

            var image = _repository.Get<ImageFile>(Guid.Parse(message.BlobRef));
            var writable = image.CreateWritableClone() as ImageFile;

            using (var stream = new MemoryStream())
            {
                asciiBlob.DownloadToStream(stream);
                var asciiArt = Encoding.UTF8.GetString(stream.ToArray());
                writable.AsciiArt = asciiArt;

                _repository.Save(writable, SaveAction.Publish);
            }

            queue.DeleteMessage(draftMsg);

            //For long running jobs periodically check if stop is signaled and if so stop execution
            if(_stopSignaled)
            {
                return "Stop of job was called";
            }

            return "Change to message that describes outcome of execution";
        }
    }
}
