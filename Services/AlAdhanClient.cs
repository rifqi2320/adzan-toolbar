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

    public async Task<IReadOnlyList<PrayerDaySchedule>> GetPrayerDaysAsync(
        string city,
        string country,
        DateOnly startDate,
        int dayCount,
        CancellationToken cancellationToken)
    {
        var endDate = startDate.AddDays(dayCount - 1);
        var months = new HashSet<(int Year, int Month)>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            months.Add((date.Year, date.Month));
        }

        var allDays = new List<PrayerDaySchedule>();
        foreach (var (year, month) in months.OrderBy(entry => entry.Year).ThenBy(entry => entry.Month))
        {
            var monthDays = await GetMonthAsync(city, country, month, year, cancellationToken).ConfigureAwait(false);
            allDays.AddRange(monthDays);
        }

        return allDays
            .Where(day => day.Date >= startDate && day.Date <= endDate)
            .OrderBy(day => day.Date)
            .ToArray();
    }

    private async Task<IReadOnlyList<PrayerDaySchedule>> GetMonthAsync(
        string city,
        string country,
        int month,
        int year,
        CancellationToken cancellationToken)
    {
        var requestPath =
            $"v1/calendarByCity?city={Uri.EscapeDataString(city)}" +
            $"&country={Uri.EscapeDataString(country)}" +
            $"&method={PrayerApiDefaults.CalculationMethod}" +
            $"&month={month}&year={year}";

        using var response = await _httpClient.GetAsync(requestPath, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var payload = await JsonSerializer.DeserializeAsync<CalendarResponse>(
                stream,
                SerializerOptions,
                cancellationToken)
            .ConfigureAwait(false);

        if (payload?.Data is null)
        {
            throw new InvalidOperationException("Prayer timings were missing from the API response.");
        }

        return payload.Data
            .Where(day => day.Date?.Gregorian?.Date is not null && day.Timings is not null)
            .Select(day => new PrayerDaySchedule
            {
                Date = DateOnly.ParseExact(day.Date!.Gregorian!.Date!, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                TimeZoneId = day.Meta?.Timezone ?? string.Empty,
                Fajr = NormalizeTime(day.Timings!.Fajr),
                Dhuhr = NormalizeTime(day.Timings.Dhuhr),
                Asr = NormalizeTime(day.Timings.Asr),
                Maghrib = NormalizeTime(day.Timings.Maghrib),
                Isha = NormalizeTime(day.Timings.Isha)
            })
            .ToArray();
    }

    private static string NormalizeTime(string rawTime)
    {
        var match = TimeRegex.Match(rawTime);
        if (!match.Success)
        {
            throw new FormatException($"Could not parse prayer time '{rawTime}'.");
        }

        return match.Value;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    private sealed class CalendarResponse
    {
        public List<AlAdhanData>? Data { get; set; }
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
