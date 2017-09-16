using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ProjectOxford.Vision;
using Microsoft.WindowsAzure.Storage.Queue;
using Shared.Models;

namespace FunctionApp1
{
    [StorageAccount("my-storage-connection")]
    public static class Function2
    {
        [FunctionName("Function2")]
        [return: Queue("2-to-ascii")]
        public static async Task<CloudQueueMessage> Run(
            [QueueTrigger("1-to-cognitive")]                          AnalysisReq request,
            [Blob("%input-container%/{BlobRef}", FileAccess.Read)]    Stream inBlob,
                                                                      TraceWriter log)
        {
            log.Info("Running image analysis...");

            var subscriptionKey = ConfigurationManager.AppSettings["CognitiveServicesKey"];
            var client = new VisionServiceClient(subscriptionKey, "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0");

            var result = await client.AnalyzeImageAsync(inBlob,
                                                        new[]
                                                        {
                                                            VisualFeature.Categories,
                                                            VisualFeature.Color,
                                                            VisualFeature.Description,
                                                            VisualFeature.Faces,
                                                            VisualFeature.ImageType,
                                                            VisualFeature.Tags
                                                        });

            var asciiArtRequest = new AsciiArtRequest
                                  {
                                      BlobRef = request.BlobRef,
                                      Description = string.Join(",", result.Description.Captions.Select(c => c.Text)),
                                      Tags = result.Tags.Select(t => t.Name).ToArray()
                                  };

            return asciiArtRequest.AsQueueItem();
        }
    }
}
