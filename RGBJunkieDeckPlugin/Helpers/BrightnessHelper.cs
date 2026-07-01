using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace RGBJunkieDeckPlugin.Helpers
{
    internal static class BrightnessHelper
    {
        private static string AutosaveScenePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RGBJunkie",
            "profiles",
            "scenes",
            "autosave_scene.json");

        private static JObject TryReadAutosaveRoot()
        {
            try
            {
                var path = AutosaveScenePath;
                if (!File.Exists(path)) return null;
                return JObject.Parse(File.ReadAllText(path));
            }
            catch
            {
                return null;
            }
        }

        internal static int? TryReadMasterBrightnessPercentFromDisk()
        {
            var root = TryReadAutosaveRoot();
            var token = root?["masterBrightness"];
            if (token == null || token.Type == JTokenType.Null) return null;
            var fraction = token.Value<double>();
            if (!double.IsFinite(fraction)) return null;
            return Math.Max(0, Math.Min(100, (int)Math.Round(fraction * 100.0)));
        }

        internal static bool? TryReadEffectPausedFromDisk()
        {
            var root = TryReadAutosaveRoot();
            var token = root?["effectPaused"];
            if (token == null || token.Type == JTokenType.Null) return null;
            return token.Value<bool>();
        }

        internal static bool? TryReadDeskLightsBlackoutFromDisk()
        {
            var root = TryReadAutosaveRoot();
            var token = root?["deskLightsBlackout"];
            if (token == null || token.Type == JTokenType.Null) return null;
            return token.Value<bool>();
        }

        internal static string TryReadActiveEffectLabelFromDisk()
        {
            var root = TryReadAutosaveRoot();
            if (root == null) return string.Empty;
            var display = root.Value<string>("activeEffectDisplayName")?.Trim();
            if (!string.IsNullOrEmpty(display)) return display;
            var mode = root.Value<string>("lightingMode")?.Trim() ?? string.Empty;
            if (mode.StartsWith("effect_", StringComparison.OrdinalIgnoreCase))
                return "Effect " + mode.Substring("effect_".Length);
            return string.Empty;
        }

        internal static string TryReadActiveSceneLabelFromDisk()
        {
            var root = TryReadAutosaveRoot();
            if (root == null) return string.Empty;
            var file = root.Value<string>("activeSceneProfileFile")?.Trim();
            if (string.IsNullOrEmpty(file)) return string.Empty;
            if (file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                return Path.GetFileNameWithoutExtension(file);
            return file;
        }
    }
}
