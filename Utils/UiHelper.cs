using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WeChatMassTool.Config;

namespace WeChatMassTool.Utils;

/// <summary>
/// UI 绘制工具类，集中所有自定义绘制逻辑
/// </summary>
public static class UiHelper
{
    #region P/Invoke

    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn(
        int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
        int nWidthEllipse, int nHeightEllipse);

    [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string? pszSubIdList);

    #endregion

    #region 圆角路径

    /// <summary>
    /// 创建圆角矩形路径
    /// </summary>
    public static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int r = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2);
        if (r <= 0)
        {
            path.AddRectangle(rect);
            return path;
        }

        int d = r * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    /// <summary>
    /// 创建顶部圆角的矩形路径（底部直角，用于 Tab 内容区）
    /// </summary>
    public static GraphicsPath CreateTopRoundedPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int r = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2);
        if (r <= 0)
        {
            path.AddRectangle(rect);
            return path;
        }

        int d = r * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom);
        path.CloseFigure();
        return path;
    }

    #endregion

    #region Region 圆角

    /// <summary>
    /// 通过 Win32 Region 设置控件圆角
    /// </summary>
    public static void ApplyRoundedRegion(Control control, int radius)
    {
        if (control.Width <= 0 || control.Height <= 0) return;
        try
        {
            var rgn = CreateRoundRectRgn(0, 0, control.Width, control.Height, radius, radius);
            control.Region = Region.FromHrgn(rgn);
        }
        catch { }
    }

    #endregion

    #region GDI+ 绘制

    /// <summary>
    /// 绘制圆角矩形
    /// </summary>
    public static void DrawRoundedRectangle(Graphics g, Rectangle rect, int radius,
        Color fillColor, Color? borderColor = null, int borderWidth = 1)
    {
        using var path = CreateRoundedPath(rect, radius);
        using var fillBrush = new SolidBrush(fillColor);
        g.FillPath(fillBrush, path);

        if (borderColor.HasValue && borderWidth > 0)
        {
            using var pen = new Pen(borderColor.Value, borderWidth);
            g.DrawPath(pen, path);
        }
    }

    /// <summary>
    /// 绘制顶部圆角矩形（底部直角）
    /// </summary>
    public static void DrawTopRoundedRectangle(Graphics g, Rectangle rect, int radius,
        Color fillColor, Color? borderColor = null, int borderWidth = 1)
    {
        using var path = CreateTopRoundedPath(rect, radius);
        using var fillBrush = new SolidBrush(fillColor);
        g.FillPath(fillBrush, path);

        if (borderColor.HasValue && borderWidth > 0)
        {
            using var pen = new Pen(borderColor.Value, borderWidth);
            g.DrawPath(pen, path);
        }
    }

    /// <summary>
    /// 绘制渐变填充的圆角矩形
    /// </summary>
    public static void DrawGradientFill(Graphics g, Rectangle rect, Color startColor, Color endColor,
        int radius, float angle = 0f)
    {
        using var path = CreateRoundedPath(rect, radius);
        using var brush = new LinearGradientBrush(rect, startColor, endColor, angle);
        g.FillPath(brush, path);
    }

    #endregion

    #region 按钮悬停效果

    private static readonly Dictionary<Control, Color> _buttonBaseColors = new();

    /// <summary>
    /// 为按钮附加悬停/按下效果
    /// </summary>
    public static void AttachButtonEffects(Button btn, Color baseColor, Color? hoverColor = null, Color? pressColor = null)
    {
        var hover = hoverColor ?? ThemeColors.ButtonHover;
        var press = pressColor ?? ThemeColors.ButtonPress;

        _buttonBaseColors[btn] = baseColor;

        btn.MouseEnter += (s, e) =>
        {
            if (btn.Enabled) btn.BackColor = hover;
        };
        btn.MouseLeave += (s, e) =>
        {
            btn.BackColor = _buttonBaseColors.TryGetValue(btn, out var bc) ? bc : baseColor;
        };
        btn.MouseDown += (s, e) =>
        {
            if (btn.Enabled && e.Button == MouseButtons.Left)
                btn.BackColor = press;
        };
        btn.MouseUp += (s, e) =>
        {
            if (btn.Enabled) btn.BackColor = hover;
        };
    }

    /// <summary>
    /// 更新按钮基础颜色（用于动态变色按钮如编辑模式）
    /// </summary>
    public static void UpdateButtonBaseColor(Button btn, Color newBaseColor, Color? hoverColor = null, Color? pressColor = null)
    {
        _buttonBaseColors[btn] = newBaseColor;
        btn.BackColor = newBaseColor;
    }

    #endregion

    #region 暗色滚动条

    /// <summary>
    /// 启用暗色滚动条（Win10 1809+）
    /// </summary>
    public static void EnableDarkScrollBar(Control control)
    {
        if (control.IsHandleCreated)
        {
            try { SetWindowTheme(control.Handle, "DarkMode_Explorer", null); }
            catch { }
        }
        else
        {
            control.HandleCreated += (s, e) =>
            {
                try { SetWindowTheme(control.Handle, "DarkMode_Explorer", null); }
                catch { }
            };
        }
    }

    #endregion
}
