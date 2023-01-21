using Aniwari.BL.Interfaces;
using Aniwari.DAL.Storage;
using Microsoft.Extensions.Logging;

namespace Aniwari.BL.Services;

public class SettingsService : ISettingsService
{
    private readonly SemaphoreSlim _mutex;
    private readonly ILogger<SettingsService> _logger;
    private readonly IStoreService _storeService;

    private SettingsStore? cachedSettings = null;

    public SettingsService(ILogger<SettingsService> logger, IStoreService storeService)
    {
        _logger = logger;
        _storeService = storeService;
        _mutex = new SemaphoreSlim(1, 1);
    }

    public async Task LoadAsync()
    {
        await _mutex.WaitAsync();

        try
        {
            cachedSettings = await _storeService.LoadAsync().ConfigureAwait(false);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task SaveAsync()
    {
        if (cachedSettings == null)
        {
            _logger.LogError("Settings file has not yet been loaded.");
            throw new Exception("Settings file has not yet been loaded.");
        }

        await _mutex.WaitAsync();
        try
        {
            await _storeService.SaveAsync(cachedSettings).ConfigureAwait(false);

        }
        finally
        {
            _mutex.Release();
        }
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
