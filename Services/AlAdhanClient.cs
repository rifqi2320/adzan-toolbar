using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AdzanToolbar.Models;
using AdzanToolbar.Settings;

namespace AdzanToolbar.Services;

internal sealed class AlAdhanClient : IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private static readonly Regex TimeRegex = new(@"^\d{1,2}:\d{2}", RegexOptions.Compiled);
    private readonly HttpClient _httpClient;

    public AlAdhanClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.aladhan.com/");
    }

    public async Task<PrayerSchedule> GetPrayerScheduleAsync(AppSettings settings, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var requestPath =
            $"v1/timingsByCity?city={Uri.EscapeDataString(settings.City)}" +
            $"&country={Uri.EscapeDataString(settings.Country)}" +
            $"&method={settings.CalculationMethod}";

        using var response = await _httpClient.GetAsync(requestPath, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var payload = await JsonSerializer.DeserializeAsync<AlAdhanResponse>(
                stream,
                SerializerOptions,
                cancellationToken)
            .ConfigureAwait(false);

        if (payload?.Data?.Timings is null || payload.Data.Date?.Gregorian?.Date is null)
        {
            throw new InvalidOperationException("Prayer timings were missing from the API response.");
        }

        var scheduleDate = DateOnly.ParseExact(
            payload.Data.Date.Gregorian.Date,
            "dd-MM-yyyy",
            CultureInfo.InvariantCulture);

        var timeZone = TryResolveTimeZone(payload.Data.Meta?.Timezone);
        var prayers = new List<PrayerTime>();
        foreach (var entry in payload.Data.Timings.ToPrayerMap())
        {
            if (!settings.Prayers.IsEnabled(entry.Key))
            {
                continue;
            }

            var adhanTime = ParsePrayerTime(scheduleDate, entry.Value, timeZone, now.Offset);
            prayers.Add(new PrayerTime
            {
                Name = entry.Key,
                AdhanAt = adhanTime,
                TriggerAt = adhanTime.AddMinutes(-settings.ReminderLeadMinutes)
            });
        }

        return new PrayerSchedule
        {
            Date = scheduleDate,
            Prayers = prayers.OrderBy(prayer => prayer.TriggerAt).ToArray()
        };
    }

    private static DateTimeOffset ParsePrayerTime(DateOnly date, string rawTime, TimeZoneInfo? timeZone, TimeSpan localOffset)
    {
        var match = TimeRegex.Match(rawTime);
        if (!match.Success)
        {
            throw new FormatException($"Could not parse prayer time '{rawTime}'.");
        }

        var time = TimeOnly.FromDateTime(
            DateTime.ParseExact(
                match.Value,
                ["H:mm", "HH:mm"],
                CultureInfo.InvariantCulture,
                DateTimeStyles.None));
        var localDateTime = date.ToDateTime(time, DateTimeKind.Unspecified);

        if (timeZone is null)
        {
            return new DateTimeOffset(localDateTime, localOffset);
        }

        var prayerOffset = timeZone.GetUtcOffset(localDateTime);
        return new DateTimeOffset(localDateTime, prayerOffset).ToLocalTime();
    }

    private static TimeZoneInfo? TryResolveTimeZone(string? timeZoneId)
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
            return null;
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    private sealed class AlAdhanResponse
    {
        public AlAdhanData? Data { get; set; }
    }

    private sealed class AlAdhanData
    {
        public AlAdhanTimings? Timings { get; set; }

        public AlAdhanDate? Date { get; set; }

        public AlAdhanMeta? Meta { get; set; }
    }

    private sealed class AlAdhanTimings
    {
        public string Fajr { get; set; } = string.Empty;

        public string Dhuhr { get; set; } = string.Empty;

        public string Asr { get; set; } = string.Empty;

        public string Maghrib { get; set; } = string.Empty;

        public string Isha { get; set; } = string.Empty;

        public IReadOnlyDictionary<string, string> ToPrayerMap() =>
            new Dictionary<string, string>
            {
                ["Fajr"] = Fajr,
                ["Dhuhr"] = Dhuhr,
                ["Asr"] = Asr,
                ["Maghrib"] = Maghrib,
                ["Isha"] = Isha
            };
    }

    private sealed class AlAdhanDate
    {
        public AlAdhanGregorianDate? Gregorian { get; set; }
    }

    private sealed class AlAdhanGregorianDate
    {
        [JsonPropertyName("date")]
        public string? Date { get; set; }
    }

    private sealed class AlAdhanMeta
    {
        public string? Timezone { get; set; }
    }
}
