using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AdzanToolbar.Theme;

namespace AdzanToolbar.Notifications;

internal sealed class AdhanPopupForm : Form
{
    private const int WsPopup = unchecked((int)0x80000000);
    private const int WsExTopmost = 0x00000008;
    private const int WsExToolWindow = 0x00000080;

    private readonly System.Windows.Forms.Timer _fadeTimer;
    private readonly System.Windows.Forms.Timer _lifetimeTimer;
    private readonly Label _titleLabel;
    private readonly Label _subtitleLabel;
    private readonly Label _prayerLabel;
    private readonly Label _timeLabel;
    private bool _fadingOut;

    public AdhanPopupForm(string title, string subtitle, string prayerName, string prayerTime)
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        BackColor = IslamicTheme.Emerald900;
        Size = new Size(360, 190);
        Padding = new Padding(18);
        DoubleBuffered = true;
        Opacity = 0;

        var outerPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            BackColor = IslamicTheme.Emerald900
        };

        _titleLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 24,
            Text = title,
            Font = IslamicTheme.BodyFont(10f, FontStyle.Bold),
            ForeColor = IslamicTheme.Gold300
        };

        _subtitleLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 24,
            Text = subtitle,
            Font = IslamicTheme.BodyFont(9.5f),
            ForeColor = Color.FromArgb(220, IslamicTheme.Parchment)
        };

        _prayerLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 54,
            Text = prayerName,
            Font = IslamicTheme.HeaderFont(24f),
            ForeColor = IslamicTheme.Parchment,
            Padding = new Padding(0, 16, 0, 0)
        };

        _timeLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 38,
            Text = prayerTime,
            Font = IslamicTheme.BodyFont(18f, FontStyle.Bold),
            ForeColor = IslamicTheme.Gold300
        };

        var dismissButton = new Button
        {
            Text = "Dismiss",
            Dock = DockStyle.Bottom
        };
        IslamicTheme.StyleButton(dismissButton, primary: false);
        dismissButton.Width = 96;
        dismissButton.Height = 34;
        dismissButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        dismissButton.Click += (_, _) => BeginFadeOut();

        outerPanel.Controls.Add(dismissButton);
        outerPanel.Controls.Add(_timeLabel);
        outerPanel.Controls.Add(_prayerLabel);
        outerPanel.Controls.Add(_subtitleLabel);
        outerPanel.Controls.Add(_titleLabel);
        Controls.Add(outerPanel);

        _fadeTimer = new System.Windows.Forms.Timer { Interval = 20 };
        _fadeTimer.Tick += (_, _) => HandleFadeTick();

        _lifetimeTimer = new System.Windows.Forms.Timer { Interval = 12000 };
        _lifetimeTimer.Tick += (_, _) =>
        {
            _lifetimeTimer.Stop();
            BeginFadeOut();
        };

        Shown += (_, _) =>
        {
            PlaceNearTray();
            _fadeTimer.Start();
            _lifetimeTimer.Start();
        };
        Click += (_, _) => BeginFadeOut();
        outerPanel.Click += (_, _) => BeginFadeOut();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.Style |= WsPopup;
            cp.ExStyle |= WsExTopmost | WsExToolWindow;
            return cp;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var rect = ClientRectangle;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var background = new LinearGradientBrush(rect, IslamicTheme.Emerald900, IslamicTheme.Lapis700, 35f);
        e.Graphics.FillRectangle(background, rect);

        using var borderPen = new Pen(IslamicTheme.Gold500, 2f);
        var borderRect = Rectangle.Inflate(rect, -1, -1);
        e.Graphics.DrawRectangle(borderPen, borderRect);

        using var motifPen = new Pen(Color.FromArgb(50, IslamicTheme.Gold300), 1.2f);
        for (var x = 24; x < Width - 24; x += 52)
        {
            for (var y = 24; y < Height - 24; y += 44)
            {
                e.Graphics.DrawEllipse(motifPen, x, y, 18, 18);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _fadeTimer.Dispose();
            _lifetimeTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    private void PlaceNearTray()
    {
        var workArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1280, 720);
        Location = new Point(workArea.Right - Width - 18, workArea.Bottom - Height - 18);
    }

    private void HandleFadeTick()
    {
        const double step = 0.12;

        if (_fadingOut)
        {
            Opacity -= step;
            if (Opacity <= 0)
            {
                _fadeTimer.Stop();
                Close();
            }

            return;
        }

        Opacity += step;
        if (Opacity >= 1)
        {
            Opacity = 1;
            _fadeTimer.Stop();
        }
    }

    private void BeginFadeOut()
    {
        if (_fadingOut)
        {
            return;
        }

        _fadingOut = true;
        _lifetimeTimer.Stop();
        _fadeTimer.Start();
    }
}
