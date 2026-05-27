using System.Drawing;
using System.Runtime.InteropServices;

namespace WeChatMassTool.Utils;

/// <summary>
/// 窗口截图工具，通过 Win32 API 捕获指定窗口图像
/// </summary>
public static class WindowCapture
{
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdc, int x, int y, int cx, int cy,
        IntPtr hdcSrc, int x1, int y1, int rop);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr ho);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    private const int SRCCOPY = 0x00CC0020;

    /// <summary>
    /// 捕获指定窗口的完整截图
    /// </summary>
    public static Bitmap? CaptureWindow(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero)
            return null;

        GetWindowRect(hWnd, out var rect);
        var width = rect.Right - rect.Left;
        var height = rect.Bottom - rect.Top;

        if (width <= 0 || height <= 0)
            return null;

        IntPtr hdcWindow = IntPtr.Zero;
        IntPtr hdcMem = IntPtr.Zero;
        IntPtr hBitmap = IntPtr.Zero;
        IntPtr hOld = IntPtr.Zero;

        try
        {
            hdcWindow = GetWindowDC(hWnd);
            if (hdcWindow == IntPtr.Zero)
                return null;

            hdcMem = CreateCompatibleDC(hdcWindow);
            if (hdcMem == IntPtr.Zero)
                return null;

            hBitmap = CreateCompatibleBitmap(hdcWindow, width, height);
            if (hBitmap == IntPtr.Zero)
                return null;

            hOld = SelectObject(hdcMem, hBitmap);

            if (!BitBlt(hdcMem, 0, 0, width, height, hdcWindow, 0, 0, SRCCOPY))
                return null;

            SelectObject(hdcMem, hOld);

            return Image.FromHbitmap(hBitmap);
        }
        finally
        {
            if (hOld != IntPtr.Zero && hdcMem != IntPtr.Zero)
                SelectObject(hdcMem, hOld);
            if (hBitmap != IntPtr.Zero)
                DeleteObject(hBitmap);
            if (hdcMem != IntPtr.Zero)
                DeleteDC(hdcMem);
            if (hdcWindow != IntPtr.Zero)
                ReleaseDC(hWnd, hdcWindow);
        }
    }

    /// <summary>
    /// 捕获窗口的指定区域（坐标相对于窗口左上角）
    /// </summary>
    public static Bitmap? CaptureWindowRegion(IntPtr hWnd, Rectangle region)
    {
        using var full = CaptureWindow(hWnd);
        if (full == null)
            return null;

        // 确保区域不超出截图范围
        var clip = new Rectangle(
            Math.Max(0, region.X),
            Math.Max(0, region.Y),
            Math.Min(region.Width, full.Width - region.X),
            Math.Min(region.Height, full.Height - region.Y));

        if (clip.Width <= 0 || clip.Height <= 0)
            return null;

        return full.Clone(clip, full.PixelFormat);
    }
}
