using Aniwari.BL.Interfaces;
using Aniwari.Managers;
using System.Reflection;

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
        window.Height = 800;

        return window;
    }

    protected override async void OnStart()
    {
        base.OnStart();

        var settings = this.Handler.MauiContext?.Services.GetService<ISettingsService>()!;
        await settings.LoadAsync();

        var torrents = this.Handler.MauiContext?.Services.GetService<ITorrentService>()!;
        await torrents.RestoreState();

        var updates = this.Handler.MauiContext?.Services.GetService<IUpdateService>()!;
        var toast = this.Handler.MauiContext?.Services.GetService<IToastManager>()!;
        var (CanUpdate, NewestVersion) = await updates.CanUpdate(Assembly.GetExecutingAssembly());
        if (CanUpdate)
        {
            toast.Show(ToastType.Info, "New version available. Check out the Settings tab.");
        }
    }
}
