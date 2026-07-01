using System;
using System.IO;
using System.Text;

namespace RGBJunkieDeckPlugin.Helpers
{
    /// <summary>
    /// Drops deep links into RGBJunkie's AppData inbox so a running (elevated) instance
    /// can apply them without UAC — Stream Deck runs non-elevated and cannot relaunch the admin exe silently.
    /// </summary>
    internal static class DeepLinkIpc
    {
        private const string AliveFileName = ".alive";
        private const int AliveMaxAgeMs = 5000;

        private static string InboxDirectory
        {
            get
            {
                var overrideRoot = Environment.GetEnvironmentVariable("RGBJUNKIE_APP_DATA");
                if (!string.IsNullOrWhiteSpace(overrideRoot))
                {
                    return Path.Combine(overrideRoot.Trim(), "ipc", "deeplink");
                }
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "RGBJunkie",
                    "ipc",
                    "deeplink");
            }
        }

        public static bool IsAppAlive()
        {
            try
            {
                var alivePath = Path.Combine(InboxDirectory, AliveFileName);
                if (!File.Exists(alivePath)) return false;
                var text = File.ReadAllText(alivePath).Trim();
                if (!long.TryParse(text, out var writtenMs)) return false;
                var age = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - writtenMs;
                return age >= 0 && age < AliveMaxAgeMs;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryEnqueue(string url)
        {
            var u = (url ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(u) || !u.StartsWith("rgbjunkie://", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            try
            {
                var dir = InboxDirectory;
                Directory.CreateDirectory(dir);
                var file = Path.Combine(dir, $"{Guid.NewGuid():N}.url");
                File.WriteAllText(file, u, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
