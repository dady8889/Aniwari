using Aniwari.BL.Services;
using Microsoft.JSInterop;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aniwari.Managers;

public interface IThemeManager
{
    /// <summary>
    /// Set colors according to the values stored in <see cref="SettingsStore"/>.
    /// </summary>
    Task SetThemeColor(IJSRuntime js);
}

public static class ThemeColors
{
    public readonly static string LightColor = "#EFEFEF";
    public readonly static string DarkColor = "#1C1C1C";
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
