using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace RGBJunkieDeckPlugin.Helpers
{
    internal static class DeckSettingsJson
    {
        private static readonly JsonSerializer Serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
        });

        public static JObject ToJObject(object settings) => JObject.FromObject(settings, Serializer);
    }
}
