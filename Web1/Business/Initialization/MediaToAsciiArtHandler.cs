using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using Shared.Models;
using InitializationModule = EPiServer.Web.InitializationModule;

namespace Web1.Business.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(InitializationModule))]
    public class MediaToAsciiArtHandler : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            var canon = ServiceLocator.Current.GetInstance<IContentEvents>();
            canon.PublishedContent += CanonOnPublishedContent;
        }

        public void Uninitialize(InitializationEngine context)
        {
            var canon = ServiceLocator.Current.GetInstance<IContentEvents>();
            canon.PublishedContent -= CanonOnPublishedContent;
        }

        private void CanonOnPublishedContent(object sender, ContentEventArgs contentEventArgs)
        {
            if(contentEventArgs.Content is ImageData img)
            {
                using (var stream = img.BinaryData.OpenRead())
                {
                    var bytes = stream.ReadAllBytes();
                    var result = CallFunction(img.ContentGuid.ToString(), bytes).GetAwaiter().GetResult();
                }
            }
        }

        private static readonly HttpClient _funcClient = new HttpClient();

        static async Task<string> CallFunction(string contentReference, byte[] byteData)
        {
            var uri = "http://localhost:7071/api/Function1";

            var req = new Req
                      {
                          FileId = contentReference,
                          Content = byteData
                      };

            using (var content = new StringContent(JsonConvert.SerializeObject(req)))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await _funcClient.PostAsync(uri, content).ConfigureAwait(false);
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }
    }
}
