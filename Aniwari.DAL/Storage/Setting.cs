using System.Runtime;

namespace Aniwari.DAL.Storage;

public record Setting(string Name, Type Type, string Description, object? DefaultValue);

public record SettingCategory(string Name, List<Setting> Settings);
