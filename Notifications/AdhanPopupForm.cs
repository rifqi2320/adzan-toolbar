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
        Size = new Size(472, 256);
        Padding = new Padding(0);
        DoubleBuffered = true;
        Opacity = 0;

        var card = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 22, 24, 20),
            BackColor = Color.Transparent
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 12));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

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
            Font = IslamicTheme.HeaderFont(30f),
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

        var detailsLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 22,
            Text = "Prepare for the adhan and prayer.",
            Font = IslamicTheme.BodyFont(9.5f),
            ForeColor = Color.FromArgb(188, IslamicTheme.Parchment),
            TextAlign = ContentAlignment.TopLeft
        };

        var leftStack = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };
        leftStack.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        leftStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        leftStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        leftStack.Controls.Add(prayerLabel, 0, 0);
        leftStack.Controls.Add(messageLabel, 0, 1);
        leftStack.Controls.Add(detailsLabel, 0, 2);

        var timeCard = new Panel
        {
            Dock = DockStyle.Top,
            Height = 96,
            Margin = new Padding(14, 6, 0, 0),
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
            Font = IslamicTheme.HeaderFont(22f),
            ForeColor = IslamicTheme.Emerald900,
            TextAlign = ContentAlignment.MiddleCenter
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

        var dismissButton = new Button
        {
            Text = "Dismiss",
            Dock = DockStyle.Fill,
            Height = 38
        };
        IslamicTheme.StyleFlatActionButton(dismissButton);
        dismissButton.Click += (_, _) => BeginFadeOut();

        var rightStack = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0)
        };
        rightStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 106));
        rightStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        rightStack.Controls.Add(timeCard, 0, 0);
        rightStack.Controls.Add(dismissButton, 0, 1);

        var bodyLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };
        bodyLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        bodyLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        bodyLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        bodyLayout.Controls.Add(leftStack, 0, 0);
        bodyLayout.Controls.Add(rightStack, 1, 0);

        var accentLine = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 2,
            BackColor = Color.FromArgb(88, IslamicTheme.Gold300),
            Margin = new Padding(0, 4, 0, 8)
        };

        layout.Controls.Add(topStack, 0, 0);
        layout.Controls.Add(accentLine, 0, 1);
        layout.Controls.Add(bodyLayout, 0, 2);

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
