using Aniwari.DAL.Storage;

namespace Aniwari.BL.Interfaces;

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
