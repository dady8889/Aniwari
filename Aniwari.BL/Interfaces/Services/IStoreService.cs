using Aniwari.DAL.Storage;

namespace Aniwari.BL.Interfaces;

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