using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json.Linq;
using RGBJunkieDeckPlugin.Helpers;
using System;
using System.Threading.Tasks;

namespace RGBJunkieDeckPlugin.Actions.Controls
{
    [PluginActionId("com.rgbjunkie.deck.cyclescenedial")]
    internal sealed class RgbJunkieCycleSceneDialEncoder : EncoderBase
    {
        private bool feedbackLayoutReady;

        public RgbJunkieCycleSceneDialEncoder(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            _ = UpdateDialFeedbackAsync();
        }

        public override void DialRotate(DialRotatePayload payload)
        {
            if (payload.Ticks == 0) return;
            var previous = payload.Ticks < 0;
            var steps = Math.Abs(payload.Ticks);
            for (var i = 0; i < steps; i++)
            {
                RgbJunkieUrlHelper.OpenUrlFast(RgbJunkieUrlHelper.BuildSceneCycleUrl(previous));
            }
            _ = UpdateDialFeedbackAsync();
        }

        public override void DialDown(DialPayload payload)
        {
        }

        public override void DialUp(DialPayload payload)
        {
        }

        public override void TouchPress(TouchpadPressPayload payload)
        {
        }

        public override void OnTick()
        {
            _ = UpdateDialFeedbackAsync();
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        {
        }

        public override void Dispose()
        {
        }

        private async Task UpdateDialFeedbackAsync()
        {
            try
            {
                if (!feedbackLayoutReady)
                {
                    await Connection.SetFeedbackLayoutAsync("$A1");
                    feedbackLayoutReady = true;
                }

                var label = BrightnessHelper.TryReadActiveSceneLabelFromDisk();
                if (string.IsNullOrWhiteSpace(label)) label = "Scene";
                var feedback = new JObject
                {
                    ["title"] = "Scene",
                    ["value"] = TruncateLabel(label, 18),
                };
                await Connection.SetFeedbackAsync(feedback);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Cycle scene dial feedback failed: {ex.Message}");
            }
        }

        private static string TruncateLabel(string text, int maxLen)
        {
            var t = (text ?? string.Empty).Trim();
            if (t.Length <= maxLen) return t;
            return t.Substring(0, Math.Max(0, maxLen - 1)) + "…";
        }
    }
}
