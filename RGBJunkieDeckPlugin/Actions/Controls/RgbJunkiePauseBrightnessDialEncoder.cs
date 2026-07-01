using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json.Linq;
using RGBJunkieDeckPlugin.Helpers;
using System;
using System.Threading.Tasks;

namespace RGBJunkieDeckPlugin.Actions.Controls
{
    [PluginActionId("com.rgbjunkie.deck.pausebrightnessdial")]
    internal sealed class RgbJunkiePauseBrightnessDialEncoder : EncoderBase
    {
        private readonly MasterBrightnessActionSettings settings;
        private bool feedbackLayoutReady;
        private bool displayPaused;

        public RgbJunkiePauseBrightnessDialEncoder(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            settings = payload.Settings == null || payload.Settings.Count == 0
                ? MasterBrightnessActionSettings.CreateDefault()
                : payload.Settings.ToObject<MasterBrightnessActionSettings>();
            SeedFromDisk();
            _ = UpdateDialFeedbackAsync();
        }

        public override void DialRotate(DialRotatePayload payload)
        {
            var step = Math.Max(1, Math.Min(20, settings.StepPercent));
            var delta = payload.Ticks * step;
            if (delta == 0) return;

            settings.DisplayPercent = Math.Max(0, Math.Min(100, settings.DisplayPercent + delta));
            _ = UpdateDialFeedbackAsync();
            RgbJunkieUrlHelper.OpenUrlFast(RgbJunkieUrlHelper.BuildBrightnessAdjustUrl(delta));
        }

        public override void DialDown(DialPayload payload)
        {
            TogglePause();
        }

        public override void DialUp(DialPayload payload)
        {
        }

        public override void TouchPress(TouchpadPressPayload payload)
        {
            TogglePause();
        }

        public override void OnTick()
        {
            var fromDiskPaused = BrightnessHelper.TryReadEffectPausedFromDisk();
            var fromDiskBrightness = BrightnessHelper.TryReadMasterBrightnessPercentFromDisk();
            var changed = false;
            if (fromDiskPaused.HasValue && fromDiskPaused.Value != displayPaused)
            {
                displayPaused = fromDiskPaused.Value;
                changed = true;
            }
            if (fromDiskBrightness.HasValue && fromDiskBrightness.Value != settings.DisplayPercent)
            {
                settings.DisplayPercent = fromDiskBrightness.Value;
                changed = true;
            }
            if (changed) _ = UpdateDialFeedbackAsync();
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            if (settings.StepPercent < 1) settings.StepPercent = 1;
            if (settings.StepPercent > 20) settings.StepPercent = 20;
            SeedFromDisk();
            _ = SaveSettingsAsync();
            _ = UpdateDialFeedbackAsync();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        {
        }

        public override void Dispose()
        {
        }

        private void TogglePause()
        {
            displayPaused = !displayPaused;
            _ = UpdateDialFeedbackAsync();
            RgbJunkieUrlHelper.OpenUrlFast(RgbJunkieUrlHelper.BuildEffectTogglePauseUrl());
        }

        private void SeedFromDisk()
        {
            var fromDisk = BrightnessHelper.TryReadMasterBrightnessPercentFromDisk();
            if (fromDisk.HasValue)
                settings.DisplayPercent = fromDisk.Value;
            else if (settings.DisplayPercent < 0 || settings.DisplayPercent > 100)
                settings.DisplayPercent = 100;

            var paused = BrightnessHelper.TryReadEffectPausedFromDisk();
            if (paused.HasValue)
                displayPaused = paused.Value;
        }

        private async Task SaveSettingsAsync()
        {
            await Connection.SetSettingsAsync(DeckSettingsJson.ToJObject(settings));
        }

        private async Task UpdateDialFeedbackAsync()
        {
            try
            {
                if (!feedbackLayoutReady)
                {
                    await Connection.SetFeedbackLayoutAsync("$B1");
                    feedbackLayoutReady = true;
                }

                var pct = Math.Max(0, Math.Min(100, settings.DisplayPercent));
                var value = displayPaused ? "Paused" : $"{pct}%";
                var indicator = displayPaused ? 0 : pct;
                var feedback = new JObject
                {
                    ["title"] = "Lights",
                    ["value"] = value,
                    ["indicator"] = indicator,
                };
                await Connection.SetFeedbackAsync(feedback);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Pause/brightness dial feedback failed: {ex.Message}");
            }
        }
    }
}
