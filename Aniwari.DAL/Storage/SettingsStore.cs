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
        var preferencesCategory = AddCategory("Preferences");
        var themeCategory = AddCategory("Theme");
        var torrentCategory = AddCategory("Torrents");

        AddSetting(preferencesCategory, () => ArchivePath, "Archive location");
        AddSetting(preferencesCategory, () => PreferredTime, "Time format");
        AddSetting(preferencesCategory, () => PreferredTitleLanguage, "Anime title language");

        AddSetting(themeCategory, () => EnableDarkMode, "Enable dark mode");
        AddSetting(themeCategory, () => ThemeColor, "Theme color");
        AddSetting(themeCategory, () => BackgroundFile, "Custom background");

        AddSetting(torrentCategory, () => MaximumSeedRatio, "Maximum seed ratio");
        AddSetting(torrentCategory, () => MaximumDownloadSpeed, "Maximum download speed");
        AddSetting(torrentCategory, () => MaximumUploadSpeed, "Maximum upload speed");
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

    [JsonIgnore]
    public bool UsesMAL => MalCsrfToken != null && MalSessionId != null && MalUsername != null;

    public List<AniwariAnime> Animes { get; set; } = new();
    public List<JikanAnime> JikanCache { get; set; } = new();
}

public sealed partial class SettingsStore
{
    private readonly Dictionary<string, Setting> _settings = new();
    private readonly Dictionary<string, SettingCategory> _categories = new();

    public SettingCategory AddCategory(string categoryName)
    {
        if (!_categories.TryGetValue(categoryName, out var cat))
        {
            var category = new SettingCategory(categoryName, new List<Setting>());
            _categories.Add(categoryName, category);
            return category;
        }

        return cat;
    }

    private void AddSetting(string propertyName, Setting setting)
    {
        _settings.Add(propertyName, setting);
    }

    private void AddSetting<T>(Expression<Func<T>> expression, string settingDescription)
    {
        var (propertyName, propertyType, defaultValue) = GetSettingProperties(expression);
        var setting = new Setting(propertyName, propertyType, settingDescription, defaultValue);

        AddSetting(propertyName, setting);
    }

    private void AddSetting<T>(SettingCategory category, Expression<Func<T>> expression, string settingDescription)
    {
        var (propertyName, propertyType, defaultValue) = GetSettingProperties(expression);
        var setting = new Setting(propertyName, propertyType, settingDescription, defaultValue);

        AddSetting(propertyName, setting);
        category.Settings.Add(setting);
    }

    private (string propertyName, Type propertyType, object? defaultValue) GetSettingProperties<T>(Expression<Func<T>> expression)
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

        return (propertyName, propertyType, defaultValue);
    }

    public ReadOnlyDictionary<string, Setting> GetSettings() => new(_settings);

    public ReadOnlyDictionary<string, SettingCategory> GetCategories() => new(_categories);

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
