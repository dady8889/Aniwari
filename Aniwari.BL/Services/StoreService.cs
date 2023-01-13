using System.Text.Json;
using Microsoft.Extensions.Logging;

using Aniwari.DAL.Storage;
using Aniwari.BL.Interfaces;
using Aniwari.DAL.Constants;

namespace Aniwari.BL.Services;

public class StoreService : IStoreService
{
    private readonly ILogger<StoreService> _logger;

    private readonly string settingsFilePath = Paths.SettingsFilePath;

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

            await JsonSerializer.SerializeAsync(newFile, settings, new JsonSerializerOptions() { WriteIndented = true }).ConfigureAwait(false);
            await newFile.DisposeAsync().ConfigureAwait(false);

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
            await file.DisposeAsync().ConfigureAwait(false);

            if (json == null)
                throw new NullReferenceException(nameof(json));

            _logger.LogDebug("Loaded settings from {}", settingsFilePath);

            return json;
        }
        catch(JsonException)
        {
            return new SettingsStore();
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
            await EnsurePathAsync().ConfigureAwait(false);

            using var file = File.Open(settingsFilePath, FileMode.Truncate);

            await JsonSerializer.SerializeAsync(file, store, new JsonSerializerOptions() { WriteIndented = true }).ConfigureAwait(false);
            await file.DisposeAsync().ConfigureAwait(false);

            _logger.LogDebug("Saving settings to file {}", settingsFilePath);

        }
        catch(Exception ex)
        {
            _logger.LogError("Could not save the settings file. Exception: {}", ex.ToString());
            throw;
        }
    }
}
