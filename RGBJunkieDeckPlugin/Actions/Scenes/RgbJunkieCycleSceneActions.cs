using BarRaider.SdTools;

using RGBJunkieDeckPlugin.Helpers;



namespace RGBJunkieDeckPlugin.Actions.Scenes

{

    [PluginActionId("com.rgbjunkie.deck.previousscene")]

    internal sealed class RgbJunkiePreviousSceneAction : RgbJunkieKeypadBase

    {

        public RgbJunkiePreviousSceneAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)

        {

        }



        public override string[] ApplicationUrls => new[] { RgbJunkieUrlHelper.BuildSceneCycleUrl(previous: true) };

    }



    [PluginActionId("com.rgbjunkie.deck.nextscene")]

    internal sealed class RgbJunkieNextSceneAction : RgbJunkieKeypadBase

    {

        public RgbJunkieNextSceneAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)

        {

        }



        public override string[] ApplicationUrls => new[] { RgbJunkieUrlHelper.BuildSceneCycleUrl(previous: false) };

    }

}


