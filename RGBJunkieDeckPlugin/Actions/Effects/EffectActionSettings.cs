using Newtonsoft.Json;
using System.Collections.Generic;

namespace RGBJunkieDeckPlugin.Actions.Effects
{
    internal sealed class EffectActionSettings
    {
        [JsonProperty("selectedEffectId")]
        public string SelectedEffectId { get; set; } = string.Empty;

        [JsonProperty("selectedEffectName")]
        public string SelectedEffectName { get; set; } = string.Empty;

        [JsonProperty("selectedWorkspaceId")]
        public string SelectedWorkspaceId { get; set; } = string.Empty;

        [JsonProperty("installedEffects")]
        public List<InstalledEffectSummary> InstalledEffects { get; set; } = new List<InstalledEffectSummary>();

        [JsonProperty("installedWorkspaces")]
        public List<InstalledWorkspaceSummary> InstalledWorkspaces { get; set; } = new List<InstalledWorkspaceSummary>();

        public static EffectActionSettings CreateDefault() => new EffectActionSettings();
    }
}
