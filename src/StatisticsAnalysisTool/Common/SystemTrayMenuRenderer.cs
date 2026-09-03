using System.Drawing;
using System.Windows.Forms;

namespace StatisticsAnalysisTool.Common;

public sealed class SystemTrayMenuRenderer : ToolStripProfessionalRenderer
{
    private static readonly Color HoverColor = Color.FromArgb(37, 45, 52);
    private static readonly Color PressedColor = Color.FromArgb(18, 50, 71);
    private static readonly Color BorderColor = Color.FromArgb(50, 59, 68);
    private static readonly Color DisabledForegroundColor = Color.FromArgb(113, 128, 141);

    public SystemTrayMenuRenderer()
    {
        RoundedEdges = false;
    }

    public static Color BackgroundColor { get; } = Color.FromArgb(20, 25, 31);
    public static Color ForegroundColor { get; } = Color.FromArgb(243, 246, 248);

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs eventArgs)
    {
        eventArgs.Graphics.Clear(BackgroundColor);
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs eventArgs)
    {
        var backgroundColor = eventArgs.Item.Pressed ? PressedColor : eventArgs.Item.Selected ? HoverColor : BackgroundColor;

        using var backgroundBrush = new SolidBrush(backgroundColor);
        eventArgs.Graphics.FillRectangle(backgroundBrush, new Rectangle(Point.Empty, eventArgs.Item.Size));
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs eventArgs)
    {
        eventArgs.TextColor = eventArgs.Item.Enabled ? ForegroundColor : DisabledForegroundColor;
        base.OnRenderItemText(eventArgs);
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs eventArgs)
    {
        var separatorY = eventArgs.Item.Height / 2;
        using var separatorPen = new Pen(BorderColor);
        eventArgs.Graphics.DrawLine(separatorPen, 8, separatorY, eventArgs.Item.Width - 8, separatorY);
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs eventArgs)
    {
        if (eventArgs.ToolStrip.Width <= 0 || eventArgs.ToolStrip.Height <= 0)
        {
            return;
        }

        var borderBounds = new Rectangle(0, 0, eventArgs.ToolStrip.Width - 1, eventArgs.ToolStrip.Height - 1);
        using var borderPen = new Pen(BorderColor);
        eventArgs.Graphics.DrawRectangle(borderPen, borderBounds);
    }
}