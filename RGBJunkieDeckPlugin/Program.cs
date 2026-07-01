using BarRaider.SdTools;

namespace RGBJunkieDeckPlugin
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // Uncomment to attach a debugger before Stream Deck connects:
            // while (!System.Diagnostics.Debugger.IsAttached) { System.Threading.Thread.Sleep(100); }

            SDWrapper.Run(args);
        }
    }
}
