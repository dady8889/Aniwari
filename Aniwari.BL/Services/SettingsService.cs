using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aniwari.BL.Services;

public interface ISettingsService
{
    /// <summary>
    /// Loads the settings file provided by <see cref="IStoreService"/> into cache.
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// Saves the cached settings file using <see cref="IStoreService"/>.
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// Gets the settings store from cache. Change the object's properties to edit the application's settings.
    /// </summary>
    SettingsStore GetStore();
}

public class SettingsService : ISettingsService
{
    private readonly ILogger<SettingsService> _logger;
    private readonly IStoreService _storeService;

    private SettingsStore? cachedSettings = null;

    public SettingsService(ILogger<SettingsService> logger, IStoreService storeService)
    {
        _logger = logger;
        _storeService = storeService;
    }

    public async Task LoadAsync()
    {
        cachedSettings = await _storeService.LoadAsync().ConfigureAwait(false);
    }

    public async Task SaveAsync()
    {
        if (cachedSettings == null)
        {
            _logger.LogError("Settings file has not yet been loaded.");
            throw new Exception("Settings file has not yet been loaded.");
        }

        await _storeService.SaveAsync(cachedSettings).ConfigureAwait(false);
    }

    public SettingsStore GetStore()
    {
        if (cachedSettings == null)
        {
            _logger.LogError("Settings file has not yet been loaded.");
            throw new Exception("Settings file has not yet been loaded.");
        }

        return cachedSettings;
    }
}
