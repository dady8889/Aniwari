using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Aniwari.BL.Services;

public interface IStoreService
{
    /// <summary>
    /// Parses the setting file as a <see cref="SettingsStore"/> object.
    /// </summary>
    Task<SettingsStore> LoadAsync();

    /// <summary>
    /// Parses the <see cref="SettingsStore"/> object and saves it into the specified location.
    /// </summary>
    Task SaveAsync(SettingsStore store);
}

public class StoreService : IStoreService
{
    private readonly ILogger<StoreService> _logger;

    private readonly string settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aniwari\\", "settings.json");

    public StoreService(ILogger<StoreService> logger)
    {
        _logger = logger;
    }

    private async Task EnsurePathAsync()
    {
        try
        {
            // if the settings file doesnt exist, save default settings
            if (File.Exists(settingsFilePath))
                return;

            // create folder structure if it doesnt exist
            string directory = Path.GetDirectoryName(settingsFilePath)!;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using var newFile = File.Create(settingsFilePath);

            var settings = new SettingsStore();

            await JsonSerializer.SerializeAsync(newFile, settings, new JsonSerializerOptions() { WriteIndented = true });
            await newFile.DisposeAsync();

            _logger.LogDebug("Creating new settings file to {}", settingsFilePath);
        }
        catch(Exception ex)
        {
            _logger.LogError("Could not ensure path to the settings file. Exception: {}", ex.ToString());
            throw;
        }
    }

    public async Task<SettingsStore> LoadAsync()
    {
        try
        {
            await EnsurePathAsync();

            using var file = File.Open(settingsFilePath, FileMode.Open);
            var json = await JsonSerializer.DeserializeAsync<SettingsStore>(file);
            await file.DisposeAsync();

            if (json == null)
                throw new NullReferenceException(nameof(json));

            _logger.LogDebug("Loaded settings from {}", settingsFilePath);

            return json;
        }
        catch(Exception ex)
        {
            _logger.LogError("Could not load the settings file. Exception: {}", ex.ToString());
            throw;
        }
    }

    public async Task SaveAsync(SettingsStore store)
    {
        try
        {
            await EnsurePathAsync();

            using var file = File.Open(settingsFilePath, FileMode.Truncate);

            await JsonSerializer.SerializeAsync(file, store, new JsonSerializerOptions() { WriteIndented = true });
            await file.DisposeAsync();

            _logger.LogDebug("Saving settings to file {}", settingsFilePath);

        } catch(Exception ex)
        {
            _logger.LogError("Could not save the settings file. Exception: {}", ex.ToString());
            throw;
        }
    }
}

public class SettingsStore
{
    private readonly Dictionary<string, Setting> _settings = new();

    public SettingsStore()
    {
        _settings.Add(nameof(EnableDarkMode), new Setting(typeof(bool), "Enable dark mode", false));
    }

    public ReadOnlyDictionary<string, Setting> GetSettings() => new(_settings);

    public void SetDefaults()
    {
        foreach (var (propertyName, setting) in _settings)
        {
            Set(setting.Type, propertyName, setting.DefaultValue);
        }
    }

    public void Set(Type type, string propertyName, object? value)
    {
        var property = this.GetType().GetProperty(propertyName);

        if (property == null || !_settings.TryGetValue(propertyName, out var setting))
        {
            throw new Exception($"The property {propertyName} is not in store.");
        }

        if (setting.Type != type)
        {
            throw new Exception($"The property {propertyName} is of type {setting.Type.Name} and not {type.Name}.");
        }

        property.SetValue(this, value);
    }

    public void Set<T>(string propertyName, T value)
    {
        Set(typeof(T), propertyName, value);
    }

    public object? Get(Type type, string propertyName)
    {
        var property = this.GetType().GetProperty(propertyName);

        if (property == null || !_settings.TryGetValue(propertyName, out var setting))
        {
            throw new Exception($"The property {propertyName} is not in store.");
        }

        if (setting.Type != type)
        {
            throw new Exception($"The property {propertyName} is of type {setting.Type.Name} and not {type.Name}.");
        }

        return property.GetValue(this);
    }

    public T? Get<T>(string propertyName)
    {
        return (T?)Get(typeof(T), propertyName);
    }

    #region Properties

    public bool EnableDarkMode { get; set; }

    public List<Anime> Animes { get; set; } = new();

    #endregion

    public record Setting(Type Type, string Description, object DefaultValue);

    public class Anime
    {
        public Anime()
        {
        }
            
        public Anime(int id, bool watching, string title, int? episodesCount, string searchString)
        {
            Id = id;
            Watching = watching;
            Title = title;
            EpisodesCount = episodesCount;
            SearchString = searchString;
        }

        public int Id { get; set; }
        public bool Watching { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SearchString { get; set; } = string.Empty;
        public int? EpisodesCount { get; set; }
        public int Watched { get; set; } = 0;

        public List<Episode> Episodes { get; set; } = new();

        public IEnumerable<Episode> GetWatchedEpisodes() => Episodes.Where(x => x.Watched);
    }

    public class Episode
    {
        public int Id { get; set; }
        public bool Watched { get; set; }
    }
}
