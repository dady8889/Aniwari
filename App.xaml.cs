using Aniwari.BL.Services;

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
        Window window = base.CreateWindow(activationState);

        // cross-platform window created event (entry-point)
        window.Created += async (s, e) =>
        {
            var settings = this.Handler.MauiContext?.Services.GetService<ISettingsService>()!;
            await settings.LoadAsync();
        };

        window.Stopped += async (s, e) =>
        {
            var settings = this.Handler.MauiContext?.Services.GetService<ISettingsService>()!;
            await settings.SaveAsync();
        };

        return window;
    }
}
