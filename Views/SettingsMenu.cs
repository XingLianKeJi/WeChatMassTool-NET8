using System.Drawing;
using System.Windows.Forms;
using WeChatMassTool.Config;
using WeChatMassTool.Constants;
using WeChatMassTool.Utils;

namespace WeChatMassTool.Views;

/// <summary>
/// 工具栏菜单处理
/// </summary>
public static class SettingsMenu
{
    public static void ShowSettingsMenu(Control owner, Point location)
    {
        var menu = new ContextMenuStrip
        {
            BackColor = ThemeColors.CardBackground,
            ForeColor = ThemeColors.TextPrimary,
            Font = new Font("Microsoft YaHei UI", 10F),
            RenderMode = ToolStripRenderMode.System,
            ShowImageMargin = false,
            Padding = new Padding(4),
        };

        var settingsItem = new ToolStripMenuItem("⚙ 设置")
        {
            ForeColor = ThemeColors.TextPrimary,
            BackColor = ThemeColors.CardBackground,
            Padding = new Padding(8, 6, 8, 6)
        };
        settingsItem.Click += (s, e) =>
        {
            MessageBox.Show("设置功能开发中，敬请期待", "设置",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        menu.Items.Add(settingsItem);

        menu.Items.Add(new ToolStripSeparator
        {
            BackColor = ThemeColors.BorderSubtle
        });

        var updateItem = new ToolStripMenuItem("🔄 检查更新")
        {
            ForeColor = ThemeColors.TextPrimary,
            BackColor = ThemeColors.CardBackground,
            Padding = new Padding(8, 6, 8, 6)
        };
        updateItem.Click += (s, e) =>
        {
            UpdateChecker.StartCheck();
        };
        menu.Items.Add(updateItem);

        menu.Items.Add(new ToolStripSeparator
        {
            BackColor = ThemeColors.BorderSubtle
        });

        var aboutItem = new ToolStripMenuItem("ℹ 关于")
        {
            ForeColor = ThemeColors.TextPrimary,
            BackColor = ThemeColors.CardBackground,
            Padding = new Padding(8, 6, 8, 6)
        };
        aboutItem.Click += (s, e) => ShowAboutDialog(owner);
        menu.Items.Add(aboutItem);

        foreach (ToolStripItem item in menu.Items)
        {
            if (item is ToolStripMenuItem mi)
            {
                mi.MouseEnter += (s, e) => mi.BackColor = ThemeColors.HoverOverlay;
                mi.MouseLeave += (s, e) => mi.BackColor = ThemeColors.CardBackground;
            }
        }

        menu.Show(owner, location);
    }

    public static void ShowAboutDialog(Control owner)
    {
        var aboutForm = new Form
        {
            Text = "关于",
            Size = new Size(380, 280),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = ThemeColors.ContentBackground
        };

        var lblName = new Label
        {
            Text = AppVersion.ProjectName,
            Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold),
            ForeColor = ThemeColors.PrimaryAccent,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 55
        };

        var lblDesc = new Label
        {
            Text = "微信群发消息桌面工具",
            Font = new Font("Microsoft YaHei UI", 10F),
            ForeColor = ThemeColors.TextSecondary,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 35
        };

        var lblVer = new Label
        {
            Text = $"版本: v{AppVersion.Version}",
            Font = new Font("Microsoft YaHei UI", 10F),
            ForeColor = ThemeColors.TextSecondary,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 35
        };

        var lblAuthor = new Label
        {
            Text = $"By {AppVersion.Author}",
            Font = new Font("Microsoft YaHei UI", 10F),
            ForeColor = ThemeColors.TextTertiary,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };

        var btnOk = new RoundButton
        {
            Text = "确定",
            BaseColor = ThemeColors.PrimaryAccent,
            HoverColor = Color.FromArgb(200, 160, 250),
            PressColor = Color.FromArgb(170, 130, 230),
            Size = new Size(100, 36),
            Font = new Font("Microsoft YaHei UI", 10F)
        };
        btnOk.Location = new Point((aboutForm.ClientSize.Width - btnOk.Width) / 2,
            aboutForm.ClientSize.Height - 55);
        btnOk.Click += (s, e) => aboutForm.Close();

        aboutForm.Controls.Add(btnOk);
        aboutForm.Controls.Add(lblAuthor);
        aboutForm.Controls.Add(lblVer);
        aboutForm.Controls.Add(lblDesc);
        aboutForm.Controls.Add(lblName);

        aboutForm.ShowDialog(owner.FindForm());
    }
}
