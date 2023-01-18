namespace Aniwari.DAL.Storage;

public record Setting(Type Type, string Description, object? DefaultValue);
