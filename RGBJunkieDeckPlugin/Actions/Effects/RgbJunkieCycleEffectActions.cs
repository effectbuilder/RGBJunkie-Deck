using BarRaider.SdTools;

using BarRaider.SdTools.Wrappers;

using RGBJunkieDeckPlugin.Helpers;

using System.Linq;

using System.Threading.Tasks;



namespace RGBJunkieDeckPlugin.Actions.Effects

{

    internal abstract class RgbJunkieCycleEffectActionBase : RgbJunkieKeypadBase

    {

        protected readonly CycleEffectActionSettings settings;

        private readonly bool previous;



        protected RgbJunkieCycleEffectActionBase(ISDConnection connection, InitialPayload payload, bool previous)

            : base(connection, payload)

        {

            this.previous = previous;

            settings = payload.Settings == null || payload.Settings.Count == 0

                ? CycleEffectActionSettings.CreateDefault()

                : payload.Settings.ToObject<CycleEffectActionSettings>();



            Connection.OnPropertyInspectorDidAppear += Connection_OnPropertyInspectorDidAppear;

            Connection.OnSendToPlugin += Connection_OnSendToPlugin;

            RefreshWorkspaceLists();

        }



        public override void Dispose()

        {

            Connection.OnPropertyInspectorDidAppear -= Connection_OnPropertyInspectorDidAppear;

            Connection.OnSendToPlugin -= Connection_OnSendToPlugin;

        }



        public override string[] ApplicationUrls => new[]

        {

            RgbJunkieUrlHelper.BuildEffectCycleUrl(previous, settings.SelectedWorkspaceId)

        };



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



        public override void ReceivedSettings(ReceivedSettingsPayload payload)

        {

            Tools.AutoPopulateSettings(settings, payload.Settings);

            SaveSettings();

        }



        private async Task SaveSettings()

        {

            var json = DeckSettingsJson.ToJObject(settings);

            await Connection.SetSettingsAsync(json);

            await Connection.SendToPropertyInspectorAsync(json);

        }

    }



    [PluginActionId("com.rgbjunkie.deck.previouseffect")]

    internal sealed class RgbJunkiePreviousEffectAction : RgbJunkieCycleEffectActionBase

    {

        public RgbJunkiePreviousEffectAction(ISDConnection connection, InitialPayload payload)

            : base(connection, payload, previous: true)

        {

        }

    }



    [PluginActionId("com.rgbjunkie.deck.nexteffect")]

    internal sealed class RgbJunkieNextEffectAction : RgbJunkieCycleEffectActionBase

    {

        public RgbJunkieNextEffectAction(ISDConnection connection, InitialPayload payload)

            : base(connection, payload, previous: false)

        {

        }

    }

}


