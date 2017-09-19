using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shared.Models;

namespace Console1
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("... Waiting for runtime to spin");

            Thread.Sleep(TimeSpan.FromSeconds(2));
            Console.WriteLine("Calling function...");

            var result = CallFunction().GetAwaiter().GetResult();

            Console.WriteLine($"Result: {result}");

            Console.ReadLine();
        }

        static async Task<string> CallFunction()
        {
            var funcClient = new HttpClient();
            //var uri = "http://localhost:7071/api/Function1";
            var uri = "https://riga-azure-cloud-test.azurewebsites.net/api/Function1";

            var byteData = GetImageAsByteArray("c:\\temp\\pic.jpg");

            var req = new ProcessingRequest
                      {
                          FileId = "pic.jpg",
                          Content = byteData
                      };

            using (var content = new StringContent(JsonConvert.SerializeObject(req)))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await funcClient.PostAsync(uri, content);
                return await response.Content.ReadAsStringAsync();
            }
        }

        private static byte[] GetImageAsByteArray(string imageFilePath)
        {
            var fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            var binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }
    }
}
