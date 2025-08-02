using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace AndroidDemo
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
