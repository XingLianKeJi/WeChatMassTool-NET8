using System.Drawing;
using System.Windows.Forms;
using WeChatMassTool.Config;
using WeChatMassTool.Constants;
using WeChatMassTool.Utils;

namespace WeChatMassTool.Views;

/// <summary>
/// 更新提示对话框
/// </summary>
public static class UpdateDialog
{
    private static readonly Font DefaultFont = new("Microsoft YaHei UI", 10F);
    private static readonly Font TitleFont = new("Microsoft YaHei UI", 14F, FontStyle.Bold);
    private static readonly Font VersionFont = new("Microsoft YaHei UI", 10F);

    /// <summary>
    /// 显示"发现新版本"对话框
    /// </summary>
    public static void ShowUpdateAvailable(IWin32Window owner, UpdateInfo info, Action? onDownload = null)
    {
        var form = new Form
        {
            Text = "检查更新",
            Size = new Size(460, 340),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = ThemeColors.ContentBackground
        };

        // 标题
        var lblTitle = new Label
        {
            Text = "发现新版本",
            Font = TitleFont,
            ForeColor = ThemeColors.PrimaryAccent,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 50
        };

        // 版本对比
        var lblVersion = new Label
        {
            Text = $"当前版本: v{AppVersion.Version}  →  最新版本: v{info.LatestVersion}",
            Font = VersionFont,
            ForeColor = ThemeColors.TextSecondary,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 50
        };

        // 分隔线
        var topSep = new Panel
        {
            Dock = DockStyle.Top,
            Height = 1,
            BackColor = ThemeColors.BorderSubtle
        };

        // 按钮面板
        var btnPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            BackColor = ThemeColors.ContentBackground
        };

        var btnUpdate = new RoundButton
        {
            Text = "立即更新",
            BaseColor = ThemeColors.PrimaryAccent,
            HoverColor = Color.FromArgb(200, 160, 250),
            PressColor = Color.FromArgb(170, 130, 230),
            Size = new Size(150, 38),
            Font = DefaultFont
        };
        btnUpdate.Location = new Point(form.ClientSize.Width / 2 - btnUpdate.Width - 10, 12);
        btnUpdate.Click += (s, e) =>
        {
            form.Close();
            onDownload?.Invoke();
        };

        var btnClose = new RoundButton
        {
            Text = "关闭",
            BaseColor = ThemeColors.ButtonDefault,
            HoverColor = ThemeColors.ButtonHover,
            PressColor = ThemeColors.ButtonPress,
            Size = new Size(150, 38),
            Font = DefaultFont
        };
        btnClose.Location = new Point(form.ClientSize.Width / 2 + 10, 12);
        btnClose.Click += (s, e) => form.Close();

        btnPanel.Controls.Add(btnUpdate);
        btnPanel.Controls.Add(btnClose);

        // 底部分隔线
        var bottomSep = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 1,
            BackColor = ThemeColors.BorderSubtle
        };

        // 可点击的发布页面链接
        var lnkRelease = new LinkLabel
        {
            Text = "前往 GitHub Release 页面获取",
            Font = new Font("Microsoft YaHei UI", 9F),
            LinkColor = ThemeColors.PrimaryAccent,
            ActiveLinkColor = Color.FromArgb(220, 180, 255),
            VisitedLinkColor = ThemeColors.PrimaryAccent,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            LinkBehavior = LinkBehavior.HoverUnderline
        };
        lnkRelease.LinkClicked += (s, e) =>
        {
            if (!string.IsNullOrEmpty(info.ReleaseUrl))
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(info.ReleaseUrl) { UseShellExecute = true });
        };

        form.Controls.Add(lnkRelease);
        form.Controls.Add(bottomSep);
        form.Controls.Add(btnPanel);
        form.Controls.Add(topSep);
        form.Controls.Add(lblVersion);
        form.Controls.Add(lblTitle);

        form.ShowDialog(owner);
    }

    /// <summary>
    /// 显示"已是最新版本"提示
    /// </summary>
    public static void ShowNoUpdate(IWin32Window owner)
    {
        var form = new Form
        {
            Text = "检查更新",
            Size = new Size(420, 350),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = ThemeColors.ContentBackground
        };

        var lblIcon = new Label
        {
            Text = "✓",
            Font = new Font("Microsoft YaHei UI", 36F, FontStyle.Bold),
            ForeColor = ThemeColors.SuccessAccent,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 120
        };

        var lblMsg = new Label
        {
            Text = $"当前已是最新版本 (v{AppVersion.Version})",
            Font = new Font("Microsoft YaHei UI", 11F),
            ForeColor = ThemeColors.TextSecondary,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 40
        };

        var btnOk = new RoundButton
        {
            Text = "确定",
            BaseColor = ThemeColors.ButtonDefault,
            HoverColor = ThemeColors.ButtonHover,
            PressColor = ThemeColors.ButtonPress,
            Size = new Size(120, 38),
            Font = DefaultFont
        };
        btnOk.Location = new Point((form.ClientSize.Width - btnOk.Width) / 2, form.ClientSize.Height - 58);
        btnOk.Click += (s, e) => form.Close();

        form.Controls.Add(btnOk);
        form.Controls.Add(lblMsg);
        form.Controls.Add(lblIcon);

        form.ShowDialog(owner);
    }

    /// <summary>
    /// 显示错误提示
    /// </summary>
    public static void ShowError(IWin32Window owner, string message)
    {
        var form = new Form
        {
            Text = "检查更新",
            Size = new Size(380, 200),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = ThemeColors.ContentBackground
        };

        var lblIcon = new Label
        {
            Text = "✕",
            Font = new Font("Microsoft YaHei UI", 24F, FontStyle.Bold),
            ForeColor = ThemeColors.DangerAccent,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 55
        };

        var lblMsg = new Label
        {
            Text = message,
            Font = DefaultFont,
            ForeColor = ThemeColors.TextSecondary,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };

        var btnOk = new RoundButton
        {
            Text = "确定",
            BaseColor = ThemeColors.ButtonDefault,
            HoverColor = ThemeColors.ButtonHover,
            PressColor = ThemeColors.ButtonPress,
            Size = new Size(100, 36),
            Font = DefaultFont
        };
        btnOk.Location = new Point((form.ClientSize.Width - btnOk.Width) / 2, form.ClientSize.Height - 52);
        btnOk.Click += (s, e) => form.Close();

        form.Controls.Add(btnOk);
        form.Controls.Add(lblMsg);
        form.Controls.Add(lblIcon);

        form.ShowDialog(owner);
    }
}
