namespace AdzanToolbar.Models;

internal sealed class PrayerDaySchedule
{
    public required DateOnly Date { get; init; }

    public required string TimeZoneId { get; init; }

    public required string Fajr { get; init; }

    public required string Dhuhr { get; init; }

    public required string Asr { get; init; }

    public required string Maghrib { get; init; }

    public required string Isha { get; init; }
}
