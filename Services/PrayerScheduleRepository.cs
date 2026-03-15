using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdzanToolbar.Models;
using AdzanToolbar.Settings;

namespace AdzanToolbar.Services;

internal sealed class PrayerScheduleRepository
{
    private readonly AlAdhanClient _adhanClient;
    private readonly PrayerCacheStore _cacheStore;

    public PrayerScheduleRepository(AlAdhanClient adhanClient, PrayerCacheStore cacheStore)
    {
        _adhanClient = adhanClient;
        _cacheStore = cacheStore;
    }

    public async Task<PrayerSchedule> GetPrayerScheduleAsync(AppSettings settings, DateOnly date, CancellationToken cancellationToken)
    {
        var day = await GetPrayerDayAsync(settings.City, settings.Country, date, cancellationToken).ConfigureAwait(false);
        return BuildSchedule(day, settings);
    }

    public async Task<IReadOnlyList<PrayerDaySchedule>> GetPrayerWeekAsync(string city, string country, CancellationToken cancellationToken)
    {
        var startDate = DateOnly.FromDateTime(DateTime.Today).AddDays(-3);
        return await GetPrayerDaysAsync(city, country, startDate, 7, cancellationToken).ConfigureAwait(false);
    }

    private async Task<PrayerDaySchedule> GetPrayerDayAsync(string city, string country, DateOnly date, CancellationToken cancellationToken)
    {
        var days = await GetPrayerDaysAsync(city, country, date, 1, cancellationToken).ConfigureAwait(false);
        return days.Single();
    }

    private async Task<IReadOnlyList<PrayerDaySchedule>> GetPrayerDaysAsync(
        string city,
        string country,
        DateOnly startDate,
        int dayCount,
        CancellationToken cancellationToken)
    {
        var endDate = startDate.AddDays(dayCount - 1);
        var cached = _cacheStore.GetDays(city, country, startDate, endDate);
        var requiredDates = Enumerable.Range(0, dayCount)
            .Select(startDate.AddDays)
            .ToHashSet();

        if (cached.Count == dayCount && cached.All(day => requiredDates.Contains(day.Date)))
        {
            return cached;
        }

        var fetched = await _adhanClient.GetPrayerDaysAsync(city, country, startDate, dayCount, cancellationToken)
            .ConfigureAwait(false);
        _cacheStore.UpsertDays(city, country, fetched);

        return _cacheStore
            .GetDays(city, country, startDate, endDate)
            .Where(day => requiredDates.Contains(day.Date))
            .OrderBy(day => day.Date)
            .ToArray();
    }

    private static PrayerSchedule BuildSchedule(PrayerDaySchedule day, AppSettings settings)
    {
        var localOffset = DateTimeOffset.Now.Offset;
        var prayers = new List<PrayerTime>();
        foreach (var entry in ToPrayerMap(day))
        {
            if (!settings.Prayers.IsEnabled(entry.Key))
            {
                continue;
            }

            var adhanAt = ParsePrayerTime(day.Date, entry.Value, day.TimeZoneId, localOffset);
            prayers.Add(new PrayerTime
            {
                Name = entry.Key,
                DisplayTime = entry.Value,
                AdhanAt = adhanAt,
                TriggerAt = adhanAt.AddMinutes(-settings.ReminderLeadMinutes)
            });
        }

        return new PrayerSchedule
        {
            Date = day.Date,
            Prayers = prayers.OrderBy(prayer => prayer.TriggerAt).ToArray()
        };
    }

    private static IReadOnlyDictionary<string, string> ToPrayerMap(PrayerDaySchedule day) =>
        new Dictionary<string, string>
        {
            ["Fajr"] = day.Fajr,
            ["Dhuhr"] = day.Dhuhr,
            ["Asr"] = day.Asr,
            ["Maghrib"] = day.Maghrib,
            ["Isha"] = day.Isha
        };

    private static DateTimeOffset ParsePrayerTime(DateOnly date, string rawTime, string timeZoneId, TimeSpan localOffset)
    {
        var time = TimeOnly.FromDateTime(
            DateTime.ParseExact(
                rawTime,
                ["H:mm", "HH:mm"],
                CultureInfo.InvariantCulture,
                DateTimeStyles.None));
        var localDateTime = date.ToDateTime(time, DateTimeKind.Unspecified);

        var timeZone = ResolveTimeZone(timeZoneId);
        if (timeZone is null)
        {
            return new DateTimeOffset(localDateTime, localOffset);
        }

        var prayerOffset = timeZone.GetUtcOffset(localDateTime);
        return new DateTimeOffset(localDateTime, prayerOffset).ToLocalTime();
    }

    private static TimeZoneInfo? ResolveTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return null;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch
        {
            if (TimeZoneInfo.TryConvertIanaIdToWindowsId(timeZoneId, out var windowsId))
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(windowsId);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }
    }
}
