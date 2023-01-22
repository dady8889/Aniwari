using System;

namespace Aniwari.Platforms;

public interface IFilePicker
{
    Task<string?> PickFile(IEnumerable<string>? filter = null);
}
