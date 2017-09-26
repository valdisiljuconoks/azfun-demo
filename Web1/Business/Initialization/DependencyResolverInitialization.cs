using System.Web.Mvc;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Web1.Business.Rendering;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using Web1.Features.AsciiArt;

namespace Web1.Business.Initialization
{
    [InitializableModule]
    public class DependencyResolverInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            //Implementations for custom interfaces can be registered here.

            context.ConfigurationComplete += (o, e) =>
            {
                //Register custom implementations that should be used in favour of the default implementations
                context.Services
                       .AddTransient<IContentRenderer, ErrorHandlingContentRenderer>()
                       .AddTransient<ContentAreaRenderer, AlloyContentAreaRenderer>()
                       .AddTransient<IAsciiResponseRetriever, CloudQueueAsciiResponseRetriever>()
                       .AddSingleton<IAsciiArtServiceSettingsProvider, AsciiArtServiceSettingsProvider>()
                       .AddTransient<IAsciiArtImageProcessor, AsciiArtImageProcessor>();
            };
        }

        public void Initialize(InitializationEngine context)
        {
            DependencyResolver.SetResolver(new ServiceLocatorDependencyResolver(context.Locate.Advanced));
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void Preload(string[] parameters)
        {
        }
    }
}
