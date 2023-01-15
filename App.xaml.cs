using Aniwari.BL.Interfaces;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;

namespace Aniwari;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

#if WINDOWS
        Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
        {
            var mauiWindow = handler.VirtualView;
            var nativeWindow = handler.PlatformView;
            nativeWindow.Activate();
            IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new SizeInt32(800, 1000));
        });
#endif

        MainPage = new MainPage();
    }

    protected override async void OnStart()
    {
        base.OnStart();

        var settings = this.Handler.MauiContext?.Services.GetService<ISettingsService>()!;
        await settings.LoadAsync();

        var torrents = this.Handler.MauiContext?.Services.GetService<ITorrentService>()!;
        await torrents.RestoreState();
    }
}
