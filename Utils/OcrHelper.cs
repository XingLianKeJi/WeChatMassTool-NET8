using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace WeChatMassTool.Utils;

/// <summary>
/// Windows 内置 OCR 封装，用于识别微信搜索结果文字并匹配目标名称
/// </summary>
public static class OcrHelper
{
    private static OcrEngine? _engine;
    private static readonly object _lock = new();

    private static OcrEngine GetEngine()
    {
        if (_engine != null)
            return _engine;

        lock (_lock)
        {
            if (_engine != null)
                return _engine;

            var engine = OcrEngine.TryCreateFromUserProfileLanguages();
            if (engine == null)
                engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("zh-Hans-CN"));

            _engine = engine ?? throw new InvalidOperationException("无法初始化 OCR 引擎，请确认系统已安装中文语言包");
            return _engine;
        }
    }

    /// <summary>
    /// 将 Bitmap 像素数据直接写入 SoftwareBitmap（避免 encoder→decoder 往返）
    /// </summary>
    private static async Task<SoftwareBitmap> BitmapToSoftwareBitmapAsync(Bitmap bmp)
    {
        var bitmapData = bmp.LockBits(
            new Rectangle(0, 0, bmp.Width, bmp.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);

        try
        {
            var dataSize = bitmapData.Stride * bitmapData.Height;
            var bytes = new byte[dataSize];
            Marshal.Copy(bitmapData.Scan0, bytes, 0, dataSize);

            using var stream = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream);
            encoder.SetPixelData(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                (uint)bmp.Width, (uint)bmp.Height,
                96.0, 96.0, bytes);
            await encoder.FlushAsync();

            stream.Seek(0);
            var decoder = await BitmapDecoder.CreateAsync(stream);
            return await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
        }
        finally
        {
            bmp.UnlockBits(bitmapData);
        }
    }

    /// <summary>
    /// 裁剪图片到左侧面板区域（微信搜索结果在左侧），减少 OCR 处理面积
    /// </summary>
    private static Bitmap CropToSearchArea(Bitmap screenshot)
    {
        // 搜索结果在微信窗口左侧面板，约占宽度 40%
        var cropWidth = (int)(screenshot.Width * 0.4);
        if (cropWidth >= screenshot.Width)
            return screenshot;

        var cropRect = new Rectangle(0, 0, cropWidth, screenshot.Height);
        return screenshot.Clone(cropRect, screenshot.PixelFormat);
    }

    public static async Task<OcrResult?> RecognizeAsync(Bitmap image)
    {
        var engine = GetEngine();
        using var softwareBitmap = await BitmapToSoftwareBitmapAsync(image);
        return await engine.RecognizeAsync(softwareBitmap);
    }

    /// <summary>
    /// 在 OCR 结果中搜索目标名称，返回匹配行索引（0-based）
    /// </summary>
    public static int FindMatchedLineIndex(OcrResult ocrResult, string targetName)
    {
        if (string.IsNullOrEmpty(targetName))
            return -1;

        var candidates = ExtractSearchResultLines(ocrResult);
        if (candidates.Count == 0)
            return -1;

        // 精确匹配
        for (var i = 0; i < candidates.Count; i++)
        {
            var cleanText = candidates[i].Replace(" ", "").Trim();
            if (cleanText.Contains(targetName, StringComparison.Ordinal) ||
                targetName.Contains(cleanText, StringComparison.Ordinal))
            {
                return i;
            }
        }

        // 模糊匹配（容错 OCR 识别错误）
        var bestIndex = -1;
        var bestSimilarity = 0.7;

        for (var i = 0; i < candidates.Count; i++)
        {
            var cleanText = candidates[i].Replace(" ", "").Trim();
            var similarity = CalculateSimilarity(targetName, cleanText);
            if (similarity > bestSimilarity)
            {
                bestSimilarity = similarity;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private static List<string> ExtractSearchResultLines(OcrResult ocrResult)
    {
        var lines = new List<string>();

        foreach (var line in ocrResult.Lines)
        {
            var text = line.Text.Trim();
            if (string.IsNullOrEmpty(text))
                continue;
            if (IsHeaderOrLabel(text))
                continue;

            lines.Add(text);
        }

        return lines;
    }

    private static bool IsHeaderOrLabel(string text)
    {
        var skipTexts = new[]
        {
            "搜索", "联系人", "群聊", "公众号", "聊天记录",
            "搜一搜", "搜索网络结果", "朋友圈", "文章",
            "小程序", "视频号", "新闻", "百科"
        };

        foreach (var skip in skipTexts)
        {
            if (text.Equals(skip, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 截取微信窗口搜索结果区域并识别目标名称的索引
    /// </summary>
    public static async Task<int> RecognizeSearchResult(IntPtr wxWindowHandle, string targetName)
    {
        using var screenshot = WindowCapture.CaptureWindow(wxWindowHandle);
        if (screenshot == null)
            throw new Exception("微信窗口截图失败");

        // 裁剪到左侧面板，减少约 60% 的 OCR 面积
        using var cropped = CropToSearchArea(screenshot);

        var ocrResult = await RecognizeAsync(cropped);
        if (ocrResult == null || ocrResult.Lines.Count == 0)
            throw new Exception("OCR 未识别到任何文字");

        LogManager.Debug($"OCR 识别到 {ocrResult.Lines.Count} 行文字");
        foreach (var line in ocrResult.Lines)
            LogManager.Debug($"  OCR 行: [{line.Text}]");

        var index = FindMatchedLineIndex(ocrResult, targetName);

        if (index >= 0)
            LogManager.Info($"OCR 匹配: 目标 '{targetName}' 在搜索结果第 {index + 1} 行");
        else
            LogManager.Warning($"OCR 未找到匹配项: '{targetName}'，回退到第一项");

        return index;
    }

    /// <summary>
    /// 截取微信窗口搜索结果区域并识别目标名称的索引和边界矩形
    /// </summary>
    public static async Task<(int Index, Rectangle BoundingRect)?> RecognizeSearchResultWithBounds(
        IntPtr wxWindowHandle, string targetName)
    {
        using var screenshot = WindowCapture.CaptureWindow(wxWindowHandle);
        if (screenshot == null)
            throw new Exception("微信窗口截图失败");

        using var cropped = CropToSearchArea(screenshot);

        var ocrResult = await RecognizeAsync(cropped);
        if (ocrResult == null || ocrResult.Lines.Count == 0)
            throw new Exception("OCR 未识别到任何文字");

        LogManager.Debug($"OCR 识别到 {ocrResult.Lines.Count} 行文字");
        foreach (var line in ocrResult.Lines)
            LogManager.Debug($"  OCR 行: [{line.Text}]");

        var candidates = ExtractSearchResultLines(ocrResult);
        if (candidates.Count == 0)
            return null;

        var index = FindMatchedLineIndex(ocrResult, targetName);
        if (index < 0)
            return null;

        // 聚合匹配行所有单词的边界矩形
        var matchedLine = GetMatchedOcrLine(ocrResult, candidates, index);
        if (matchedLine == null)
            return (index, Rectangle.Empty);

        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxRight = 0;
        var maxBottom = 0;

        foreach (var word in matchedLine.Words)
        {
            var r = word.BoundingRect;
            minX = Math.Min(minX, (int)r.X);
            minY = Math.Min(minY, (int)r.Y);
            maxRight = Math.Max(maxRight, (int)(r.X + r.Width));
            maxBottom = Math.Max(maxBottom, (int)(r.Y + r.Height));
        }

        var boundingRect = new Rectangle(minX, minY, maxRight - minX, maxBottom - minY);

        LogManager.Info($"OCR 匹配: 目标 '{targetName}' 在搜索结果第 {index + 1} 行, 区域: {boundingRect}");

        return (index, boundingRect);
    }

    /// <summary>
    /// 从 OCR 结果中获取跳过标签后的匹配行对应的原始 OcrLine
    /// </summary>
    private static Windows.Media.Ocr.OcrLine? GetMatchedOcrLine(
        OcrResult ocrResult, List<string> candidates, int candidateIndex)
    {
        var lineIdx = 0;
        foreach (var line in ocrResult.Lines)
        {
            var text = line.Text.Trim();
            if (string.IsNullOrEmpty(text)) continue;
            if (IsHeaderOrLabel(text)) continue;

            if (lineIdx == candidateIndex)
                return line;
            lineIdx++;
        }
        return null;
    }

    private static double CalculateSimilarity(string s1, string s2)
    {
        if (s1 == s2) return 1.0;
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0.0;

        var distance = LevenshteinDistance(s1, s2);
        var maxLength = Math.Max(s1.Length, s2.Length);
        return 1.0 - (double)distance / maxLength;
    }

    private static int LevenshteinDistance(string s1, string s2)
    {
        var len1 = s1.Length;
        var len2 = s2.Length;
        var dp = new int[len1 + 1, len2 + 1];

        for (var i = 0; i <= len1; i++) dp[i, 0] = i;
        for (var j = 0; j <= len2; j++) dp[0, j] = j;

        for (var i = 1; i <= len1; i++)
        {
            for (var j = 1; j <= len2; j++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                dp[i, j] = Math.Min(
                    Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                    dp[i - 1, j - 1] + cost);
            }
        }

        return dp[len1, len2];
    }
}
