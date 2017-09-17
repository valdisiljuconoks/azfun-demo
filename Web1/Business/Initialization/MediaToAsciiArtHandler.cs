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
        private static readonly HttpClient _funcClient = new HttpClient();

        public void Initialize(InitializationEngine context)
        {
            var canon = ServiceLocator.Current.GetInstance<IContentEvents>();
            canon.CreatedContent += OnImageCreated;
        }

        public void Uninitialize(InitializationEngine context)
        {
            var canon = ServiceLocator.Current.GetInstance<IContentEvents>();
            canon.CreatedContent -= OnImageCreated;
        }

        private void OnImageCreated(object sender, ContentEventArgs args)
        {
            if(args.Content is ImageData img)
            {
                using (var stream = img.BinaryData.OpenRead())
                {
                    var bytes = stream.ReadAllBytes();
                    var result = AsyncHelper.RunSync(() => CallFunctionAsync(img.ContentGuid.ToString(), bytes));
                }
            }
        }

        private static async Task<string> CallFunctionAsync(string contentReference, byte[] byteData)
        {
            var uri = "http://localhost:7071/api/Function1";

            var req = new ProcessingRequest
                      {
                          FileId = contentReference,
                          Content = byteData,
                          Width = 150
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
