using BarRaider.SdTools;
using BarRaider.SdTools.Wrappers;
using RGBJunkieDeckPlugin.Helpers;

namespace RGBJunkieDeckPlugin.Actions.Controls
{
    [PluginActionId("com.rgbjunkie.deck.brightnessup")]
    internal sealed class RgbJunkieBrightnessUpAction : KeypadBase
    {
        public RgbJunkieBrightnessUpAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }

        public override void KeyReleased(KeyPayload payload)
        {
            RgbJunkieUrlHelper.OpenUrlFast(RgbJunkieUrlHelper.BuildBrightnessAdjustUrl(5));
        }

        public override void KeyPressed(KeyPayload payload)
        {
        }

        public override void OnTick()
        {
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
    }

    [PluginActionId("com.rgbjunkie.deck.brightnessdown")]
    internal sealed class RgbJunkieBrightnessDownAction : KeypadBase
    {
        public RgbJunkieBrightnessDownAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }

        public override void KeyReleased(KeyPayload payload)
        {
            RgbJunkieUrlHelper.OpenUrlFast(RgbJunkieUrlHelper.BuildBrightnessAdjustUrl(-5));
        }

        public override void KeyPressed(KeyPayload payload)
        {
        }

        public override void OnTick()
        {
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
    }
}
