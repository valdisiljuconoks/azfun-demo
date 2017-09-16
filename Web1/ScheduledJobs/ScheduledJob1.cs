using System;
using System.IO;
using System.Net.Http;
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

        private static readonly Lazy<HttpClient> _httpClient = new Lazy<HttpClient>(() => new HttpClient());

        public override string Execute()
        {
            OnStatusChanged($"Starting execution of {GetType()}");

            var log = new StringBuilder();

            var response = AsyncHelper.RunSync(() => _httpClient.Value.GetAsync("http://localhost:7071/api/Settings"));
            var settings = JsonConvert.DeserializeObject<SettingsMessage>(AsyncHelper.RunSync(() => response.Content.ReadAsStringAsync()));

            var account = CloudStorageAccount.Parse(settings.StorageUrl);

            var queueClient = account.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference(settings.DoneQueueName);

            var draftMsg = queue.GetMessage();
            if (draftMsg == null)
                return "No mesages found in the queue";

            while (draftMsg != null)
            {
                var message = JsonConvert.DeserializeObject<AsciiArtResult>(draftMsg.AsString);

                log.AppendLine($"Started processing image ({message.BlobRef})...");

                var blobClient = account.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(settings.DoneContainerName);
                var asciiBlob = container.GetBlobReference(message.BlobRef);

                try
                {
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

                    log.AppendLine($"Finished image ({message.BlobRef}).");

                    if(_stopSignaled)
                        return "Stop of job was called";
                }
                catch (Exception e)
                {
                    // TODO proper error handling
                    log.AppendLine($"Error occoured: {e.Message}");
                }

                draftMsg = queue.GetMessage();
            }


            return log.ToString();
        }
    }
}
