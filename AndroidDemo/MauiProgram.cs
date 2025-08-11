
using AndroidDemo.Server;
using ApplicationDomain.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using System;
using Microsoft.Extensions.DependencyInjection;

#if DEBUG
using Microsoft.Extensions.Logging;
#endif

namespace AndroidDemo
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            Register(builder);

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        public static void Register(MauiAppBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services.TryAddSingleton<IServerService, ServerProvider>();
            builder.Services.TryAddSingleton<IMonitoringService, MonitoringService>();
            builder.Services.TryAddSingleton<INetworkService, NetworkService>();
            builder.Services.AddTransient<MainPage>();
        }
    }
    
}
