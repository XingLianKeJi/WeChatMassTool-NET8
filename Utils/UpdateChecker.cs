using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;
using AutoUpdaterDotNET;
using WeChatMassTool.Config;
using WeChatMassTool.Constants;

namespace WeChatMassTool.Utils;

/// <summary>
/// 更新信息
/// </summary>
public class UpdateInfo
{
    public bool HasUpdate { get; set; }
    public string LatestVersion { get; set; } = "";
    public string ReleaseNotes { get; set; } = "";
    public string ReleaseUrl { get; set; } = "";
    public string DownloadUrl { get; set; } = "";
}

/// <summary>
/// 基于 AutoUpdater.NET 的更新检查器，直接解析 GitHub Releases API
/// </summary>
public static class UpdateChecker
{
    private const string ApiUrl = "https://api.github.com/repos/XingLianKeJi/WeChatMassTool-NET8/releases/latest";
    private const string FallbackUrl = "https://github.com/XingLianKeJi/WeChatMassTool-NET8/releases/latest";

    private static UpdateInfoEventArgs? _latestArgs;
    private static Form? _loadingForm;

    [DllImport("Gdi32.dll")]
    private static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int w, int h);

    static UpdateChecker()
    {
        AutoUpdater.InstalledVersion = new Version(AppVersion.Version);
        AutoUpdater.HttpUserAgent = "WeChatMassTool";
        AutoUpdater.ReportErrors = false;

        AutoUpdater.ParseUpdateInfoEvent += OnParseUpdateInfo;
    }

    /// <summary>
    /// 显示带遮罩的 loading 提示
    /// </summary>
    private static void ShowLoading()
    {
        var mainForm = Application.OpenForms.Cast<Form>().FirstOrDefault();
        if (mainForm == null) return;

        // 遮罩层：半透明覆盖主窗口
        _loadingForm = new Form
        {
            FormBorderStyle = FormBorderStyle.None,
            StartPosition = FormStartPosition.Manual,
            Size = mainForm.Size,
            Location = mainForm.Location,
            BackColor = Color.FromArgb(30, 30, 30),
            Opacity = 0.6,
            ShowInTaskbar = false,
            ControlBox = false
        };
        _loadingForm.Shown += (s, e) => _loadingForm?.Activate();

        // loading 卡片
        var card = new Panel
        {
            Size = new Size(280, 100),
            BackColor = ThemeColors.CardBackground,
            Location = new Point((_loadingForm.ClientSize.Width - 280) / 2, (_loadingForm.ClientSize.Height - 100) / 2)
        };
        card.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, 280, 100, ThemeColors.CornerRadiusMedium, ThemeColors.CornerRadiusMedium));

        var lblMsg = new Label
        {
            Text = "正在检查更新，请稍候...",
            Font = new Font("Microsoft YaHei UI", 11F),
            ForeColor = ThemeColors.TextPrimary,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };

        card.Controls.Add(lblMsg);
        _loadingForm.Controls.Add(card);

        _loadingForm.Show(mainForm);
        Application.DoEvents();
    }

    /// <summary>
    /// 关闭 loading 遮罩
    /// </summary>
    public static void CloseLoading()
    {
        if (_loadingForm != null && !_loadingForm.IsDisposed)
        {
            _loadingForm.Close();
            _loadingForm = null;
        }
    }

    /// <summary>
    /// 解析 GitHub Releases API 的 JSON 响应
    /// </summary>
    private static void OnParseUpdateInfo(ParseUpdateInfoEventArgs args)
    {
        try
        {
            using var doc = JsonDocument.Parse(args.RemoteData);
            var root = doc.RootElement;

            var tagName = root.TryGetProperty("tag_name", out var tagProp) ? tagProp.GetString() ?? "" : "";
            var version = tagName.TrimStart('v', 'V');
            var htmlUrl = root.TryGetProperty("html_url", out var urlProp) ? urlProp.GetString() ?? FallbackUrl : FallbackUrl;
            var body = root.TryGetProperty("body", out var bodyProp) ? bodyProp.GetString() ?? "" : "";

            // 从 assets 中找 zip 下载链接
            var downloadUrl = htmlUrl;
            if (root.TryGetProperty("assets", out var assets))
            {
                foreach (var asset in assets.EnumerateArray())
                {
                    var name = asset.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "" : "";
                    if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        downloadUrl = asset.GetProperty("browser_download_url").GetString() ?? htmlUrl;
                        break;
                    }
                }
            }

            args.UpdateInfo = new UpdateInfoEventArgs
            {
                CurrentVersion = version,
                DownloadURL = downloadUrl,
                ChangelogURL = htmlUrl,
                Mandatory = new Mandatory { Value = false },
            };

            // 缓存更新信息供自定义 UI 使用
            _latestArgs = args.UpdateInfo;
        }
        catch (Exception ex)
        {
            LogManager.Error("解析更新信息失败", ex);
        }
    }

    /// <summary>
    /// 获取最近一次解析的 UpdateInfoEventArgs
    /// </summary>
    public static UpdateInfoEventArgs? GetLatestArgs() => _latestArgs;

    /// <summary>
    /// 启动更新检查（显示 loading 提示）
    /// </summary>
    public static void StartCheck()
    {
        ShowLoading();
        AutoUpdater.Start(ApiUrl);
    }
}
