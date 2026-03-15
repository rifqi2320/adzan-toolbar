using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdzanToolbar.Notifications;
using AdzanToolbar.Scheduling;
using AdzanToolbar.Services;
using AdzanToolbar.Settings;
using AdzanToolbar.Tray;

namespace AdzanToolbar.App;

internal sealed class AdzanApplicationContext : ApplicationContext
{
    private readonly JsonSettingsStore _settingsStore;
    private readonly TrayHost _trayHost;
    private readonly TrayNotifier _notifier;
    private readonly AlAdhanClient _adhanClient;
    private readonly AdhanScheduler _scheduler;
    private SettingsForm? _settingsForm;
    private bool _isExiting;

    public AdzanApplicationContext()
    {
        _settingsStore = new JsonSettingsStore();
        var settings = _settingsStore.Load();

        _trayHost = new TrayHost();
        _trayHost.OpenSettingsRequested += (_, _) => OpenSettings();
        _trayHost.TestNotificationRequested += (_, _) => ShowTestNotification();
        _trayHost.RestartSchedulerRequested += async (_, _) => await RestartSchedulerAsync();
        _trayHost.ExitRequested += async (_, _) => await ExitApplicationAsync();

        _notifier = new TrayNotifier(_trayHost);
        _adhanClient = new AlAdhanClient(new HttpClient());
        _scheduler = new AdhanScheduler(_adhanClient, _notifier);
        _scheduler.StatusChanged += (_, status) => _trayHost.SetStatus(status);
        _scheduler.ErrorOccurred += (_, message) => _trayHost.ShowError(message);

        _ = InitializeAsync(settings);
    }

    private async Task InitializeAsync(AppSettings settings)
    {
        try
        {
            _trayHost.SetStatus("Starting scheduler...");
            await _scheduler.StartAsync(settings);
        }
        catch (Exception ex)
        {
            _trayHost.ShowError($"Failed to start: {ex.Message}");
            _trayHost.SetStatus("Scheduler failed to start");
        }
    }

    private void OpenSettings()
    {
        if (_settingsForm is not null && !_settingsForm.IsDisposed)
        {
            _settingsForm.BringToFront();
            _settingsForm.Focus();
            return;
        }

        var currentSettings = _settingsStore.Load();
        _settingsForm = new SettingsForm(currentSettings);
        _settingsForm.FormClosed += (_, _) => _settingsForm = null;
        _settingsForm.SettingsSaved += async (_, savedSettings) =>
        {
            _settingsStore.Save(savedSettings);
            await RestartSchedulerAsync(savedSettings);
        };

        _settingsForm.Show();
    }

    private void ShowTestNotification()
    {
        var settings = _settingsStore.Load();
        _notifier.ShowTest(settings.City, settings.Country);
    }

    private async Task RestartSchedulerAsync(AppSettings? settings = null)
    {
        try
        {
            var effectiveSettings = settings ?? _settingsStore.Load();
            _trayHost.SetStatus("Restarting scheduler...");
            await _scheduler.RestartAsync(effectiveSettings);
        }
        catch (Exception ex)
        {
            _trayHost.ShowError($"Restart failed: {ex.Message}");
            _trayHost.SetStatus("Scheduler restart failed");
        }
    }

    private async Task ExitApplicationAsync()
    {
        if (_isExiting)
        {
            return;
        }

        _isExiting = true;

        try
        {
            await _scheduler.StopAsync();
        }
        catch
        {
            // Ignore shutdown failures.
        }

        _adhanClient.Dispose();
        _trayHost.Dispose();
        ExitThread();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _settingsForm?.Dispose();
            _scheduler.Dispose();
            _adhanClient.Dispose();
            _trayHost.Dispose();
        }

        base.Dispose(disposing);
    }
}
