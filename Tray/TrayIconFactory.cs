using System.Drawing;
using System.Drawing.Drawing2D;
using AdzanToolbar.Theme;

namespace AdzanToolbar.Tray;

internal static class TrayIconFactory
{
    public static Icon Create()
    {
        using var bitmap = new Bitmap(64, 64);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);

        using var background = new LinearGradientBrush(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            IslamicTheme.Emerald900,
            IslamicTheme.Emerald500,
            45f);
        graphics.FillEllipse(background, 4, 4, 56, 56);

        using var ringPen = new Pen(IslamicTheme.Gold300, 3f);
        graphics.DrawEllipse(ringPen, 6, 6, 52, 52);

        using var crescentBrush = new SolidBrush(IslamicTheme.Gold300);
        graphics.FillEllipse(crescentBrush, 16, 13, 22, 22);
        using var cutBrush = new SolidBrush(IslamicTheme.Emerald500);
        graphics.FillEllipse(cutBrush, 22, 13, 22, 22);

        using var mosqueBrush = new SolidBrush(IslamicTheme.Parchment);
        graphics.FillRectangle(mosqueBrush, 18, 33, 28, 14);
        graphics.FillRectangle(mosqueBrush, 44, 26, 4, 21);

        using var archPath = new GraphicsPath();
        archPath.AddLine(20, 47, 20, 39);
        archPath.AddArc(20, 29, 24, 20, 180, 180);
        archPath.AddLine(44, 39, 44, 47);
        archPath.CloseFigure();
        graphics.FillPath(mosqueBrush, archPath);

        graphics.FillRectangle(new SolidBrush(IslamicTheme.Gold500), 45, 23, 2, 6);
        graphics.FillEllipse(crescentBrush, 43, 19, 6, 6);
        graphics.FillEllipse(cutBrush, 45, 19, 6, 6);

        return Icon.FromHandle(bitmap.GetHicon());
    }
}
