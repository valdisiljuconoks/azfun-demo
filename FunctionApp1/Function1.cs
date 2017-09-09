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
            [Blob("container/{FileId}", Connection = "MyStorageConnection")] CloudBlockBlob outBlob,
            [Queue("to-fun2", Connection = "MyStorageConnection")] CloudQueue queue,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processing a request...");

            using (var stream = new MemoryStream(request.Content))
            {
                await outBlob.UploadFromStreamAsync(stream);
            }

            await queue.AddMessageAsync(new AnalysisReq
                                        {
                                            BlobRef = outBlob.Name
                                        }.AsQueueItem());

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(outBlob.Name) };
        }
    }
}
