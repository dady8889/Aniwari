using Aniwari.BL.Interfaces;
using Aniwari.DAL.Storage;
using Microsoft.Extensions.Logging;

namespace Aniwari.BL.Services;

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
