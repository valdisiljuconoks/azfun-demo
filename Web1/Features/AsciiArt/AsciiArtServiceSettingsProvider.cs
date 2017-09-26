using System.Configuration;
using Newtonsoft.Json;
using Shared.Models;
using Web1.Business;

namespace Web1.Features.AsciiArt
{
    class AsciiArtServiceSettingsProvider : IAsciiArtServiceSettingsProvider
    {
        public SettingsMessage GetSettings()
        {
            var response = AsyncHelper.RunSync(() => Global.HttpClient.Value.GetAsync(ConfigurationManager.AppSettings["func:SettingsUri"]));
            return JsonConvert.DeserializeObject<SettingsMessage>(AsyncHelper.RunSync(() => response.Content.ReadAsStringAsync()));
        }
    }
}
