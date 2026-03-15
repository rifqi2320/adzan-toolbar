using System;
using System.Collections.Generic;
using System.Linq;

namespace AdzanToolbar.Models;

internal sealed class PrayerSchedule
{
    public DateOnly Date { get; init; }

    public IReadOnlyList<PrayerTime> Prayers { get; init; } = Array.Empty<PrayerTime>();

    public PrayerTime? FindNext(DateTimeOffset now)
    {
        return Prayers
            .OrderBy(prayer => prayer.TriggerAt)
            .FirstOrDefault(prayer => prayer.TriggerAt > now);
    }
}
