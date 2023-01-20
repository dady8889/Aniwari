using Aniwari.BL.Interfaces;
using Aniwari.DAL.MyAnimeList;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aniwari.BL.Services;

public class MyAnimeListService : IMyAnimeListService
{
    private const string EditAnimeUrl = "https://myanimelist.net/ownlist/anime/edit.json";
    private const string AddAnimeUrl = "https://myanimelist.net/ownlist/anime/add.json";

    private string GetAnimeListUrl(string username) => $"https://myanimelist.net/animelist/{username}";
    private string GetLoadUrl(string username) => $"https://myanimelist.net/animelist/{username}/load.json";
    private string GetDeleteAnimeUrl(int malAnimeId) => $"https://myanimelist.net/ownlist/anime/{malAnimeId}/delete";
        
    private readonly ILogger<MyAnimeListService> _logger;
    private readonly ISettingsService _settingsService;
    private readonly HttpClient _httpClient;

    public MyAnimeListService(ILogger<MyAnimeListService> logger, ISettingsService settingsService, HttpClient httpClient)
    { 
        _logger = logger;
        _settingsService = settingsService;
        _httpClient = httpClient;
    }

    private void SetHeaders(HttpRequestMessage message, string referer)
    {
        var malSessionId = _settingsService.GetStore().MalSessionId;

        message.Headers.Add("authority", "myanimelist.net");
        message.Headers.Add("accept", "*/*");
        message.Headers.Add("cache-control", "no-cache");
        message.Headers.Add("origin", "https://myanimelist.net");
        message.Headers.Add("pragma", "no-cache");
        message.Headers.Add("referer", referer);
        message.Headers.Add("sec-ch-ua", "\"Not_A Brand\";v=\"99\", \"Google Chrome\";v=\"109\", \"Chromium\";v=\"109\"");
        message.Headers.Add("sec-ch-ua-mobile", "?0");
        message.Headers.Add("sec-ch-ua-platform", "Windows");
        message.Headers.Add("sec-fetch-dest", "empty");
        message.Headers.Add("sec-fetch-mode", "cors");
        message.Headers.Add("sec-fetch-site", "same-origin");
        message.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36");
        message.Headers.Add("x-requested-with", "XMLHttpRequest");

        message.Headers.Add("cookie", $"MALSESSIONID={malSessionId}; is_logged_in=1");
    }

    private (string CsrfToken, string Username, string MalSessionId) GetSecrets()
    {
        var csrfToken = _settingsService.GetStore().MalCsrfToken;
        var username = _settingsService.GetStore().MalUsername;
        var malSessionId = _settingsService.GetStore().MalSessionId;

        if (csrfToken == null || username == null || malSessionId == null)
            throw new Exception("User MAL secrets are missing.");

        return (csrfToken, username, malSessionId);
    }

    public async Task AddAnime(int malAnimeId)
    {
        var (csrfToken, username, _) = GetSecrets();

        var body = new MalAddAnimeData()
        {
            AnimeId = malAnimeId,
            Status = (int)MALAnimeState.Watching,
            StartDate = new MalDate()
            {
                Day = DateTime.Now.Day,
                Month = DateTime.Now.Month,
                Year = DateTime.Now.Year,
            },
            CsrfToken = csrfToken
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/x-www-form-urlencoded");

        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(AddAnimeUrl),
            Content = content
        };

        SetHeaders(httpRequestMessage, GetAnimeListUrl(username));

        var request = await _httpClient.SendAsync(httpRequestMessage);

        _logger.LogDebug("Request to add new anime ended with code {}", request.StatusCode);
    }

    public async Task DeleteAnime(int malAnimeId)
    {
        var (csrfToken, username, _) = GetSecrets();
        var json = JsonSerializer.Serialize(new { csrf_token = csrfToken });
        var content = new StringContent(json, Encoding.UTF8, "application/x-www-form-urlencoded");

        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(GetDeleteAnimeUrl(malAnimeId)),
            Content = content
        };

        SetHeaders(httpRequestMessage, GetAnimeListUrl(username));

        var request = await _httpClient.SendAsync(httpRequestMessage);

        _logger.LogDebug("Request to remove anime ended with code {}", request.StatusCode);
    }

    public async Task EditAnime(int malAnimeId, int watchedEpisodes, MALAnimeState status = MALAnimeState.Watching)
    {
        var (csrfToken, username, _) = GetSecrets();

        var body = new MalEditAnimeData()
        {
            AnimeId = malAnimeId,
            WatchedEpisodes = watchedEpisodes,
            Status = (int)status,
            CsrfToken = csrfToken
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/x-www-form-urlencoded");

        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(EditAnimeUrl),
            Content = content
        };

        SetHeaders(httpRequestMessage, GetAnimeListUrl(username));

        var request = await _httpClient.SendAsync(httpRequestMessage);

        _logger.LogDebug("Request to set anime episodes count ended with code {}", request.StatusCode);
    }

    public async Task<List<MALAnime>?> GetAnimeList(MALAnimeState state, int offset = 0)
    {
        var (_, username, _) = GetSecrets();

        var uri = GetLoadUrl(username) + $"?status={(int)state}" + $"&offset={offset}";

        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(uri)
        };

        SetHeaders(httpRequestMessage, GetLoadUrl(username));

        var request = await _httpClient.SendAsync(httpRequestMessage);

        _logger.LogDebug("Request to get anime list ended with code {}", request.StatusCode);

        var animeList = await request.Content.ReadFromJsonAsync<List<MALAnime>>();

        return animeList;
    }

    private class MalEditAnimeData
    {
        [JsonPropertyName("num_watched_episodes")]
        public int WatchedEpisodes { get; set; }

        [JsonPropertyName("anime_id")]
        public int AnimeId { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("csrf_token")]
        public string CsrfToken { get; set; } = string.Empty;
    }

    private class MalAddAnimeData
    {
        [JsonPropertyName("anime_id")]
        public int AnimeId { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("num_watched_episodes")]
        public int WatchedEpisodes { get; set; }

        [JsonPropertyName("storage_value")]
        public int StorageValue { get; set; }

        [JsonPropertyName("storage_type")]
        public int StorageType { get; set; }

        [JsonPropertyName("start_date")]
        public MalDate StartDate { get; set; }

        [JsonPropertyName("finish_date")]
        public MalDate FinishDate { get; set; }

        [JsonPropertyName("num_watched_times")]
        public int WatchedTimes { get; set; }

        [JsonPropertyName("rewatch_value")]
        public int RewatchValue { get; set; }

        [JsonPropertyName("csrf_token")]
        public string CsrfToken { get; set; } = string.Empty;
    }

    private struct MalDate
    {
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("month")]
        public int Month { get; set; }

        [JsonPropertyName("day")]
        public int Day { get; set; }
    }
}
