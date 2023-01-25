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
        MainPage = new MainPage();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        window.Width = 800;
        window.Height = 1000;

        return window;
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
