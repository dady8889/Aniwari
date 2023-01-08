using WindowsFolderPicker = Windows.Storage.Pickers.FolderPicker;

namespace Aniwari.Platforms.Windows;

public class FolderPicker : IFolderPicker
{
    public async Task<string?> PickFolder()
    {
        var folderPicker = new WindowsFolderPicker();

        // Make it work for Windows 10
        folderPicker.FileTypeFilter.Add("*");

        // Get the current window's HWND by passing in the Window object
        var window = Application.Current?.Windows?.FirstOrDefault()?.Handler.PlatformView as MauiWinUIWindow;
        var hwnd = window?.WindowHandle;

        if (hwnd == null)
        {
            throw new NullReferenceException();
        }

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd.Value);

        var result = await folderPicker.PickSingleFolderAsync();

        return result?.Path;
    }
}
