using System.Runtime.InteropServices;
using System.Text;

namespace WeChatMassTool.Utils;

public static class ClipboardManager
{
    [DllImport("user32.dll")]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll")]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll")]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll")]
    private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    [DllImport("user32.dll")]
    private static extern IntPtr GetClipboardData(uint uFormat);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll")]
    private static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("user32.dll")]
    private static extern bool IsClipboardFormatAvailable(uint format);

    private const uint CF_UNICODETEXT = 13;
    private const uint CF_HDROP = 15;

    public static void SetClipboardText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        try
        {
            OpenClipboard(IntPtr.Zero);
            EmptyClipboard();

            var bytes = Encoding.Unicode.GetBytes(text + "\0");
            var hMem = GlobalAlloc(0x0002, (UIntPtr)bytes.Length);
            var pMem = GlobalLock(hMem);
            Marshal.Copy(bytes, 0, pMem, bytes.Length);
            GlobalUnlock(hMem);
            SetClipboardData(CF_UNICODETEXT, hMem);
            CloseClipboard();
        }
        catch
        {
            try
            {
                CloseClipboard();
            }
            catch { }
        }
    }

    /// <summary>
    /// 将文件路径列表以 CF_HDROP 格式设置到剪贴板，使微信等应用能识别为文件粘贴
    /// </summary>
    public static void SetClipboardFiles(string[] filePaths)
    {
        if (filePaths == null || filePaths.Length == 0)
            return;

        try
        {
            // DROPFILES 结构体：20 字节，后跟 Unicode 文件路径列表（双 null 终止）
            var dropFilesSize = 20;
            var pathsData = new List<byte>();
            foreach (var path in filePaths)
            {
                var pathBytes = Encoding.Unicode.GetBytes(path + "\0");
                pathsData.AddRange(pathBytes);
            }
            // 末尾额外 null 终止符
            pathsData.AddRange(new byte[2]);

            var totalSize = dropFilesSize + pathsData.Count;
            var hMem = GlobalAlloc(0x0002, (UIntPtr)totalSize);
            var pMem = GlobalLock(hMem);

            // 写入 DROPFILES 结构体
            var dropFiles = new byte[dropFilesSize];
            // pFiles: 文件列表偏移量（结构体大小 20）
            BitConverter.GetBytes(20).CopyTo(dropFiles, 0);
            // pt.x, pt.y: 0
            // fNC: 0
            // fWide: 1 (Unicode)
            BitConverter.GetBytes(1).CopyTo(dropFiles, 16);

            Marshal.Copy(dropFiles, 0, pMem, dropFilesSize);
            Marshal.Copy(pathsData.ToArray(), 0, pMem + dropFilesSize, pathsData.Count);
            GlobalUnlock(hMem);

            OpenClipboard(IntPtr.Zero);
            EmptyClipboard();
            SetClipboardData(CF_HDROP, hMem);
            CloseClipboard();
        }
        catch
        {
            try { CloseClipboard(); } catch { }
        }
    }

    public static string GetClipboardText()
    {
        try
        {
            if (!IsClipboardFormatAvailable(CF_UNICODETEXT))
                return string.Empty;

            OpenClipboard(IntPtr.Zero);
            var hData = GetClipboardData(CF_UNICODETEXT);
            if (hData == IntPtr.Zero)
            {
                CloseClipboard();
                return string.Empty;
            }

            var pMem = GlobalLock(hData);
            var bytes = new byte[1024];
            Marshal.Copy(pMem, bytes, 0, bytes.Length);
            GlobalUnlock(hData);
            CloseClipboard();

            var nullIndex = Array.IndexOf(bytes, (byte)0);
            if (nullIndex > 0)
                bytes = bytes.Take(nullIndex).ToArray();

            return Encoding.Unicode.GetString(bytes);
        }
        catch
        {
            try { CloseClipboard(); } catch { }
            return string.Empty;
        }
    }
}

public static class WebLinkManager
{
    public static void OpenWebPage(string url)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
        }
    }
}
