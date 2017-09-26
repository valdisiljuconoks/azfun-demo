using System;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Shared.Models;

namespace Web1.Features.AsciiArt
{
    class CloudQueueAsciiResponseRetriever : IAsciiResponseRetriever
    {
        private readonly IAsciiArtImageProcessor _processor;
        private readonly IAsciiArtServiceSettingsProvider _settings;

        public CloudQueueAsciiResponseRetriever(
            IAsciiArtServiceSettingsProvider settings,
            IAsciiArtImageProcessor processor)
        {
            _settings = settings;
            _processor = processor;
        }

        public void Pump(StringBuilder log)
        {
            var settings = _settings.GetSettings();
            var account = CloudStorageAccount.Parse(settings.StorageUrl);
            var queueClient = account.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference(settings.DoneQueueName);

            var draftMsg = queue.GetMessage();
            if(draftMsg == null)
            {
                log.AppendLine("No messages found in the queue");
                return;
            }

            while (draftMsg != null)
            {
                var message = JsonConvert.DeserializeObject<AsciiArtResult>(draftMsg.AsString);

                log.AppendLine($"Started processing image ({message.BlobRef})...");

                try
                {
                    _processor.SaveAsciiArt(account, message);
                    queue.DeleteMessage(draftMsg);

                    log.AppendLine($"Finished image ({message.BlobRef}).");
                }
                catch (Exception e)
                {
                    log.AppendLine($"Error occoured: {e.Message}");
                }

                draftMsg = queue.GetMessage();
            }
        }
    }
}
