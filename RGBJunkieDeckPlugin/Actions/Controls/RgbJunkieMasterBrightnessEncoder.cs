using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json.Linq;
using RGBJunkieDeckPlugin.Helpers;
using System;
using System.Threading.Tasks;

namespace RGBJunkieDeckPlugin.Actions.Controls
{
    [PluginActionId("com.rgbjunkie.deck.masterbrightness")]
    internal sealed class RgbJunkieMasterBrightnessEncoder : EncoderBase
    {
        private readonly MasterBrightnessActionSettings settings;
        private bool feedbackLayoutReady;

        public RgbJunkieMasterBrightnessEncoder(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            settings = payload.Settings == null || payload.Settings.Count == 0
                ? MasterBrightnessActionSettings.CreateDefault()
                : payload.Settings.ToObject<MasterBrightnessActionSettings>();
            SeedDisplayFromDisk();
            _ = UpdateDialFeedbackAsync(settings.DisplayPercent);
        }

        public override void DialRotate(DialRotatePayload payload)
        {
            var step = Math.Max(1, Math.Min(20, settings.StepPercent));
            var delta = payload.Ticks * step;
            if (delta == 0) return;

            settings.DisplayPercent = Math.Max(0, Math.Min(100, settings.DisplayPercent + delta));
            _ = UpdateDialFeedbackAsync(settings.DisplayPercent);
            RgbJunkieUrlHelper.OpenUrlFast(RgbJunkieUrlHelper.BuildBrightnessAdjustUrl(delta));
        }

        public override void DialDown(DialPayload payload)
        {
            settings.DisplayPercent = 100;
            _ = UpdateDialFeedbackAsync(100);
            RgbJunkieUrlHelper.OpenUrlFast(RgbJunkieUrlHelper.BuildBrightnessSetUrl(100));
        }

        public override void DialUp(DialPayload payload)
        {
        }

        public override void TouchPress(TouchpadPressPayload payload)
        {
            settings.DisplayPercent = 100;
            _ = UpdateDialFeedbackAsync(100);
            RgbJunkieUrlHelper.OpenUrlFast(RgbJunkieUrlHelper.BuildBrightnessSetUrl(100));
        }

        public override void OnTick()
        {
            var fromDisk = BrightnessHelper.TryReadMasterBrightnessPercentFromDisk();
            if (!fromDisk.HasValue) return;
            if (fromDisk.Value == settings.DisplayPercent) return;
            settings.DisplayPercent = fromDisk.Value;
            _ = UpdateDialFeedbackAsync(fromDisk.Value);
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            if (settings.StepPercent < 1) settings.StepPercent = 1;
            if (settings.StepPercent > 20) settings.StepPercent = 20;
            SeedDisplayFromDisk();
            _ = SaveSettingsAsync();
            _ = UpdateDialFeedbackAsync(settings.DisplayPercent);
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        {
        }

        public override void Dispose()
        {
        }

        private void SeedDisplayFromDisk()
        {
            var fromDisk = BrightnessHelper.TryReadMasterBrightnessPercentFromDisk();
            if (fromDisk.HasValue)
                settings.DisplayPercent = fromDisk.Value;
            else if (settings.DisplayPercent < 0 || settings.DisplayPercent > 100)
                settings.DisplayPercent = 100;
        }

        private async Task SaveSettingsAsync()
        {
            await Connection.SetSettingsAsync(DeckSettingsJson.ToJObject(settings));
        }

        private async Task UpdateDialFeedbackAsync(int percent)
        {
            try
            {
                if (!feedbackLayoutReady)
                {
                    await Connection.SetFeedbackLayoutAsync("$B1");
                    feedbackLayoutReady = true;
                }

                var pct = Math.Max(0, Math.Min(100, percent));
                var feedback = new JObject
                {
                    ["title"] = "Brightness",
                    ["value"] = $"{pct}%",
                    ["indicator"] = pct,
                };
                await Connection.SetFeedbackAsync(feedback);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Brightness dial feedback failed: {ex.Message}");
            }
        }
    }
}
