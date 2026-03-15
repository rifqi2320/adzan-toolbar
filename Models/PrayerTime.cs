using System;

namespace AdzanToolbar.Models;

internal sealed class PrayerTime
{
    public required string Name { get; init; }

    public required DateTimeOffset AdhanAt { get; init; }

    public required DateTimeOffset TriggerAt { get; init; }
}
