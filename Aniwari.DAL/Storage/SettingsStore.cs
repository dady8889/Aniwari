using Aniwari.DAL.Constants;
using Aniwari.DAL.Jikan;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Aniwari.DAL.Storage;

public sealed partial class SettingsStore
{
    public SettingsStore()
    {
        AddSetting(() => EnableDarkMode, "Enable dark mode");
        AddSetting(() => ThemeColor, "Theme color");
        AddSetting(() => BackgroundFile, "Custom background");
        AddSetting(() => ArchivePath, "Archive location");
        AddSetting(() => MaximumSeedRatio, "Maximum seed ratio");
        AddSetting(() => MaximumDownloadSpeed, "Maximum download speed");
        AddSetting(() => MaximumUploadSpeed, "Maximum upload speed");
        AddSetting(() => PreferredTime, "Time format");
        AddSetting(() => PreferredTitleLanguage, "Anime title language");
    }

    public bool EnableDarkMode { get; set; } = false;
    public string ThemeColor { get; set; } = ThemeColors.LightColor;
    public string ArchivePath { get; set; } = Paths.ArchiveDirPath;
    public decimal MaximumSeedRatio { get; set; } = -1;
    public int MaximumDownloadSpeed { get; set; } = -1;
    public int MaximumUploadSpeed { get; set; } = -1;
    public PreferredTime PreferredTime { get; set; } = PreferredTime.Local;
    public PreferredTitleLanguage PreferredTitleLanguage { get; set; } = PreferredTitleLanguage.English;
    public string? BackgroundFile { get; set; } = null;

    public string? MalSessionId { get; set; } = null;
    public string? MalCsrfToken { get; set; } = null;
    public string? MalUsername { get; set; } = null;
    public bool UsesMAL => MalCsrfToken != null && MalSessionId != null && MalUsername != null;

    public List<AniwariAnime> Animes { get; set; } = new();
    public List<JikanAnime> JikanCache { get; set; } = new();
}

public sealed partial class SettingsStore
{
    private readonly Dictionary<string, Setting> _settings = new();

    private void AddSetting(string propertyName, Type settingType, string settingDescription, object? defaultValue)
    {
        _settings.Add(propertyName, new Setting(settingType, settingDescription, defaultValue));
    }

    private void AddSetting<T>(Expression<Func<T>> expression, string settingDescription)
    {
        var memberExpression = (MemberExpression)expression.Body;

        if (memberExpression == null)
            throw new ArgumentNullException(nameof(expression));

        var propertyName = memberExpression.Member.Name;
        var propertyType = typeof(T);
        var defaultValue = expression.Compile().Invoke();

        if (propertyName == null)
            throw new ArgumentNullException(nameof(expression));

        if (propertyType == null)
            throw new ArgumentNullException(nameof(expression));

        AddSetting(propertyName, propertyType, settingDescription, defaultValue);
    }

    public ReadOnlyDictionary<string, Setting> GetSettings() => new(_settings);

    public void SetDefaults()
    {
        foreach (var (propertyName, setting) in _settings)
        {
            Set(setting.Type, propertyName, setting.DefaultValue);
        }
    }

    public void Set(Type type, string propertyName, object? value)
    {
        var property = this.GetType().GetProperty(propertyName);

        if (property == null || !_settings.TryGetValue(propertyName, out var setting))
        {
            throw new Exception($"The property {propertyName} is not in store.");
        }

        if (setting.Type != type)
        {
            throw new Exception($"The property {propertyName} is of type {setting.Type.Name} and not {type.Name}.");
        }

        property.SetValue(this, value);
    }

    public void Set<T>(string propertyName, T value)
    {
        Set(typeof(T), propertyName, value);
    }

    public object? Get(Type type, string propertyName)
    {
        var property = this.GetType().GetProperty(propertyName);

        if (property == null || !_settings.TryGetValue(propertyName, out var setting))
        {
            throw new Exception($"The property {propertyName} is not in store.");
        }

        if (setting.Type != type)
        {
            throw new Exception($"The property {propertyName} is of type {setting.Type.Name} and not {type.Name}.");
        }

        return property.GetValue(this);
    }

    public T? Get<T>(string propertyName)
    {
        return (T?)Get(typeof(T), propertyName);
    }
}
