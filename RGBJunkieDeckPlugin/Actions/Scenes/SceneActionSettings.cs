using Newtonsoft.Json;
using System.Collections.Generic;

namespace RGBJunkieDeckPlugin.Actions.Scenes
{
    internal sealed class SceneActionSettings
    {
        [JsonProperty("selectedSceneId")]
        public string SelectedSceneId { get; set; } = string.Empty;

        [JsonProperty("selectedSceneName")]
        public string SelectedSceneName { get; set; } = string.Empty;

        [JsonProperty("installedScenes")]
        public List<InstalledSceneSummary> InstalledScenes { get; set; } = new List<InstalledSceneSummary>();

        public static SceneActionSettings CreateDefault() => new SceneActionSettings();
    }
}
