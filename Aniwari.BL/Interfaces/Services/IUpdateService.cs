using System.Reflection;

namespace Aniwari.BL.Interfaces;

public interface IUpdateService
{
    Task<(bool CanUpdate, Version NewestVersion)> CanUpdate(Assembly assembly, CancellationToken token = default);
    Task Update(Assembly assembly, IProgress<double>? onProgress = null, CancellationToken token = default);
}
