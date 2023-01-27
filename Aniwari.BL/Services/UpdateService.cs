using Aniwari.BL.Interfaces;
using Microsoft.Extensions.Logging;
using Onova;
using Onova.Models;
using Onova.Services;
using System.Reflection;

namespace Aniwari.BL.Services;

public class UpdateService : IUpdateService
{
    private readonly ILogger<UpdateService> _logger;
    private readonly ITorrentService _torrentService;
    private readonly ISettingsService _settingService;

    public UpdateService(ILogger<UpdateService> logger, ITorrentService torrentService, ISettingsService settingsService)
    {
        _logger = logger;
        _torrentService = torrentService;
        _settingService = settingsService;
    }

    private UpdateManager GetUpdateManager(Assembly assembly)
    {
        return new UpdateManager(AssemblyMetadata.FromAssembly(assembly),
            new GithubPackageResolver("dady8889", "Aniwari", "Aniwari*.zip"),
            new ZipPackageExtractor());
    }

    public async Task<(bool CanUpdate, Version NewestVersion)> CanUpdate(Assembly assembly, CancellationToken token = default)
    {
        try
        {
            using var _updateManager = GetUpdateManager(assembly);

            var check = await _updateManager.CheckForUpdatesAsync(token);

            if (check != null && check.LastVersion != null && check.CanUpdate)
            {
                _logger.LogInformation("Can update to version {}", check.LastVersion.ToString());
                return (true, check.LastVersion);
            }

            _logger.LogInformation("No updates found");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error occured while finding updates. Exception: {}", ex.Message);
            throw;
        }

        return (false, new());
    }

    public async Task Update(Assembly assembly, IProgress<double>? onProgress = null, CancellationToken token = default)
    {
        try
        {
            using var _updateManager = GetUpdateManager(assembly);

            var check = await _updateManager.CheckForUpdatesAsync(token);

            if (check == null || check.LastVersion == null || !check.CanUpdate)
            {
                _logger.LogError("Cannot update to newest version");
                return;
            }

            _logger.LogInformation("Preparing update {}", check.LastVersion.ToString());
            await _updateManager.PrepareUpdateAsync(check.LastVersion, onProgress, token);

            _logger.LogInformation("Saving application state");
            await _torrentService.SaveStateAndExit();
            await _settingService.SaveAsync();

            _logger.LogInformation("Launching updater");
            _updateManager.LaunchUpdater(check.LastVersion);
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not update the application. Exception: {}", ex.Message);
            throw;
        }
    }
}
