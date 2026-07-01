using Newtonsoft.Json;
using System.Collections.Generic;

namespace RGBJunkieDeckPlugin.Actions.Effects
{
    internal sealed class CycleEffectActionSettings
    {
        [JsonProperty("selectedWorkspaceId")]
        public string SelectedWorkspaceId { get; set; } = string.Empty;

        [JsonProperty("installedWorkspaces")]
        public List<InstalledWorkspaceSummary> InstalledWorkspaces { get; set; } = new List<InstalledWorkspaceSummary>();

        public static CycleEffectActionSettings CreateDefault() => new CycleEffectActionSettings();
    }
}
