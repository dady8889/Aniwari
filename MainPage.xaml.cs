using Microsoft.Maui.Platform;
using Microsoft.UI.Windowing;

namespace Aniwari;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

#if WINDOWS
    // https://github.com/MicrosoftEdge/WebView2Feedback/issues/2290

    bool _foundWindow;

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (!_foundWindow)
        {
            var window = GetParentWindow();

            if ((window?.Handler?.PlatformView as MauiWinUIWindow)?.GetAppWindow() is AppWindow appWindow)
            {
                appWindow.Changed += AppWindow_Changed;
                _foundWindow = true;
            }
        }
    }

    private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
    {
        /*if (args.DidPositionChange)
        {
            var width = this.Window.Width;

            this.Window.Width = width + 1;
            this.Window.Width = width;
        }*/
    }
#endif
}
