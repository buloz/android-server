using Android.App;
using Android.Content.PM;
using Android.OS;
using ApplicationDomain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

namespace AndroidDemo
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var mauiApp = IPlatformApplication.Current;

            var serverService = mauiApp?.Services.GetService<IServerService>();

            if (serverService != null)
            {
                _ = serverService.StartAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        // Log or handle the exception as needed
                        System.Diagnostics.Debug.WriteLine($"Server start failed: {task.Exception}");
                    }
                });
            }
        }
    }
}
