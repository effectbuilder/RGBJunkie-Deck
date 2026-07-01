using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using BarRaider.SdTools.Wrappers;
using Newtonsoft.Json.Linq;
using RGBJunkieDeckPlugin.Actions.Effects;
using RGBJunkieDeckPlugin.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RGBJunkieDeckPlugin.Actions.Controls
{
    [PluginActionId("com.rgbjunkie.deck.cycleeffectdial")]
    internal sealed class RgbJunkieCycleEffectDialEncoder : EncoderBase
    {
        private readonly CycleEffectActionSettings settings;
        private bool feedbackLayoutReady;

        public RgbJunkieCycleEffectDialEncoder(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            settings = payload.Settings == null || payload.Settings.Count == 0
                ? CycleEffectActionSettings.CreateDefault()
                : payload.Settings.ToObject<CycleEffectActionSettings>();

            Connection.OnPropertyInspectorDidAppear += Connection_OnPropertyInspectorDidAppear;
            Connection.OnSendToPlugin += Connection_OnSendToPlugin;
            RefreshWorkspaceLists();
            _ = UpdateDialFeedbackAsync();
        }

        public override void DialRotate(DialRotatePayload payload)
        {
            if (payload.Ticks == 0) return;
            var previous = payload.Ticks < 0;
            var steps = Math.Abs(payload.Ticks);
            for (var i = 0; i < steps; i++)
            {
                RgbJunkieUrlHelper.OpenUrlFast(
                    RgbJunkieUrlHelper.BuildEffectCycleUrl(previous, settings.SelectedWorkspaceId));
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
            Tools.AutoPopulateSettings(settings, payload.Settings);
            RefreshWorkspaceLists();
            _ = SaveSettingsAsync();
            _ = UpdateDialFeedbackAsync();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        {
        }

        public override void Dispose()
        {
            Connection.OnPropertyInspectorDidAppear -= Connection_OnPropertyInspectorDidAppear;
            Connection.OnSendToPlugin -= Connection_OnSendToPlugin;
        }

        private void Connection_OnPropertyInspectorDidAppear(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.PropertyInspectorDidAppear> e)
        {
            RefreshInspectorLists();
        }

        private void Connection_OnSendToPlugin(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.SendToPlugin> e)
        {
            if (e?.Event?.Payload?["property_inspector"]?.ToString() != "propertyInspectorConnected") return;
            RefreshInspectorLists();
        }

        private void RefreshInspectorLists()
        {
            RefreshWorkspaceLists();
            SaveSettings();
        }

        private void RefreshWorkspaceLists()
        {
            WorkspacesHelper.RefreshWorkspacesDatabase();
            settings.InstalledWorkspaces = WorkspacesHelper.WorkspaceSummaries.ToList();
        }

        private async Task SaveSettingsAsync()
        {
            var json = DeckSettingsJson.ToJObject(settings);
            await Connection.SetSettingsAsync(json);
            await Connection.SendToPropertyInspectorAsync(json);
        }

        private void SaveSettings()
        {
            _ = SaveSettingsAsync();
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

                var label = BrightnessHelper.TryReadActiveEffectLabelFromDisk();
                if (string.IsNullOrWhiteSpace(label)) label = "Effect";
                var feedback = new JObject
                {
                    ["title"] = "Effect",
                    ["value"] = TruncateLabel(label, 18),
                };
                await Connection.SetFeedbackAsync(feedback);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Cycle effect dial feedback failed: {ex.Message}");
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
