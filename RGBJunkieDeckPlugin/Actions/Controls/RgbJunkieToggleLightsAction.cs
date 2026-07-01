using BarRaider.SdTools;
using BarRaider.SdTools.Wrappers;
using RGBJunkieDeckPlugin.Helpers;
using System.Threading.Tasks;

namespace RGBJunkieDeckPlugin.Actions.Controls
{
    [PluginActionId("com.rgbjunkie.deck.togglelights")]
    internal sealed class RgbJunkieToggleLightsAction : KeypadBase
    {
        private bool lightsOff;

        public RgbJunkieToggleLightsAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            SeedFromDisk();
            _ = UpdateKeyStateAsync();
        }

        public override void KeyReleased(KeyPayload payload)
        {
            lightsOff = !lightsOff;
            _ = UpdateKeyStateAsync();
            RgbJunkieUrlHelper.OpenUrlFast(RgbJunkieUrlHelper.BuildLightsToggleUrl());
        }

        public override void KeyPressed(KeyPayload payload)
        {
        }

        public override void OnTick()
        {
            var fromDisk = BrightnessHelper.TryReadDeskLightsBlackoutFromDisk();
            if (!fromDisk.HasValue || fromDisk.Value == lightsOff) return;
            lightsOff = fromDisk.Value;
            _ = UpdateKeyStateAsync();
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

        private void SeedFromDisk()
        {
            var fromDisk = BrightnessHelper.TryReadDeskLightsBlackoutFromDisk();
            if (fromDisk.HasValue)
                lightsOff = fromDisk.Value;
        }

        private async Task UpdateKeyStateAsync()
        {
            try
            {
                await Connection.SetStateAsync(lightsOff ? 1u : 0u);
            }
            catch
            {
                // Stream Deck may be disconnecting.
            }
        }
    }
}
