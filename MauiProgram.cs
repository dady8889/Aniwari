using Microsoft.Extensions.Logging;
using Aniwari.BL;
using Microsoft.Maui.LifecycleEvents;
using AngleSharp.Dom;
using Microsoft.Maui.Platform;
using Microsoft.UI.Windowing;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.UI.Xaml.Controls;

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
                );
#endif
            });


        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Logging.AddDebug();
#endif

        builder.Services.AddAniwari();

        return builder.Build();
    }
}
