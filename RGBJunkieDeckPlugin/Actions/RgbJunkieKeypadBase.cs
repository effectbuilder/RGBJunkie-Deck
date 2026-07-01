using BarRaider.SdTools;
using BarRaider.SdTools.Wrappers;
using RGBJunkieDeckPlugin.Helpers;
using System;
using System.Linq;

namespace RGBJunkieDeckPlugin.Actions
{
    internal abstract class RgbJunkieKeypadBase : KeypadBase
    {
        protected TitleParameters CurrentTitleParameters;

        protected RgbJunkieKeypadBase(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            Connection.OnTitleParametersDidChange += Connection_OnTitleParametersDidChange;
        }

        public override void Dispose()
        {
            Connection.OnTitleParametersDidChange -= Connection_OnTitleParametersDidChange;
        }

        public abstract string[] ApplicationUrls { get; }

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

        public override void KeyReleased(KeyPayload payload)
        {
            RgbJunkieUrlHelper.OpenUrls(ApplicationUrls);
        }

        protected virtual void Connection_OnTitleParametersDidChange(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.TitleParametersDidChange> e)
        {
            CurrentTitleParameters = e.Event.Payload.TitleParameters;
            UpdateButtonTitle();
        }

        protected virtual void UpdateButtonTitle()
        {
        }

        protected void SetButtonTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return;
            Connection.SetTitleAsync(title.SplitToFitKey(CurrentTitleParameters));
        }
    }
}
