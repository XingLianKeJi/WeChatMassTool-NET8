using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WeChatMassTool.Config;
using WeChatMassTool.Utils;

namespace WeChatMassTool.Views;

/// <summary>
/// 圆角按钮控件
/// </summary>
public class RoundButton : Button
{
    public int CornerRadius { get; set; } = ThemeColors.CornerRadiusButton;
    public Color BaseColor { get; set; } = ThemeColors.ButtonDefault;
    public Color HoverColor { get; set; } = ThemeColors.ButtonHover;
    public Color PressColor { get; set; } = ThemeColors.ButtonPress;

    private bool _isHovered;
    private bool _isPressed;

    public RoundButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseOverBackColor = Color.Transparent;
        FlatAppearance.MouseDownBackColor = Color.Transparent;
        BackColor = BaseColor;
        ForeColor = ThemeColors.TextPrimary;
        DoubleBuffered = true;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _isHovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _isHovered = false;
        _isPressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        _isPressed = true;
        Invalidate();
        base.OnMouseDown(mevent);
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        _isPressed = false;
        Invalidate();
        base.OnMouseUp(mevent);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        // 用 Region 物理裁剪掉圆角外的区域，彻底杜绝直角底色
        if (Width > 0 && Height > 0)
        {
            var newRegion = new Region(
                UiHelper.CreateRoundedPath(new Rectangle(0, 0, Width, Height), CornerRadius));
            var oldRegion = Region;
            Region = newRegion;
            oldRegion?.Dispose();
        }
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        var fillColor = _isPressed ? PressColor : _isHovered ? HoverColor : BaseColor;

        // 填充按钮背景
        using var path = UiHelper.CreateRoundedPath(rect, CornerRadius);
        using var brush = new SolidBrush(fillColor);
        e.Graphics.FillPath(brush, path);

        // 悬停/按下时画高亮边框
        if (_isHovered || _isPressed)
        {
            var borderColor = _isPressed
                ? Color.FromArgb(160, 140, 200)
                : Color.FromArgb(120, 110, 170);
            using var pen = new Pen(borderColor, 1.5f);
            e.Graphics.DrawPath(pen, path);
        }

        // 文本
        int pad = Math.Max(4, CornerRadius / 2);
        var textRect = new Rectangle(pad, 0, Width - 1 - pad * 2, Height - 1);
        var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
        TextRenderer.DrawText(e.Graphics, Text, Font, textRect, ForeColor, flags);
    }

    /// <summary>
    /// 动态设置基础颜色并刷新
    /// </summary>
    public void SetBaseColor(Color color)
    {
        BaseColor = color;
        BackColor = color;
        Invalidate();
    }
}

/// <summary>
/// 渐变进度条控件
/// </summary>
public class GradientProgressBar : Control
{
    private int _value;
    private int _maximum = 100;

    public int Value
    {
        get => _value;
        set { _value = Math.Clamp(value, 0, _maximum); Invalidate(); }
    }

    public int Maximum
    {
        get => _maximum;
        set { _maximum = Math.Max(1, value); Invalidate(); }
    }

    public Color GradientStart { get; set; } = ThemeColors.PrimaryAccent;
    public Color GradientEnd { get; set; } = Color.FromArgb(255, 121, 198);
    public int CornerRadius { get; set; } = ThemeColors.CornerRadiusSmall;
    public Color TrackColor { get; set; } = ThemeColors.InputBackground;

    public GradientProgressBar()
    {
        DoubleBuffered = true;
        Height = 20;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        // 轨道背景
        using (var trackPath = UiHelper.CreateRoundedPath(rect, CornerRadius))
        using (var trackBrush = new SolidBrush(TrackColor))
        {
            e.Graphics.FillPath(trackBrush, trackPath);
        }

        // 进度填充
        if (_maximum > 0 && _value > 0)
        {
            int fillWidth = (int)((double)_value / _maximum * Width);
            var fillRect = new Rectangle(0, 0, Math.Max(CornerRadius * 2, fillWidth), Height - 1);

            using var fillPath = UiHelper.CreateRoundedPath(fillRect, CornerRadius);
            using var fillBrush = new LinearGradientBrush(fillRect, GradientStart, GradientEnd, 0f);
            e.Graphics.FillPath(fillBrush, fillPath);
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }
}

/// <summary>
/// 卡片面板控件（圆角 + 边框）
/// </summary>
public class CardPanel : Panel
{
    public int CornerRadius { get; set; } = ThemeColors.CornerRadiusMedium;
    public bool ShowBorder { get; set; } = true;

