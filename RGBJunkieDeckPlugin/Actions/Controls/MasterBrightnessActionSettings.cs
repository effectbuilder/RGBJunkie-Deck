using Newtonsoft.Json;

namespace RGBJunkieDeckPlugin.Actions.Controls
{
    internal sealed class MasterBrightnessActionSettings
    {
        [JsonProperty("stepPercent")]
        public int StepPercent { get; set; } = 2;

        [JsonProperty("displayPercent")]
        public int DisplayPercent { get; set; } = 100;

        public static MasterBrightnessActionSettings CreateDefault() => new MasterBrightnessActionSettings();
    }
}
