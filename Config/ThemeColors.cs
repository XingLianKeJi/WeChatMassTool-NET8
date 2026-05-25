using System.Drawing;

namespace WeChatMassTool.Config;

/// <summary>
/// 统一颜色常量类，替代硬编码的 Color.FromArgb(...)
/// </summary>
public static class ThemeColors
{
    // --- Surface 层级（从深到浅）---
    public static Color WindowBackground { get; } = Color.FromArgb(27, 29, 35);
    public static Color TitleBarBackground { get; } = Color.FromArgb(33, 37, 43);
    public static Color ToolbarBg { get; } = Color.FromArgb(30, 34, 41);
    public static Color ContentBackground { get; } = Color.FromArgb(40, 44, 52);
    public static Color CardBackground { get; } = Color.FromArgb(35, 38, 45);
    public static Color InputBackground { get; } = Color.FromArgb(45, 48, 55);
    public static Color HoverOverlay { get; } = Color.FromArgb(50, 54, 65);

    // --- 强调色 ---
    public static Color PrimaryAccent { get; } = Color.FromArgb(189, 147, 249);
    public static Color SecondaryAccent { get; } = Color.FromArgb(52, 152, 219);
    public static Color SuccessAccent { get; } = Color.FromArgb(46, 204, 113);
    public static Color DangerAccent { get; } = Color.FromArgb(200, 60, 60);
    public static Color DangerLight { get; } = Color.FromArgb(239, 83, 80);
    public static Color ToastSuccess { get; } = Color.FromArgb(46, 213, 115);
    public static Color ToastError { get; } = Color.FromArgb(239, 83, 80);
    public static Color ToastInfo { get; } = Color.FromArgb(52, 152, 219);

    // --- 按钮颜色 ---
    public static Color ButtonDefault { get; } = Color.FromArgb(68, 81, 105);
    public static Color ButtonHover { get; } = Color.FromArgb(82, 96, 124);
    public static Color ButtonPress { get; } = Color.FromArgb(55, 65, 85);

    // --- 文字 ---
    public static Color TextPrimary { get; } = Color.White;
    public static Color TextSecondary { get; } = Color.FromArgb(170, 170, 170);
    public static Color TextTertiary { get; } = Color.FromArgb(150, 150, 150);
    public static Color TextMuted { get; } = Color.FromArgb(140, 140, 140);
    public static Color TextFaint { get; } = Color.FromArgb(120, 120, 120);
    public static Color TextDiagnostic { get; } = Color.FromArgb(200, 200, 200);

    // --- 边框/分割 ---
    public static Color BorderSubtle { get; } = Color.FromArgb(50, 55, 68);
    public static Color Divider { get; } = Color.FromArgb(25, 27, 33);

    // --- Tab 相关 ---
    public static Color TabActiveBg { get; } = Color.FromArgb(50, 55, 68);
    public static Color TabInactiveBg { get; } = Color.FromArgb(40, 44, 52);
    public static Color TabHoverBg { get; } = Color.FromArgb(45, 50, 60);

    // --- 诊断 ---
    public static Color DiagnosticBackground { get; } = Color.FromArgb(30, 32, 38);

    // --- 圆角半径 ---
    public const int CornerRadiusSmall = 6;
    public const int CornerRadiusMedium = 10;
    public const int CornerRadiusLarge = 14;
    public const int CornerRadiusButton = 8;
}
