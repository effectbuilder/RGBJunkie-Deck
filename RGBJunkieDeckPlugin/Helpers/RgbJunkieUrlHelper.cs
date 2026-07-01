using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using BarRaider.SdTools;

namespace RGBJunkieDeckPlugin.Helpers
{
    internal static class RgbJunkieUrlHelper
    {
        public const string SilentQuery = "silent=1";

        public static void OpenUrls(IEnumerable<string> urls)
        {
            foreach (var url in urls.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                OpenUrlFast(url);
            }
        }

        /// <summary>Enqueue or launch one deep link — minimal delay for dial rotation.</summary>
        public static void OpenUrlFast(string url)
        {
            var u = (url ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(u)) return;
            try
            {
                if (DeepLinkIpc.IsAppAlive() && DeepLinkIpc.TryEnqueue(u))
                {
                    return;
                }

                Process.Start(new ProcessStartInfo(u) { UseShellExecute = true });
                System.Threading.Thread.Sleep(350);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Failed to open \"{u}\": {ex.Message}");
            }
        }

        public static string BuildEffectApplyUrl(string effectName, string workspaceId = "", bool silent = true)
        {
            var name = (effectName ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(name)) return string.Empty;
            var url = new StringBuilder();
            url.Append("rgbjunkie://effect/apply/");
            url.Append(Uri.EscapeDataString(name));
            return AppendSilentAndWorkspace(url.ToString(), workspaceId, silent);
        }

        /// <summary>Build scene apply URL from the saved scene filename (e.g. <c>Fire.json</c>).</summary>
        public static string BuildSceneApplyUrl(string sceneFileId, bool silent = true)
        {
            var fileId = (sceneFileId ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(fileId)) return string.Empty;
            if (!fileId.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                fileId += ".json";
            var display = System.IO.Path.GetFileNameWithoutExtension(fileId);
            if (string.IsNullOrEmpty(display)) return string.Empty;

            var qs = new List<string>();
            if (silent) qs.Add(SilentQuery);
            qs.Add($"sceneFile={Uri.EscapeDataString(fileId)}");

            return $"rgbjunkie://scene/apply/{Uri.EscapeDataString(display)}?{string.Join("&", qs)}";
        }

        public static string BuildEffectCycleUrl(bool previous, string workspaceId = "", bool silent = true)
        {
            var url = previous ? "rgbjunkie://effect/applyprevious" : "rgbjunkie://effect/applynext";
            return AppendSilentAndWorkspace(url, workspaceId, silent);
        }

        public static string BuildSceneCycleUrl(bool previous, bool silent = true)
        {
            var url = previous ? "rgbjunkie://scene/applyprevious" : "rgbjunkie://scene/applynext";
            return silent ? $"{url}?{SilentQuery}" : url;
        }

        public static string BuildViewUrl(string viewPath, bool silent = true)
        {
            var path = (viewPath ?? string.Empty).Trim().Trim('/');
            if (string.IsNullOrEmpty(path)) path = "overview";
            var url = $"rgbjunkie://view/{path}";
            return silent ? $"{url}?{SilentQuery}" : url;
        }

        public static string BuildAppRestartUrl(bool silent = true)
        {
            var url = "rgbjunkie://app/restart";
            return silent ? $"{url}?{SilentQuery}" : url;
        }

        public static string BuildBrightnessSetUrl(int percent, bool silent = true)
        {
            var pct = Math.Max(0, Math.Min(100, percent));
            var url = $"rgbjunkie://brightness/set/{pct}";
            return silent ? $"{url}?{SilentQuery}" : url;
        }

        public static string BuildBrightnessAdjustUrl(int deltaPercent, bool silent = true)
        {
            var delta = Math.Max(-100, Math.Min(100, deltaPercent));
            if (delta == 0) return string.Empty;
            var sign = delta > 0 ? "+" : string.Empty;
            var url = $"rgbjunkie://brightness/adjust?delta={sign}{delta}";
            return silent ? $"{url}&{SilentQuery}" : url;
        }

        public static string BuildEffectTogglePauseUrl(bool silent = true)
        {
            var url = "rgbjunkie://effect/togglepause";
            return silent ? $"{url}?{SilentQuery}" : url;
        }

        public static string BuildLightsToggleUrl(bool silent = true)
        {
            var url = "rgbjunkie://lights/toggle";
            return silent ? $"{url}?{SilentQuery}" : url;
        }

        public static string BuildOpenAppDataUrl(string subPath = "", bool silent = true)
        {
            var sub = (subPath ?? string.Empty).Trim().Trim('/');
            var url = string.IsNullOrEmpty(sub)
                ? "rgbjunkie://open/appdata"
                : $"rgbjunkie://open/appdata/{sub}";
            return silent ? $"{url}?{SilentQuery}" : url;
        }

        private static string AppendSilentAndWorkspace(string url, string workspaceId, bool silent)
        {
            var qs = new List<string>();
            if (silent) qs.Add(SilentQuery);
            var ws = (workspaceId ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(ws)) qs.Add($"workspace={Uri.EscapeDataString(ws)}");
            if (qs.Count == 0) return url;
            return $"{url}?{string.Join("&", qs)}";
        }
    }
}
