using WindowsFilePicker = Windows.Storage.Pickers.FileOpenPicker;

namespace Aniwari.Platforms.Windows;

public class FilePicker : IFilePicker
{
    public async Task<string?> PickFile(IEnumerable<string>? filter = null)
    {
        var filePicker = new WindowsFilePicker();

        if (filter == null)
            filePicker.FileTypeFilter.Add("*");
        else
            foreach (var f in filter)
                filePicker.FileTypeFilter.Add(f);

        // Get the current window's HWND by passing in the Window object
        var window = Application.Current?.Windows?.FirstOrDefault()?.Handler.PlatformView as MauiWinUIWindow;
        var hwnd = window?.WindowHandle;

        if (hwnd == null)
        {
            throw new NullReferenceException();
        }

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd.Value);

        var result = await filePicker.PickSingleFileAsync();

        return result?.Path;
    }
}
