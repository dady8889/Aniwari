using Aniwari.BL.Interfaces;
using Aniwari.BL.Services;
using Aniwari.DAL.Constants;
using Microsoft.JSInterop;

namespace Aniwari.Managers;

public interface IThemeManager
{
    /// <summary>
    /// Set colors according to the values stored in <see cref="SettingsStore"/>.
    /// </summary>
    Task SetThemeColor(IJSRuntime js);
}

public class ThemeManager : IThemeManager
{
    private readonly ISettingsService _settingsService;

    public ThemeManager(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task SetThemeColor(IJSRuntime js)
    {
        var color = _settingsService.GetStore().ThemeColor;
        var usesDarkMode = _settingsService.GetStore().EnableDarkMode;

        if (usesDarkMode)
        {
            await SetDefaultDark(js);
            return;
        }

        await SetThemeColor(js, color);
    }

    private async Task SetThemeColor(IJSRuntime js, string hex)
    {
        if (string.IsNullOrEmpty(hex))
        {
            await SetDefaultLight(js);
            return;
        }

        await js.InvokeVoidAsync("setThemeColors", hex);
    }

    private async Task SetDefaultLight(IJSRuntime js)
    {
        await SetThemeColor(js, ThemeColors.LightColor);
    }

    private async Task SetDefaultDark(IJSRuntime js)
    {
        await SetThemeColor(js, ThemeColors.DarkColor);
    }
}
