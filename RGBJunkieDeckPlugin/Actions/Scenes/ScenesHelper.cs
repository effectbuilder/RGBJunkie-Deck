using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RGBJunkieDeckPlugin.Actions.Scenes
{
    internal sealed class InstalledSceneSummary
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    internal static class ScenesHelper
    {
        private static readonly Dictionary<string, InstalledSceneSummary> ScenesDatabase =
            new Dictionary<string, InstalledSceneSummary>(StringComparer.OrdinalIgnoreCase);

        internal static string ScenesDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RGBJunkie",
            "profiles",
            "scenes");

        internal static string ActiveSceneAutosavePath => Path.Combine(ScenesDirectory, "autosave_scene.json");

        /// <summary>Not user-saved scenes — internal RGBJunkie data files in <c>profiles/scenes</c>.</summary>
        private static readonly HashSet<string> SceneListSkipFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "autosave_scene.json",
            "session_profile_baseline.json",
            "user_color_profiles.json",
            "wled_devices.json",
        };

        private static bool IsSkippedSceneListFile(string fileName) =>
            !string.IsNullOrWhiteSpace(fileName) && SceneListSkipFiles.Contains(fileName);

        internal static IReadOnlyList<InstalledSceneSummary> SceneSummaries =>
            ScenesDatabase.Values.OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase).ToList();

        internal static void RefreshScenesDatabase()
        {
            ScenesDatabase.Clear();
            var dir = ScenesDirectory;
            if (!Directory.Exists(dir)) return;

            foreach (var file in Directory.EnumerateFiles(dir, "*.json", SearchOption.TopDirectoryOnly))
            {
                var fileName = Path.GetFileName(file);
                if (IsSkippedSceneListFile(fileName)) continue;
                var display = Path.GetFileNameWithoutExtension(fileName);
                if (string.IsNullOrWhiteSpace(display)) continue;
                ScenesDatabase[fileName] = new InstalledSceneSummary { Id = fileName, Name = display };
            }
        }

        internal static InstalledSceneSummary SceneLookup(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            ScenesDatabase.TryGetValue(id, out var scene);
            return scene;
        }

        internal static string ResolveSceneDisplayName(string sceneId, string fallbackName = "")
        {
            RefreshScenesDatabase();
            var picked = SceneLookup(sceneId);
            if (picked != null && !string.IsNullOrWhiteSpace(picked.Name))
                return picked.Name;
            return (fallbackName ?? string.Empty).Trim();
        }

        /// <summary>Read the scene RGBJunkie last had selected (updated after scene apply / prev-next).</summary>
        internal static InstalledSceneSummary TryGetActiveSceneFromDisk()
        {
            try
            {
                var path = ActiveSceneAutosavePath;
                if (!File.Exists(path)) return null;
                var activeFile = JObject.Parse(File.ReadAllText(path)).Value<string>("activeSceneProfileFile")?.Trim();
                if (string.IsNullOrEmpty(activeFile)) return null;
                if (!activeFile.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    activeFile += ".json";
                return SceneLookup(activeFile);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Could not read active scene from autosave: {ex.Message}");
                return null;
            }
        }
    }
}
