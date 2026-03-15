using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using AdzanToolbar.Theme;

namespace AdzanToolbar.Tray;

internal sealed class TrayHost : IDisposable
{
    private readonly SynchronizationContext _uiContext;
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _statusItem;
    private readonly Icon _appIcon;
    private bool _disposed;

    public TrayHost()
    {
        _uiContext = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
        _statusItem = new ToolStripMenuItem("Starting...")
        {
            Enabled = false
        };

        var openSettingsItem = new ToolStripMenuItem("Open Settings");
        openSettingsItem.Click += (_, _) => OpenSettingsRequested?.Invoke(this, EventArgs.Empty);

        var testNotificationItem = new ToolStripMenuItem("Trigger Test Notification");
        testNotificationItem.Click += (_, _) => TestNotificationRequested?.Invoke(this, EventArgs.Empty);

        var restartItem = new ToolStripMenuItem("Restart Scheduler");
        restartItem.Click += (_, _) => RestartSchedulerRequested?.Invoke(this, EventArgs.Empty);

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);

        var menu = new ContextMenuStrip();
        menu.Renderer = new IslamicMenuRenderer();
        menu.BackColor = IslamicTheme.Parchment;
        menu.ForeColor = IslamicTheme.Ink;
        menu.Font = IslamicTheme.BodyFont(10f);
        menu.Items.Add(_statusItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(openSettingsItem);
        menu.Items.Add(testNotificationItem);
        menu.Items.Add(restartItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);

        _appIcon = TrayIconFactory.Create();
        _notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = menu,
            Icon = _appIcon,
            Text = "Adzan Toolbar",
            Visible = true
        };
        _notifyIcon.DoubleClick += (_, _) => OpenSettingsRequested?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? OpenSettingsRequested;

    public event EventHandler? TestNotificationRequested;

    public event EventHandler? RestartSchedulerRequested;

    public event EventHandler? ExitRequested;

    public void SetStatus(string status)
    {
        RunOnUiThread(() =>
        {
            _statusItem.Text = status;
            _notifyIcon.Text = status.Length <= 63 ? status : $"{status[..60]}...";
        });
    }

    public void ShowBalloon(string title, string message)
    {
        RunOnUiThread(() =>
        {
            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = message;
            _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            _notifyIcon.ShowBalloonTip(10000);
        });
    }

    public void ShowError(string message)
    {
        RunOnUiThread(() =>
        {
            _notifyIcon.BalloonTipTitle = "Adzan Toolbar Error";
            _notifyIcon.BalloonTipText = message;
            _notifyIcon.BalloonTipIcon = ToolTipIcon.Error;
            _notifyIcon.ShowBalloonTip(10000);
        });
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        RunOnUiThread(() =>
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _appIcon.Dispose();
        });
    }

    private void RunOnUiThread(Action action)
    {
        if (SynchronizationContext.Current == _uiContext)
        {
            action();
            return;
        }

        _uiContext.Send(_ => action(), null);
    }
}
