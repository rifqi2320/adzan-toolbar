using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace AdzanToolbar.Notifications;

internal sealed class PopupNotifier : IDisposable
{
    private readonly SynchronizationContext _uiContext;
    private readonly Queue<PopupRequest> _queue = new();
    private AdhanPopupForm? _activePopup;
    private bool _disposed;

    public PopupNotifier()
    {
        _uiContext = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
    }

    public void ShowPrayerReminder(string prayerName, string prayerTime, string city, string country)
    {
        Enqueue(new PopupRequest
        {
            Title = $"{prayerName} Adhan",
            Subtitle = $"{city}, {country}",
            PrayerName = prayerName,
            PrayerTime = prayerTime
        });
    }

    public void ShowTest(string city, string country)
    {
        Enqueue(new PopupRequest
        {
            Title = "Adzan Popup Test",
            Subtitle = $"{city}, {country}",
            PrayerName = "Reminder",
            PrayerTime = "Now"
        });
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _uiContext.Send(_ =>
        {
            _activePopup?.Close();
            _activePopup?.Dispose();
            _activePopup = null;
            _queue.Clear();
        }, null);
    }

    private void Enqueue(PopupRequest request)
    {
        if (_disposed)
        {
            return;
        }

        _uiContext.Post(_ =>
        {
            _queue.Enqueue(request);
            ShowNextIfIdle();
        }, null);
    }

    private void ShowNextIfIdle()
    {
        if (_activePopup is not null || _queue.Count == 0)
        {
            return;
        }

        var request = _queue.Dequeue();
        _activePopup = new AdhanPopupForm(request.Title, request.Subtitle, request.PrayerName, request.PrayerTime);
        _activePopup.FormClosed += (_, _) =>
        {
            _activePopup = null;
            ShowNextIfIdle();
        };
        _activePopup.Show();
    }

    private sealed class PopupRequest
    {
        public string Title { get; init; } = string.Empty;

        public string Subtitle { get; init; } = string.Empty;

        public string PrayerName { get; init; } = string.Empty;

        public string PrayerTime { get; init; } = string.Empty;
    }
}
