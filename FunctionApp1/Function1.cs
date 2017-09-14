using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Shared.Models;

namespace FunctionApp1
{
    [StorageAccount("MyStorageConnection")]
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run(

            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]    Req request,
            [Blob("in-container/{FileId}")]                        CloudBlockBlob outBlob,
            [Queue("1-to-cognitive")]                              CloudQueue queue,
            TraceWriter                                            log)
        {
            log.Info("Received image for processing...");

            await outBlob.UploadFromByteArrayAsync(request.Content, 0, request.Content.Length);
            var analysisRequest = new AnalysisReq
                              {
                                  BlobRef = outBlob.Name
                              };

            await queue.AddMessageAsync(analysisRequest.AsQueueItem());

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(outBlob.Name) };
        }
    }
}
