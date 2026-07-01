using BarRaider.SdTools;
using RGBJunkieDeckPlugin.Helpers;

namespace RGBJunkieDeckPlugin.Actions.Views
{
    internal abstract class RgbJunkieViewActionBase : RgbJunkieKeypadBase
    {
        private readonly string viewPath;

        protected RgbJunkieViewActionBase(ISDConnection connection, InitialPayload payload, string viewPath)
            : base(connection, payload)
        {
            this.viewPath = viewPath;
        }

        public override string[] ApplicationUrls => new[] { RgbJunkieUrlHelper.BuildViewUrl(viewPath) };
    }

    [PluginActionId("com.rgbjunkie.deck.vieweffects")]
    internal sealed class RgbJunkieEffectsViewAction : RgbJunkieViewActionBase
    {
        public RgbJunkieEffectsViewAction(ISDConnection connection, InitialPayload payload)
            : base(connection, payload, "effects") { }
    }

    [PluginActionId("com.rgbjunkie.deck.viewhardware")]
    internal sealed class RgbJunkieHardwareSettingsViewAction : RgbJunkieViewActionBase
    {
        public RgbJunkieHardwareSettingsViewAction(ISDConnection connection, InitialPayload payload)
            : base(connection, payload, "settings/hardware") { }
    }

    [PluginActionId("com.rgbjunkie.deck.viewinstalled")]
    internal sealed class RgbJunkieInstalledSettingsViewAction : RgbJunkieViewActionBase
    {
        public RgbJunkieInstalledSettingsViewAction(ISDConnection connection, InitialPayload payload)
            : base(connection, payload, "settings/installed") { }
    }

    [PluginActionId("com.rgbjunkie.deck.viewlogs")]
    internal sealed class RgbJunkieLogsViewAction : RgbJunkieViewActionBase
    {
        public RgbJunkieLogsViewAction(ISDConnection connection, InitialPayload payload)
            : base(connection, payload, "logs") { }
    }

    [PluginActionId("com.rgbjunkie.deck.openplugins")]
    internal sealed class RgbJunkieOpenPluginsFolderAction : RgbJunkieKeypadBase
    {
        public RgbJunkieOpenPluginsFolderAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }

        public override string[] ApplicationUrls => new[] { RgbJunkieUrlHelper.BuildOpenAppDataUrl("plugins") };
    }

    [PluginActionId("com.rgbjunkie.deck.restart")]
    internal sealed class RgbJunkieRestartAction : RgbJunkieKeypadBase
    {
        public RgbJunkieRestartAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }

        public override string[] ApplicationUrls => new[] { RgbJunkieUrlHelper.BuildAppRestartUrl() };
    }
}
