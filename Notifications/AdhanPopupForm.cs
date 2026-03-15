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
    private bool _fadingOut;

    public AdhanPopupForm(string title, string subtitle, string prayerName, string prayerTime)
    {
        SuspendLayout();
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        BackColor = IslamicTheme.Emerald900;
        Size = new Size(440, 232);
        Padding = new Padding(0);
        DoubleBuffered = true;
        Opacity = 0;

        var card = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(22, 20, 22, 18),
            BackColor = Color.Transparent
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            BackColor = Color.Transparent
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 16));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

        var titleLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = title,
            Font = IslamicTheme.BodyFont(10f, FontStyle.Bold),
            ForeColor = IslamicTheme.Gold300,
            TextAlign = ContentAlignment.BottomLeft
        };

        var subtitleLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = subtitle,
            Font = IslamicTheme.BodyFont(9.5f),
            ForeColor = Color.FromArgb(220, IslamicTheme.Parchment),
            TextAlign = ContentAlignment.TopLeft
        };

        var topStack = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0)
        };
        topStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        topStack.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        topStack.Controls.Add(titleLabel, 0, 0);
        topStack.Controls.Add(subtitleLabel, 0, 1);

        var prayerLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = prayerName,
            Font = IslamicTheme.HeaderFont(26f),
            ForeColor = IslamicTheme.Parchment,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var messageLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 28,
            Text = "Time for prayer",
            Font = IslamicTheme.BodyFont(10.5f),
            ForeColor = Color.FromArgb(224, IslamicTheme.Parchment),
            TextAlign = ContentAlignment.TopLeft
        };

        var centerStack = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };
        centerStack.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        centerStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        centerStack.Controls.Add(prayerLabel, 0, 0);
        centerStack.Controls.Add(messageLabel, 0, 1);

        var timeCard = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(12, 4, 0, 8),
            Padding = new Padding(14, 12, 14, 12),
            BackColor = Color.FromArgb(236, 228, 207)
        };

        var timeCaption = new Label
        {
            Dock = DockStyle.Top,
            Height = 20,
            Text = "Scheduled",
            Font = IslamicTheme.BodyFont(9f, FontStyle.Bold),
            ForeColor = IslamicTheme.Emerald900,
            TextAlign = ContentAlignment.MiddleCenter
        };

        var timeLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = prayerTime,
            Font = IslamicTheme.HeaderFont(18f),
            ForeColor = IslamicTheme.Emerald900,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoEllipsis = true
        };
        var timeStack = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        timeStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
        timeStack.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        timeStack.Controls.Add(timeCaption, 0, 0);
        timeStack.Controls.Add(timeLabel, 0, 1);
        timeCard.Controls.Add(timeStack);

        var dismissButton = new ThemedButton
        {
            Text = "Dismiss",
            Anchor = AnchorStyles.Right,
            Width = 108
        };
        IslamicTheme.StyleFlatActionButton(dismissButton);
        dismissButton.Click += (_, _) => BeginFadeOut();

        var footerPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };
        footerPanel.Controls.Add(dismissButton);

        var accentLine = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 2,
            BackColor = Color.FromArgb(88, IslamicTheme.Gold300),
            Margin = new Padding(0, 4, 0, 8)
        };

        layout.Controls.Add(topStack, 0, 0);
        layout.SetColumnSpan(topStack, 2);
        layout.Controls.Add(accentLine, 0, 1);
        layout.SetColumnSpan(accentLine, 2);
        layout.Controls.Add(centerStack, 0, 2);
        layout.Controls.Add(timeCard, 1, 2);
        layout.Controls.Add(footerPanel, 0, 3);
        layout.SetColumnSpan(footerPanel, 2);

        card.Controls.Add(layout);
        Controls.Add(card);

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
        card.Click += (_, _) => BeginFadeOut();
        ResumeLayout(performLayout: true);
    }

    protected override bool ShowWithoutActivation => true;

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

        using var motifPen = new Pen(Color.FromArgb(28, IslamicTheme.Gold300), 1.1f);
        for (var x = 34; x < Width - 30; x += 76)
        {
            for (var y = 28; y < Height - 26; y += 62)
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
