using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ProjectOxford.Vision;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Shared.Models;

namespace FunctionApp1
{
    public static class Function2
    {
        [FunctionName("Function2")]
        [return: Queue("2-to-ascii", Connection = "MyStorageConnection")]
        public static async Task<CloudQueueMessage> Run(
            [QueueTrigger("1-to-cognitive", Connection = "MyStorageConnection")] AnalysisReq request,
            [Blob("in-container/{BlobRef}", Connection = "MyStorageConnection")] CloudBlockBlob inBlob,
            TraceWriter log)
        {

            log.Info("Running image analysis...");

            var subscriptionKey = ConfigurationManager.AppSettings["CognitiveServicesKey"];
            var client = new VisionServiceClient(subscriptionKey, "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0");

            using (var image = new MemoryStream())
            {
                await inBlob.DownloadToStreamAsync(image);
                image.Position = 0;

                var result = await client.AnalyzeImageAsync(image,
                                                            new[]
                                                            {
                                                                VisualFeature.Categories,
                                                                VisualFeature.Color,
                                                                VisualFeature.Description,
                                                                VisualFeature.Faces,
                                                                VisualFeature.ImageType,
                                                                VisualFeature.Tags
                                                            });

                return new CloudQueueMessage(
                    JsonConvert.SerializeObject(new AsciiArtRequest
                                                {
                                                    BlobRef = request.BlobRef,
                                                    Description = string.Join(",", result.Description.Captions.Select(c => c.Text)),
                                                    Tags = result.Tags.Select(t => t.Name).ToArray()
                                                }));
            }
        }
    }
}
