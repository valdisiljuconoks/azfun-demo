using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Shared.Models;

namespace FunctionApp1
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger("post")] Req request,
            [Blob("in-container/{FileId}", Connection = "MyStorageConnection")] CloudBlockBlob outBlob,
            [Queue("1-to-cognitive", Connection = "MyStorageConnection")] CloudQueue queue,
            TraceWriter log)
        {
            log.Info("Received image for processing...");

            await outBlob.UploadFromByteArrayAsync(request.Content, 0, request.Content.Length);
            await queue.AddMessageAsync(new AnalysisReq
                                        {
                                            BlobRef = outBlob.Name
                                        }.AsQueueItem());

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(outBlob.Name) };
        }
    }
}
