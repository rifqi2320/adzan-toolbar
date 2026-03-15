using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using AdzanToolbar.Models;

namespace AdzanToolbar.Services;

internal sealed class PrayerCacheStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _cachePath;

    public PrayerCacheStore()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AdzanToolbar");
        Directory.CreateDirectory(appDataPath);
        _cachePath = Path.Combine(appDataPath, "prayer-cache.json");
    }

    public IReadOnlyList<PrayerDaySchedule> GetDays(string city, string country, DateOnly startDate, DateOnly endDate)
    {
        var cache = LoadCache();
        Prune(cache);
        SaveCache(cache);
        return cache.Entries
            .Where(entry => Matches(entry, city, country) && entry.Date >= startDate && entry.Date <= endDate)
            .OrderBy(entry => entry.Date)
            .Select(ToModel)
            .ToArray();
    }

    public void UpsertDays(string city, string country, IReadOnlyList<PrayerDaySchedule> days)
    {
        var cache = LoadCache();
        Prune(cache);

        foreach (var day in days)
        {
            cache.Entries.RemoveAll(entry => Matches(entry, city, country) && entry.Date == day.Date);
            cache.Entries.Add(new CachedPrayerDay
            {
                City = city.Trim(),
                Country = country.Trim(),
                Date = day.Date,
                TimeZoneId = day.TimeZoneId,
                Fajr = day.Fajr,
                Dhuhr = day.Dhuhr,
                Asr = day.Asr,
                Maghrib = day.Maghrib,
                Isha = day.Isha,
                CachedAtUtc = DateTime.UtcNow
            });
        }

        SaveCache(cache);
    }

    private CacheEnvelope LoadCache()
    {
        if (!File.Exists(_cachePath))
        {
            return new CacheEnvelope();
        }

        try
        {
            var json = File.ReadAllText(_cachePath);
            return JsonSerializer.Deserialize<CacheEnvelope>(json, SerializerOptions) ?? new CacheEnvelope();
        }
        catch
        {
            return new CacheEnvelope();
        }
    }

    private void SaveCache(CacheEnvelope cache)
    {
        var json = JsonSerializer.Serialize(cache, SerializerOptions);
        File.WriteAllText(_cachePath, json);
    }

    private static void Prune(CacheEnvelope cache)
    {
        var cutoffDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-7));
        cache.Entries.RemoveAll(entry => entry.Date < cutoffDate || entry.CachedAtUtc < DateTime.UtcNow.AddDays(-7));
    }

    private static bool Matches(CachedPrayerDay entry, string city, string country) =>
        string.Equals(entry.City, city.Trim(), StringComparison.OrdinalIgnoreCase) &&
        string.Equals(entry.Country, country.Trim(), StringComparison.OrdinalIgnoreCase);

    private static PrayerDaySchedule ToModel(CachedPrayerDay entry) =>
        new()
        {
            Date = entry.Date,
            TimeZoneId = entry.TimeZoneId,
            Fajr = entry.Fajr,
            Dhuhr = entry.Dhuhr,
            Asr = entry.Asr,
            Maghrib = entry.Maghrib,
            Isha = entry.Isha
        };

    private sealed class CacheEnvelope
    {
        public List<CachedPrayerDay> Entries { get; set; } = [];
    }

    private sealed class CachedPrayerDay
    {
        public string City { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public DateOnly Date { get; set; }

        public string TimeZoneId { get; set; } = string.Empty;

        public string Fajr { get; set; } = string.Empty;

        public string Dhuhr { get; set; } = string.Empty;

        public string Asr { get; set; } = string.Empty;

        public string Maghrib { get; set; } = string.Empty;

        public string Isha { get; set; } = string.Empty;

        public DateTime CachedAtUtc { get; set; }
    }
}
