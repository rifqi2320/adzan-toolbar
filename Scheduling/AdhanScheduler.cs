using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdzanToolbar.Models;
using AdzanToolbar.Notifications;
using AdzanToolbar.Services;
using AdzanToolbar.Settings;

namespace AdzanToolbar.Scheduling;

internal sealed class AdhanScheduler : IDisposable
{
    private readonly PrayerScheduleRepository _scheduleRepository;
    private readonly PopupNotifier _notifier;
    private readonly HashSet<string> _triggeredKeys = new(StringComparer.OrdinalIgnoreCase);

    private CancellationTokenSource? _cts;
    private Task? _runnerTask;
    private PrayerSchedule? _currentSchedule;
    private DateTimeOffset _lastRefreshAt;

    public AdhanScheduler(PrayerScheduleRepository scheduleRepository, PopupNotifier notifier)
    {
        _scheduleRepository = scheduleRepository;
        _notifier = notifier;
    }

    public event EventHandler<string>? StatusChanged;

    public event EventHandler<string>? ErrorOccurred;

    public async Task StartAsync(AppSettings settings)
    {
        if (_runnerTask is not null)
        {
            return;
        }

        _cts = new CancellationTokenSource();
        try
        {
            await RefreshScheduleAsync(settings, _cts.Token).ConfigureAwait(false);
            await TickAsync(settings, _cts.Token).ConfigureAwait(false);
            _runnerTask = RunAsync(settings, _cts.Token);
        }
        catch
        {
            _cts.Dispose();
            _cts = null;
            _currentSchedule = null;
            _lastRefreshAt = default;
            throw;
        }
    }

    public async Task RestartAsync(AppSettings settings)
    {
        await StopAsync().ConfigureAwait(false);
        await StartAsync(settings).ConfigureAwait(false);
    }

    public async Task StopAsync()
    {
        if (_cts is null || _runnerTask is null)
        {
            return;
        }

        _cts.Cancel();
        try
        {
            await _runnerTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown.
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
            _runnerTask = null;
            _currentSchedule = null;
            _lastRefreshAt = default;
            _triggeredKeys.Clear();
        }
    }

    private async Task RunAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(PrayerApiDefaults.PollingIntervalSeconds));
        while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
        {
            try
            {
                await TickAsync(settings, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex.Message);
                StatusChanged?.Invoke(this, "Scheduler retrying after error");
            }
        }
    }

    private async Task TickAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.Now;
        if (_currentSchedule is null || ShouldRefreshSchedule(now))
        {
            await RefreshScheduleAsync(settings, cancellationToken).ConfigureAwait(false);
        }

        if (_currentSchedule is null)
        {
            return;
        }

        foreach (var prayer in _currentSchedule.Prayers.Where(prayer => prayer.TriggerAt <= now))
        {
            var key = BuildTriggerKey(_currentSchedule.Date, prayer.Name);
            if (_triggeredKeys.Contains(key))
            {
                continue;
            }

            _notifier.ShowPrayerReminder(prayer.Name, prayer.DisplayTime, settings.City, settings.Country);
            _triggeredKeys.Add(key);
        }

        var nextPrayer = _currentSchedule.FindNext(now);
        if (nextPrayer is null)
        {
            StatusChanged?.Invoke(this, $"No more prayers today in {settings.City}");
            return;
        }

        StatusChanged?.Invoke(
            this,
            $"Next: {nextPrayer.Name} at {nextPrayer.AdhanAt:HH:mm} ({settings.City})");
    }

    private async Task RefreshScheduleAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        var previousDate = _currentSchedule?.Date;
        var today = DateOnly.FromDateTime(DateTime.Today);
        var now = DateTimeOffset.Now;
        _currentSchedule = await _scheduleRepository.GetPrayerScheduleAsync(settings, today, cancellationToken).ConfigureAwait(false);
        _lastRefreshAt = now;

        if (previousDate != _currentSchedule.Date)
        {
            _triggeredKeys.Clear();
        }

        foreach (var prayer in _currentSchedule.Prayers.Where(prayer => prayer.TriggerAt < now))
        {
            _triggeredKeys.Add(BuildTriggerKey(_currentSchedule.Date, prayer.Name));
        }

        var activePrayerNames = string.Join(", ", _currentSchedule.Prayers.Select(prayer => prayer.Name));
        StatusChanged?.Invoke(
            this,
            $"Loaded {activePrayerNames} for {_currentSchedule.Date:dd MMM} ({settings.City})");
    }

    private static string BuildTriggerKey(DateOnly date, string prayerName) => $"{date:yyyy-MM-dd}:{prayerName}";

    private bool ShouldRefreshSchedule(DateTimeOffset now)
    {
        if (_currentSchedule is null)
        {
            return true;
        }

        var nextPrayer = _currentSchedule.FindNext(now);
        var refreshInterval = TimeSpan.FromMinutes(15);
        var localDate = DateOnly.FromDateTime(now.Date);

        if (_currentSchedule.Date < localDate)
        {
            return true;
        }

        if (_currentSchedule.Date > localDate)
        {
            return now - _lastRefreshAt >= refreshInterval;
        }

        if (nextPrayer is null)
        {
            return now - _lastRefreshAt >= refreshInterval;
        }

        return false;
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
