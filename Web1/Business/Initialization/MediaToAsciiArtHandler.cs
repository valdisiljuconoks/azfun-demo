using EPiServer;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Web1.Features.AsciiArt;
using InitializationModule = EPiServer.Web.InitializationModule;

namespace Web1.Business.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(InitializationModule))]
    public class MediaToAsciiArtHandler : IInitializableModule
    {
        private IAsciiArtRequester _uploader;

        public void Initialize(InitializationEngine context)
        {
            var canon = ServiceLocator.Current.GetInstance<IContentEvents>();
            _uploader = ServiceLocator.Current.GetInstance<IAsciiArtRequester>();

            canon.CreatedContent += OnImageCreated;
        }

        public void Uninitialize(InitializationEngine context)
        {
            var canon = ServiceLocator.Current.GetInstance<IContentEvents>();
            canon.CreatedContent -= OnImageCreated;
        }

        private void OnImageCreated(object sender, ContentEventArgs args)
        {
            if(!(args.Content is ImageData img))
                return;

            using (var stream = img.BinaryData.OpenRead())
            {
                var bytes = stream.ReadAllBytes();
                _uploader.Upload(img.ContentGuid.ToString(), bytes);
            }
        }
    }
}
