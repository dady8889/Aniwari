using Microsoft.Extensions.Logging;
using Aniwari.BL;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.UI.Xaml.Controls;
using Aniwari.Platforms;
using Aniwari.Managers;
using Aniwari.BL.Interfaces;

namespace Aniwari;

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
            })
            .ConfigureLifecycleEvents(events =>
            {

                // Fix WebView bug with popups
#if WINDOWS
                events.AddWindows(windows => windows
                       .OnPlatformMessage((window, args) =>
                       {
                           // force redraw of webview => causes all popups to close
                           if (args.MessageId == 561) // WM_ENTERSIZEMOVE
                           {
                               var mauiWindow = window.GetWindow();
                               if (mauiWindow != null)
                               {
                                   var blazorWebView = (mauiWindow.Content as MainPage)?.Content as BlazorWebView;
                                   if (blazorWebView != null)
                                   {
                                       var platformWebView = blazorWebView.Handler?.PlatformView as WebView2;
                                       if (platformWebView != null)
                                       {
                                           var margin = platformWebView.Margin;
                                           platformWebView.Margin = new Microsoft.UI.Xaml.Thickness(platformWebView.Margin.Left + 1);
                                           platformWebView.UpdateLayout();
                                           platformWebView.Margin = margin;
                                       }
                                   }
                               }

                               // System.Diagnostics.Debug.WriteLine($"WM_ENTERSIZEMOVE");
                           }

                           // force resize => causes popups to show at the correct location
                           else if (args.MessageId == 562) // WM_EXITSIZEMOVE
                           {
                               var windowsWindow = window.GetAppWindow();
                               if (windowsWindow != null)
                               {
                                   var size = windowsWindow.Size;
                                   windowsWindow.Resize(new Windows.Graphics.SizeInt32(size.Width + 1, size.Height));
                                   windowsWindow.Resize(new Windows.Graphics.SizeInt32(size.Width, size.Height));
                               }

                               // System.Diagnostics.Debug.WriteLine($"WM_EXITSIZEMOVE");
                           }
                       })
                       .OnClosed((window, args) =>
                       {
                           var mauiWindow = window.GetWindow();
                           if (mauiWindow != null && mauiWindow.Handler != null)
                           {
                               var torrents = mauiWindow.Handler.MauiContext?.Services.GetService<ITorrentService>()!;
                               torrents.SaveStateAndExit().Wait();

                               var settings = mauiWindow.Handler.MauiContext?.Services.GetService<ISettingsService>()!;
                               settings.SaveAsync().Wait();
                           }
                       })
                );
#endif
            });


        builder.Services.AddMauiBlazorWebView();

        // Debug services
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        builder.Logging.AddFilter("Microsoft.AspNetCore.Components.RenderTree.*", LogLevel.None);
        builder.Logging.AddDebug();
#endif

        // Internal services
        builder.Services.AddSingleton<IThemeManager, ThemeManager>();
        builder.Services.AddSingleton<IToastManager, ToastManager>();
        builder.Services.AddSingleton<ITitleManager, TitleManager>();

        // BL services
        builder.Services.AddAniwari();

        // Windows specific services
#if WINDOWS
        builder.Services.AddTransient<IFolderPicker, Platforms.Windows.FolderPicker>();
        builder.Services.AddTransient<Platforms.IFilePicker, Platforms.Windows.FilePicker>();
#endif

        return builder.Build();
    }
}
