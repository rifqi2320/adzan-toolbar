using System;
using System.IO;
using System.Text.Json;

namespace AdzanToolbar.Settings;

internal sealed class JsonSettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _configPath;

    public JsonSettingsStore()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AdzanToolbar");
        Directory.CreateDirectory(appDataPath);
        _configPath = Path.Combine(appDataPath, "config.json");
    }

    public AppSettings Load()
    {
        if (!File.Exists(_configPath))
        {
            var defaults = new AppSettings();
            Save(defaults);
            return defaults;
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            return Normalize(JsonSerializer.Deserialize<AppSettings>(json, SerializerOptions) ?? new AppSettings());
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        var normalized = Normalize(settings);
        var json = JsonSerializer.Serialize(normalized, SerializerOptions);
        File.WriteAllText(_configPath, json);
    }

    private static AppSettings Normalize(AppSettings settings)
    {
        settings.City = string.IsNullOrWhiteSpace(settings.City) ? "Jakarta" : settings.City.Trim();
        settings.Country = string.IsNullOrWhiteSpace(settings.Country) ? "Indonesia" : settings.Country.Trim();
        settings.CalculationMethod = settings.CalculationMethod <= 0 ? 20 : settings.CalculationMethod;
        settings.ReminderLeadMinutes = Math.Max(0, settings.ReminderLeadMinutes);
        settings.PollingIntervalSeconds = settings.PollingIntervalSeconds < 10 ? 30 : settings.PollingIntervalSeconds;
        settings.Prayers ??= new PrayerPreferences();
        return settings;
    }
}
