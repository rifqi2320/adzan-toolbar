using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AdzanToolbar.Theme;

internal sealed class IslamicHeaderPanel : Panel
{
    public string TitleText { get; set; } = "Adzan Reminder";

    public string SubtitleText { get; set; } = "Prayer times and reminders inspired by Islamic geometry";

    public IslamicHeaderPanel()
    {
        DoubleBuffered = true;
        Dock = DockStyle.Top;
        Height = 170;
        Padding = new Padding(24, 22, 24, 22);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        using var background = new LinearGradientBrush(
            ClientRectangle,
            IslamicTheme.Emerald900,
            IslamicTheme.Lapis700,
            18f);
        g.FillRectangle(background, ClientRectangle);

        DrawMotif(g);
        DrawArch(g);
        DrawTexts(g);
        DrawBorder(g);
    }

    private void DrawTexts(Graphics g)
    {
        var titleBounds = new Rectangle(24, 26, Width - 170, 48);
        var subtitleBounds = new Rectangle(24, 78, Width - 180, 52);

        using var titleBrush = new SolidBrush(IslamicTheme.Parchment);
        using var subtitleBrush = new SolidBrush(Color.FromArgb(220, IslamicTheme.Gold300));
        using var titleFormat = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
        using var subtitleFormat = new StringFormat { Alignment = StringAlignment.Near };

        g.DrawString(TitleText, IslamicTheme.HeaderFont(22f), titleBrush, titleBounds, titleFormat);
        g.DrawString(SubtitleText, IslamicTheme.BodyFont(9.5f), subtitleBrush, subtitleBounds, subtitleFormat);
    }

    private void DrawArch(Graphics g)
    {
        var archArea = new Rectangle(Width - 150, 24, 108, 122);

        using var archBrush = new SolidBrush(Color.FromArgb(36, IslamicTheme.Gold300));
        using var archPen = new Pen(Color.FromArgb(170, IslamicTheme.Gold300), 2f);
        using var columnBrush = new SolidBrush(Color.FromArgb(45, IslamicTheme.Parchment));

        g.FillRectangle(columnBrush, archArea.X + 12, archArea.Y + 38, 12, 80);
        g.FillRectangle(columnBrush, archArea.Right - 24, archArea.Y + 38, 12, 80);

        using var path = new GraphicsPath();
        path.AddLine(archArea.X + 10, archArea.Bottom, archArea.X + 10, archArea.Y + 48);
        path.AddArc(archArea.X + 10, archArea.Y + 4, archArea.Width - 20, 88, 180, 180);
        path.AddLine(archArea.Right - 10, archArea.Y + 48, archArea.Right - 10, archArea.Bottom);
        path.CloseFigure();

        g.FillPath(archBrush, path);
        g.DrawPath(archPen, path);

        using var innerBrush = new SolidBrush(Color.FromArgb(65, IslamicTheme.Emerald500));
        var inner = Rectangle.Inflate(archArea, -22, -18);
        g.FillPie(innerBrush, inner, 180, 180);
        g.FillRectangle(innerBrush, inner.X, inner.Y + inner.Height / 2, inner.Width, inner.Height / 2);

        DrawCrescent(g, new Point(archArea.X + 57, archArea.Y + 35), 16);
    }

    private void DrawCrescent(Graphics g, Point center, int radius)
    {
        using var goldBrush = new SolidBrush(IslamicTheme.Gold300);
        using var cutBrush = new SolidBrush(Color.FromArgb(0, 0, 0, 0));

        var outer = new Rectangle(center.X - radius, center.Y - radius, radius * 2, radius * 2);
        var inner = new Rectangle(center.X - radius + 8, center.Y - radius + 3, radius * 2 - 6, radius * 2 - 6);

        using var crescent = new GraphicsPath(FillMode.Winding);
        crescent.AddEllipse(outer);
        crescent.AddEllipse(inner);

        g.FillPath(goldBrush, crescent);

        using var starBrush = new SolidBrush(IslamicTheme.Parchment);
        FillStar(g, starBrush, center.X + 22, center.Y - 6, 6f, 5, -90f);
    }

    private void DrawMotif(Graphics g)
    {
        using var pen = new Pen(Color.FromArgb(60, IslamicTheme.Gold300), 1.5f);
        for (var x = 250; x < Width - 150; x += 56)
        {
            for (var y = 20; y < Height - 20; y += 54)
            {
                DrawRosette(g, pen, x, y, 18f);
            }
        }
    }

    private void DrawRosette(Graphics g, Pen pen, float centerX, float centerY, float radius)
    {
        var points = new PointF[8];
        for (var i = 0; i < points.Length; i++)
        {
            var angle = (float)(-Math.PI / 2 + (i * Math.PI / 4));
            points[i] = new PointF(
                centerX + (float)Math.Cos(angle) * radius,
                centerY + (float)Math.Sin(angle) * radius);
        }

        g.DrawPolygon(pen, points);
        g.DrawEllipse(pen, centerX - radius / 2, centerY - radius / 2, radius, radius);
    }

    private void DrawBorder(Graphics g)
    {
        using var pen = new Pen(IslamicTheme.Gold500, 2f);
        var y = Height - 14;
        for (var x = 18; x < Width - 18; x += 18)
        {
            g.DrawArc(pen, x, y - 10, 18, 14, 0, 180);
        }
    }

    private static void FillStar(Graphics g, Brush brush, float cx, float cy, float radius, int points, float rotationDegrees)
    {
        var vertices = new PointF[points * 2];
        var rotation = rotationDegrees * MathF.PI / 180f;
        for (var i = 0; i < vertices.Length; i++)
        {
            var r = i % 2 == 0 ? radius : radius / 2.5f;
            var angle = rotation + (i * MathF.PI / points);
            vertices[i] = new PointF(cx + MathF.Cos(angle) * r, cy + MathF.Sin(angle) * r);
        }

        g.FillPolygon(brush, vertices);
    }
}
