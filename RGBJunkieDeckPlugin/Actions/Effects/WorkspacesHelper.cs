using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RGBJunkieDeckPlugin.Actions.Effects
{
    internal sealed class InstalledWorkspaceSummary
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
    }

    internal static class WorkspacesHelper
    {
        private static readonly List<InstalledWorkspaceSummary> WorkspacesDatabase = new List<InstalledWorkspaceSummary>();

        internal static string DeviceLayoutPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RGBJunkie",
            "profiles",
            "devices",
            "autosave_device.json");

        internal static IReadOnlyList<InstalledWorkspaceSummary> WorkspaceSummaries => WorkspacesDatabase.ToList();

        internal static void RefreshWorkspacesDatabase()
        {
            WorkspacesDatabase.Clear();
            WorkspacesDatabase.Add(new InstalledWorkspaceSummary
            {
                Id = string.Empty,
                Name = "Selected canvas"
            });

            var path = DeviceLayoutPath;
            if (!File.Exists(path))
            {
                WorkspacesDatabase.Add(new InstalledWorkspaceSummary { Id = "main", Name = "Main [1]" });
                return;
            }

            try
            {
                var json = JObject.Parse(File.ReadAllText(path));
                var tabs = json["workspaceTabs"] as JArray;
                if (tabs == null || tabs.Count == 0)
                {
                    WorkspacesDatabase.Add(new InstalledWorkspaceSummary { Id = "main", Name = "Main [1]" });
                    return;
                }

                for (var i = 0; i < tabs.Count; i++)
                {
                    var tab = tabs[i] as JObject;
                    if (tab == null) continue;
                    var id = tab.Value<string>("id")?.Trim();
                    if (string.IsNullOrEmpty(id)) continue;
                    var label = tab.Value<string>("label")?.Trim();
                    WorkspacesDatabase.Add(new InstalledWorkspaceSummary
                    {
                        Id = id,
                        Name = FormatTabLabel(label, i)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Could not read canvas tabs from {path}: {ex.Message}");
                WorkspacesDatabase.Add(new InstalledWorkspaceSummary { Id = "main", Name = "Main [1]" });
            }
        }

        internal static InstalledWorkspaceSummary WorkspaceLookup(string id)
        {
            if (id == null) return WorkspacesDatabase.FirstOrDefault();
            return WorkspacesDatabase.FirstOrDefault(w => string.Equals(w.Id, id, StringComparison.Ordinal));
        }

        private static string FormatTabLabel(string label, int tabIndex)
        {
            var baseLabel = string.IsNullOrWhiteSpace(label) ? "?" : label.Trim();
            var slot = tabIndex + 1;
            if (slot > 9 || tabIndex < 0) return baseLabel;
            return $"{baseLabel} [{slot}]";
        }
    }
}
