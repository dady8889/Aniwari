using Microsoft.Extensions.Logging;
using Aniwari.BL;

namespace Aniwari;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.SetMinimumLevel(LogLevel.Trace);
		builder.Logging.AddDebug();
#endif

		builder.Services.AddAniwari();

		return builder.Build();
	}
}
