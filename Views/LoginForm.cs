using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeChatMassTool.Config;
using Timer = System.Windows.Forms.Timer;

namespace WeChatMassTool.Views;

public partial class LoginForm : Form
{
    private Timer? _fadeInTimer;
    private Timer? _fadeOutTimer;
    private int _opacity = 0;

    public event Action? LoginSuccessful;

    public LoginForm()
    {
        InitializeComponent();
        InitTimers();
    }

    private bool _isDesignMode
    {
        get
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return true;
            if (System.Diagnostics.Process.GetCurrentProcess().ProcessName == "devenv")
                return true;
            return false;
        }
    }

    private void InitTimers()
    {
        if (_isDesignMode) return;

        _fadeInTimer = new Timer { Interval = 20 };
        _fadeInTimer.Tick += FadeInTimer_Tick;
        _fadeInTimer.Start();
    }

    private void FadeInTimer_Tick(object? sender, EventArgs e)
    {
        _opacity += 15;
        if (_opacity >= 100)
        {
            _opacity = 100;
            _fadeInTimer?.Stop();
            StartLoading();
        }
        this.Opacity = _opacity / 100.0;
    }

    private void StartLoading()
    {
        var delayTimer = new Timer { Interval = 500 };
        delayTimer.Tick += (s, e) =>
        {
            delayTimer.Stop();
            StartFadeOut();
        };
        delayTimer.Start();
    }

    private void StartFadeOut()
    {
        _fadeOutTimer = new Timer { Interval = 50 };
        _fadeOutTimer.Tick += FadeOutTimer_Tick;
        _fadeOutTimer.Start();
    }

    private void FadeOutTimer_Tick(object? sender, EventArgs e)
    {
        _opacity -= 15;
        if (_opacity <= 0)
        {
            _opacity = 0;
            _fadeOutTimer?.Stop();
            this.Close();
            LoginSuccessful?.Invoke();
        }
        this.Opacity = _opacity / 100.0;
    }

    #region Windows Form Designer generated code

    private Panel logoPanel;
    private Label logoLabel;
    private Label infoLabel;
    private Panel loadingPanel;
    private Label loadingLabel;
    private ProgressBar progressBar;

    private void InitializeComponent()
    {
        logoPanel = new Panel();
        logoLabel = new Label();
        infoLabel = new Label();
        loadingPanel = new Panel();
        loadingLabel = new Label();
        progressBar = new ProgressBar();
        logoPanel.SuspendLayout();
        loadingPanel.SuspendLayout();
        SuspendLayout();
        // 
        // logoPanel
        // 
        logoPanel.BackColor = Color.Transparent;
        logoPanel.Controls.Add(logoLabel);
        logoPanel.Dock = DockStyle.Top;
        logoPanel.Location = new Point(0, 148);
        logoPanel.Name = "logoPanel";
        logoPanel.Size = new Size(630, 150);
        logoPanel.TabIndex = 0;
        // 
        // logoLabel
        // 
        logoLabel.Dock = DockStyle.Fill;
        logoLabel.Font = new Font("Microsoft YaHei UI", 24F, FontStyle.Bold);
        logoLabel.ForeColor = Color.FromArgb(255, 121, 198); // 登录页特有颜色
        logoLabel.Location = new Point(0, 0);
        logoLabel.Name = "logoLabel";
        logoLabel.Size = new Size(630, 150);
        logoLabel.TabIndex = 0;
        logoLabel.Text = "微信群发消息工具";
        logoLabel.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // infoLabel
        // 
        infoLabel.Dock = DockStyle.Top;
        infoLabel.Font = new Font("Microsoft YaHei UI", 12F);
        infoLabel.ForeColor = ThemeColors.PrimaryAccent;
        infoLabel.Location = new Point(0, 0);
        infoLabel.Name = "infoLabel";
        infoLabel.Size = new Size(630, 148);
        infoLabel.TabIndex = 1;
        infoLabel.Text = "遥遥领先";
        infoLabel.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // loadingPanel
        // 
        loadingPanel.BackColor = Color.Transparent;
        loadingPanel.Controls.Add(loadingLabel);
        loadingPanel.Dock = DockStyle.Bottom;
        loadingPanel.Location = new Point(0, 370);
        loadingPanel.Name = "loadingPanel";
        loadingPanel.Size = new Size(630, 50);
        loadingPanel.TabIndex = 2;
        // 
        // loadingLabel
        // 
        loadingLabel.Dock = DockStyle.Fill;
        loadingLabel.Font = new Font("Microsoft YaHei UI", 10F);
        loadingLabel.ForeColor = ThemeColors.TextSecondary;
        loadingLabel.Location = new Point(0, 0);
        loadingLabel.Name = "loadingLabel";
        loadingLabel.Size = new Size(630, 50);
        loadingLabel.TabIndex = 0;
        loadingLabel.Text = "正在启动...";
        loadingLabel.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // progressBar
        // 
        progressBar.Dock = DockStyle.Bottom;
        progressBar.Location = new Point(0, 420);
        progressBar.MarqueeAnimationSpeed = 30;
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(630, 5);
        progressBar.Style = ProgressBarStyle.Marquee;
        progressBar.TabIndex = 3;
        // 
        // LoginForm
        // 
        BackColor = ThemeColors.TitleBarBackground;
        ClientSize = new Size(630, 425);
        Controls.Add(logoPanel);
        Controls.Add(infoLabel);
        Controls.Add(loadingPanel);
        Controls.Add(progressBar);
        FormBorderStyle = FormBorderStyle.None;
        Name = "LoginForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "WeChatMassTool - Loading";
        TopMost = true;
        TransparencyKey = ThemeColors.TitleBarBackground;
        logoPanel.ResumeLayout(false);
        loadingPanel.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion
}