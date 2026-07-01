using BarRaider.SdTools;
using BarRaider.SdTools.Wrappers;
using RGBJunkieDeckPlugin.Helpers;
using System.Linq;
using System.Threading.Tasks;

namespace RGBJunkieDeckPlugin.Actions.Scenes
{
    [PluginActionId("com.rgbjunkie.deck.scene")]
    internal sealed class RgbJunkieSceneAction : RgbJunkieKeypadBase
    {
        private readonly SceneActionSettings settings;

        public RgbJunkieSceneAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            settings = payload.Settings == null || payload.Settings.Count == 0
                ? SceneActionSettings.CreateDefault()
                : payload.Settings.ToObject<SceneActionSettings>();

            Connection.OnPropertyInspectorDidAppear += Connection_OnPropertyInspectorDidAppear;
            Connection.OnSendToPlugin += Connection_OnSendToPlugin;
        }

        public override void Dispose()
        {
            Connection.OnPropertyInspectorDidAppear -= Connection_OnPropertyInspectorDidAppear;
            Connection.OnSendToPlugin -= Connection_OnSendToPlugin;
        }

        public override string[] ApplicationUrls
        {
            get
            {
                var url = RgbJunkieUrlHelper.BuildSceneApplyUrl(settings.SelectedSceneId);
                return string.IsNullOrEmpty(url) ? new string[0] : new[] { url };
            }
        }

        public override void KeyReleased(KeyPayload payload)
        {
            ScenesHelper.RefreshScenesDatabase();
            SyncSelectedSceneNameFromId();
            if (string.IsNullOrWhiteSpace(settings.SelectedSceneId))
            {
                Logger.Instance.LogMessage(
                    TracingLevel.WARN,
                    "RGBJunkie Scene: pick a saved scene in the property inspector first.");
                return;
            }

            var url = RgbJunkieUrlHelper.BuildSceneApplyUrl(settings.SelectedSceneId);
            if (!string.IsNullOrEmpty(url))
                RgbJunkieUrlHelper.OpenUrls(new[] { url });
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
            ScenesHelper.RefreshScenesDatabase();
            settings.InstalledScenes = ScenesHelper.SceneSummaries.ToList();
            SyncSelectedSceneNameFromId();
            SaveSettings();
        }

        private void SyncSelectedSceneNameFromId()
        {
            var name = ScenesHelper.ResolveSceneDisplayName(
                settings.SelectedSceneId,
                settings.SelectedSceneName);
            if (!string.IsNullOrEmpty(name))
                settings.SelectedSceneName = name;
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            SyncSelectedSceneNameFromId();
            SaveSettings();
        }

        private async Task SaveSettings()
        {
            var json = DeckSettingsJson.ToJObject(settings);
            await Connection.SetSettingsAsync(json);
            await Connection.SendToPropertyInspectorAsync(json);
        }
    }
}
