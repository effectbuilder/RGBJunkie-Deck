using BarRaider.SdTools;

using BarRaider.SdTools.Wrappers;

using RGBJunkieDeckPlugin.Helpers;

using System.Linq;

using System.Threading.Tasks;



namespace RGBJunkieDeckPlugin.Actions.Effects

{

    [PluginActionId("com.rgbjunkie.deck.effect")]

    internal sealed class RgbJunkieEffectAction : RgbJunkieKeypadBase

    {

        private readonly EffectActionSettings settings;



        public RgbJunkieEffectAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)

        {

            settings = payload.Settings == null || payload.Settings.Count == 0

                ? EffectActionSettings.CreateDefault()

                : payload.Settings.ToObject<EffectActionSettings>();



            Connection.OnPropertyInspectorDidAppear += Connection_OnPropertyInspectorDidAppear;

            Connection.OnSendToPlugin += Connection_OnSendToPlugin;

            RefreshWorkspaceLists();

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

                EffectsHelper.RefreshEffectsDatabase();
                var picked = EffectsHelper.EffectLookup(settings.SelectedEffectId);
                var effectName = picked?.Name ?? settings.SelectedEffectName;
                var url = RgbJunkieUrlHelper.BuildEffectApplyUrl(
                    effectName,
                    settings.SelectedWorkspaceId);

                return string.IsNullOrEmpty(url) ? new string[0] : new[] { url };

            }

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

            EffectsHelper.RefreshEffectsDatabase();

            settings.InstalledEffects = EffectsHelper.EffectSummaries.ToList();

            RefreshWorkspaceLists();

            SaveSettings();

        }



        private void RefreshWorkspaceLists()

        {

            WorkspacesHelper.RefreshWorkspacesDatabase();

            settings.InstalledWorkspaces = WorkspacesHelper.WorkspaceSummaries.ToList();

        }



        public override void ReceivedSettings(ReceivedSettingsPayload payload)

        {

            Tools.AutoPopulateSettings(settings, payload.Settings);

            EffectsHelper.RefreshEffectsDatabase();
            var picked = EffectsHelper.EffectLookup(settings.SelectedEffectId);

            if (picked != null) settings.SelectedEffectName = picked.Name;

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


