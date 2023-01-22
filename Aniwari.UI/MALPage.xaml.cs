using JikanDotNet;
using Microsoft.Maui.Platform;
using Microsoft.Web.WebView2.Core;
using System.Net;
using System.Text.Json;

namespace Aniwari;

public partial class MALPage : ContentPage
{
    public event EventHandler<PageClosedEventArgs>? PageClosed;
    private const string MalUrl = "https://myanimelist.net/";

    public MALPage()
    {
        InitializeComponent();
    }

    public bool DeleteCookies { get; init; }

    public class PageClosedEventArgs : EventArgs
    {
        public string? MalSessionId { get; set; }
        public string? CsrfToken { get; set; }
        public string? Username { get; set; }
    }

    private async void webView_Navigated(object sender, WebNavigatedEventArgs e)
    {
        var coreWebView = (webView?.Handler?.PlatformView as MauiWebView)?.CoreWebView2;
        var cookieManager = coreWebView?.CookieManager;

        if (DeleteCookies)
        {
            cookieManager?.DeleteAllCookies();
            if (App.Current?.MainPage != null)
                await App.Current.MainPage.Navigation.PopModalAsync();

            PageClosed?.Invoke(this, new PageClosedEventArgs());

            return;
        }

        if (e.Url == MalUrl)
        {
            var ck = await cookieManager?.GetCookiesAsync(MalUrl);
            var malsessionid = ck.FirstOrDefault(x => x.Name == "MALSESSIONID");
            var csrfJson = await coreWebView?.ExecuteScriptAsync("(function() { var metas = document.documentElement.getElementsByTagName(\"meta\"); for (var i = 0; i < metas.length; ++i) { if (metas[i].getAttribute(\"name\") == \"csrf_token\") return metas[i].getAttribute(\"content\"); }\r\n })()");
            var csrf = JsonSerializer.Deserialize<string>(csrfJson);
            var usernameJson = await coreWebView?.ExecuteScriptAsync("(function() { var a = document.documentElement.getElementsByClassName(\"header-profile-link\"); if (a.length > 0) return a[0].innerText; })()");
            var username = JsonSerializer.Deserialize<string>(usernameJson);

            if (App.Current?.MainPage != null)
                await App.Current.MainPage.Navigation.PopModalAsync();

            PageClosed?.Invoke(this, new PageClosedEventArgs()
            {
                MalSessionId = malsessionid?.Value,
                CsrfToken = csrf,
                Username = username
            });
        }
    }
}