    private Color _cardBackColor = ThemeColors.CardBackground;
    public Color CardBackColor
    {
        get => _cardBackColor;
        set { _cardBackColor = value; BackColor = value; Invalidate(); }
    }

    public CardPanel()
    {
        DoubleBuffered = true;
        Padding = new Padding(1);
        BackColor = _cardBackColor;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        // 先用父背景色填充角落区域（圆角外的区域）
        using (var parentBrush = new SolidBrush(Parent?.BackColor ?? BackColor))
            e.Graphics.FillRectangle(parentBrush, ClientRectangle);
        // 再用卡片色填充圆角区域
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using (var path = UiHelper.CreateRoundedPath(rect, CornerRadius))
        using (var fillBrush = new SolidBrush(_cardBackColor))
            e.Graphics.FillPath(fillBrush, path);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (!ShowBorder) return;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = UiHelper.CreateRoundedPath(rect, CornerRadius);
        using var pen = new Pen(ThemeColors.BorderSubtle, 1);
        e.Graphics.DrawPath(pen, path);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }
}

/// <summary>
/// Tab 切换控件
/// </summary>
public class TabStrip : Control
{
    private readonly List<string> _tabs = new();
    private int _selectedIndex;
    private int _hoveredIndex = -1;
    private readonly Font _tabFont = new("Microsoft YaHei UI", 10F);
    private readonly StringFormat _format = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

    public event EventHandler<int>? SelectedIndexChanged;

    public IReadOnlyList<string> Tabs => _tabs.AsReadOnly();

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (value >= 0 && value < _tabs.Count && _selectedIndex != value)
            {
                _selectedIndex = value;
                Invalidate();
                SelectedIndexChanged?.Invoke(this, _selectedIndex);
            }
        }
    }

    public TabStrip()
    {
        DoubleBuffered = true;
        Height = 42;
        BackColor = ThemeColors.TitleBarBackground;
    }

    public void AddTab(string text)
    {
        _tabs.Add(text);
        Invalidate();
    }

    public void ClearTabs()
    {
        _tabs.Clear();
        _selectedIndex = 0;
        _hoveredIndex = -1;
        Invalidate();
    }

    private int GetTabWidth()
    {
        if (_tabs.Count == 0) return 0;
        return 120;
    }

    private int GetTabIndexAt(int x)
    {
        int tabWidth = GetTabWidth();
        int index = x / tabWidth;
        return index >= 0 && index < _tabs.Count ? index : -1;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        int tabWidth = GetTabWidth();
        int tabHeight = Height - 6;
        int gap = 4;

        for (int i = 0; i < _tabs.Count; i++)
        {
            int x = 8 + i * (tabWidth + gap);
            var tabRect = new Rectangle(x, 3, tabWidth, tabHeight);

            // 背景
            Color bgColor = i == _selectedIndex ? ThemeColors.TabActiveBg
                          : i == _hoveredIndex ? ThemeColors.TabHoverBg
                          : ThemeColors.ContentBackground;

            using var path = UiHelper.CreateRoundedPath(tabRect, 6);
            using var bgBrush = new SolidBrush(bgColor);
            e.Graphics.FillPath(bgBrush, path);

            // 选中态边框
            if (i == _selectedIndex)
            {
                using var pen = new Pen(ThemeColors.PrimaryAccent, 1.5f);
                e.Graphics.DrawPath(pen, path);
            }

            // 文字
            using var textBrush = new SolidBrush(
                i == _selectedIndex ? ThemeColors.TextPrimary : ThemeColors.TextSecondary);
            e.Graphics.DrawString(_tabs[i], _tabFont, textBrush, tabRect, _format);
        }

        // 底部分隔线
        using var linePen = new Pen(ThemeColors.BorderSubtle, 1);
        e.Graphics.DrawLine(linePen, 0, Height - 1, Width, Height - 1);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        var newHovered = GetTabIndexAt(e.X);
        if (newHovered != _hoveredIndex)
        {
            _hoveredIndex = newHovered;
            Invalidate();
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hoveredIndex = -1;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        var index = GetTabIndexAt(e.X);
        if (index >= 0)
            SelectedIndex = index;
        base.OnMouseClick(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tabFont.Dispose();
            _format.Dispose();
        }
        base.Dispose(disposing);
    }
}
