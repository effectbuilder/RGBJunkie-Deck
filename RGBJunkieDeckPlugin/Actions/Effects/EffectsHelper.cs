using BarRaider.SdTools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RGBJunkieDeckPlugin.Actions.Effects
{
    internal sealed class InstalledEffectSummary
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    internal static class EffectsHelper
    {
        private static readonly Dictionary<string, InstalledEffectSummary> EffectsDatabase =
            new Dictionary<string, InstalledEffectSummary>(StringComparer.OrdinalIgnoreCase);

        internal static IReadOnlyList<InstalledEffectSummary> EffectSummaries =>
            EffectsDatabase.Values.OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase).ToList();

        internal static void RefreshEffectsDatabase()
        {
            EffectsDatabase.Clear();
            foreach (var root in GetEffectScanRoots())
            {
                if (!Directory.Exists(root)) continue;
                foreach (var file in Directory.EnumerateFiles(root, "*.html", SearchOption.AllDirectories))
                {
                    try
                    {
                        var html = File.ReadAllText(file);
                        var title = ExtractTitle(html, Path.GetFileNameWithoutExtension(file));
                        if (string.IsNullOrWhiteSpace(title)) continue;
                        var id = MakeStableId(file, title);
                        if (!EffectsDatabase.ContainsKey(id))
                        {
                            EffectsDatabase[id] = new InstalledEffectSummary { Id = id, Name = title };
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.LogMessage(TracingLevel.WARN, $"Skipped effect file {file}: {ex.Message}");
                    }
                }
            }
        }

        internal static InstalledEffectSummary EffectLookup(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            EffectsDatabase.TryGetValue(id, out var effect);
            return effect;
        }

        internal static IEnumerable<string> GetEffectScanRoots()
        {
            var appDataEffects = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RGBJunkie",
                "effects");
            yield return appDataEffects;

            foreach (var programRoot in new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "RGBJunkie", "effects"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "RGBJunkie", "effects"),
            })
            {
                yield return programRoot;
            }
        }

        private static string MakeStableId(string filePath, string title)
        {
            var fileKey = filePath.ToLowerInvariant();
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(fileKey));
                var shortHash = BitConverter.ToString(hash).Replace("-", string.Empty).Substring(0, 12);
                return $"{title}|{shortHash}";
            }
        }

        private static string ExtractTitle(string html, string fallback)
        {
            if (string.IsNullOrEmpty(html)) return fallback;
            var titleMatch = Regex.Match(html, "<title[^>]*>([^<]+)</title>", RegexOptions.IgnoreCase);
            if (titleMatch.Success)
            {
                var extracted = titleMatch.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(extracted)) return extracted;
            }

            foreach (var pattern in new[] { "property=\"title\"", "name=\"title\"", "name=\"name\"" })
            {
                var metaIdx = html.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
                if (metaIdx < 0) continue;
                var contentIdx = html.IndexOf("content=\"", metaIdx, StringComparison.OrdinalIgnoreCase);
                if (contentIdx < 0) continue;
                var valStart = contentIdx + 9;
                var valEnd = html.IndexOf('"', valStart);
                if (valEnd <= valStart) continue;
                var value = html.Substring(valStart, valEnd - valStart).Trim();
                if (!string.IsNullOrEmpty(value)) return value;
            }

            return fallback;
        }
    }
}
