using Aniwari.BL.Interfaces;
using Aniwari.BL.Services;
using Aniwari.DAL.Constants;
using Microsoft.JSInterop;
using Microsoft.Maui.Storage;
using MonoTorrent;
using System.IO;
using System.Net.Mime;

namespace Aniwari.Managers;

public interface IThemeManager
{
    /// <summary>
    /// Set colors according to the values stored in <see cref="SettingsStore"/>.
    /// </summary>
    Task SetThemeColor(IJSRuntime js);

    Task SetMainBackground(IJSRuntime js);
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

    public async Task SetMainBackground(IJSRuntime js)
    {
        await js.InvokeVoidAsync("setMainBackground", GetBackgroundImage());
    }

    private string? GetBackgroundImage()
    {
        var backgroundFile = _settingsService.GetStore().BackgroundFile;
        if (backgroundFile == null)
            return null;

        var fileBytes = File.ReadAllBytes(Path.Combine(Paths.AppDataPath, backgroundFile));

        var ext = Path.GetExtension(backgroundFile);
        string mimeType = "";

        if (ext == ".jpg" || ext == ".jpeg")
            mimeType = "jpeg";
        else if (ext == ".png")
            mimeType = "png";

        var base64Image = "data:image/" + mimeType + ";base64," + Convert.ToBase64String(fileBytes);

        return base64Image;
    }
}
