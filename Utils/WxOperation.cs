using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WeChatMassTool.Config;

namespace WeChatMassTool.Utils;

public class WxOperation
{
    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string? lpszClass, string? lpszWindow);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, StringBuilder lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, string lParam);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int WM_GETTEXT = 0x000D;
    private const int WM_GETTEXTLENGTH = 0x000E;
    private const uint WM_KEYDOWN = 0x0100;
    private const uint WM_KEYUP = 0x0101;
    private const byte VK_CONTROL = 0x11;
    private const byte VK_SHIFT = 0x10;
    private const byte VK_RETURN = 0x0D;
    private const byte VK_ESCAPE = 0x1B;
    private const byte VK_UP = 0x26;
    private const byte VK_DOWN = 0x28;
    private const byte VK_DELETE = 0x2E;
    private const byte VK_F = 0x46;
    private const byte VK_V = 0x56;
    private const byte VK_A = 0x41;
    private const byte VK_S = 0x53;
    private const byte VK_END = 0x23;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    private IntPtr _wxWindowHandle = IntPtr.Zero;

    public bool LocateWechatWindow()
    {
        WindowManager.WakeUpWindow(AppConfig.WeChatProcessName);
        HumanSimulator.RandomDelay(600, 1200);

        _wxWindowHandle = FindWindow(AppConfig.WindowClassName, AppConfig.WindowName);
        if (_wxWindowHandle == IntPtr.Zero)
        {
            var processes = Process.GetProcessesByName("Weixin");
            foreach (var process in processes)
            {
                _wxWindowHandle = process.MainWindowHandle;
                if (_wxWindowHandle != IntPtr.Zero)
                    break;
            }
        }

        if (_wxWindowHandle != IntPtr.Zero)
        {
            WindowManager.BringWindowToFront(_wxWindowHandle);
            HumanSimulator.RandomDelay(200, 500);
        }

        return _wxWindowHandle != IntPtr.Zero;
    }

    public void SendKeys(string keys, double waitTime = 0)
    {
        foreach (char c in keys)
        {
            short vk = VkKeyScan(c);
            byte vkCode = (byte)(vk & 0xff);
            bool shift = (vk & 0x100) != 0;

            if (shift)
                keybd_event(VK_SHIFT, 0, 0, UIntPtr.Zero);

            keybd_event(vkCode, 0, 0, UIntPtr.Zero);
            HumanSimulator.RandomKeyPause();
            keybd_event(vkCode, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

            if (shift)
                keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

            if (waitTime > 0)
                HumanSimulator.RandomDelay((int)(waitTime * 800), (int)(waitTime * 1200));
        }
    }

    public void SendKey(byte vk, double waitTime = 0)
    {
        keybd_event(vk, 0, 0, UIntPtr.Zero);
        HumanSimulator.RandomKeyPause();
        keybd_event(vk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        if (waitTime > 0)
            HumanSimulator.RandomDelay((int)(waitTime * 800), (int)(waitTime * 1200));
    }

    public void SendCtrlKey(byte key, double waitTime = 0)
    {
        keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
        HumanSimulator.RandomDelay(HumanSimConfig.CtrlKeyGapMin, HumanSimConfig.CtrlKeyGapMax);
        keybd_event(key, 0, 0, UIntPtr.Zero);
        HumanSimulator.RandomKeyPause();
        keybd_event(key, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        HumanSimulator.RandomKeyPause();
        keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        if (waitTime > 0)
            HumanSimulator.RandomDelay((int)(waitTime * 800), (int)(waitTime * 1200));
    }

    private IntPtr FindChildEditControl(IntPtr parentHandle, string? windowName = null)
    {
        IntPtr editHandle = IntPtr.Zero;
        StringBuilder className = new StringBuilder(256);

        EnumChildWindows(parentHandle, (hWnd, lParam) =>
        {
            GetClassName(hWnd, className, className.Capacity);
            if (className.ToString().Contains("Edit", StringComparison.OrdinalIgnoreCase) ||
                className.ToString().Contains("RichEdit", StringComparison.OrdinalIgnoreCase))
            {
                if (windowName == null)
                {
                    editHandle = hWnd;
                    return false;
                }

                var sb = new StringBuilder(256);
                SendMessage(hWnd, WM_GETTEXTLENGTH, 0, IntPtr.Zero);
                SendMessage(hWnd, WM_GETTEXT, 256, sb);
                if (sb.ToString().Contains(windowName))
                {
                    editHandle = hWnd;
                    return false;
                }
            }
            return true;
        }, IntPtr.Zero);

        return editHandle;
    }

    public void GotoChatBox(string name)
    {
        if (_wxWindowHandle == IntPtr.Zero)
            throw new Exception("微信窗口未找到");

        EnsureWindowActive();

        SendCtrlKey(VK_F, 0.5);
        HumanSimulator.RandomDelay(HumanSimConfig.ShortDelayMin, HumanSimConfig.ShortDelayMax);

        ClipboardManager.SetClipboardText(name);
        HumanSimulator.RandomDelay(HumanSimConfig.ShortDelayMin, HumanSimConfig.ShortDelayMax);

        SendCtrlKey(VK_V, 0.5);
        HumanSimulator.RandomDelay(HumanSimConfig.MediumDelayMin, HumanSimConfig.MediumDelayMax);

        // OCR 识别搜索结果，智能选择正确目标
        int targetIndex = -1;
        try
        {
            targetIndex = OcrHelper.RecognizeSearchResult(_wxWindowHandle, name)
                .GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            LogManager.Warning($"OCR 识别失败，回退直接回车: {ex.Message}");
        }

        // 按下箭头选中正确结果
        if (targetIndex > 0)
        {
            for (int i = 0; i < targetIndex; i++)
            {
                SendKey(VK_DOWN, 0.2);
                HumanSimulator.RandomDelay(100, 250);
            }
        }

        SendKey(VK_RETURN, 0.8);
        HumanSimulator.RandomDelay(HumanSimConfig.MediumDelayMin, HumanSimConfig.MediumDelayMax);
    }

    private void EnsureWindowActive()
    {
        if (_wxWindowHandle != IntPtr.Zero)
        {
            ShowWindow(_wxWindowHandle, 9);
            SetForegroundWindow(_wxWindowHandle);
            HumanSimulator.RandomDelay(300, 700);
        }
    }

    public void SendTextMessage(string text, double waitTime, string sendShortcut = "{Enter}")
    {
        if (string.IsNullOrEmpty(text))
            return;

        EnsureWindowActive();

        ClipboardManager.SetClipboardText(text);
        HumanSimulator.RandomDelay(HumanSimConfig.ShortDelayMin, HumanSimConfig.ShortDelayMax);

        SendCtrlKey(VK_V, waitTime);
        HumanSimulator.RandomDelay((int)(waitTime * 800), (int)(waitTime * 1200));

        if (sendShortcut == "{Enter}")
            SendKey(VK_RETURN, 1.0);
        else if (sendShortcut == "{Ctrl}{Enter}")
            SendCtrlKey(VK_RETURN, 1.0);

        HumanSimulator.RandomDelay(HumanSimConfig.LongDelayMin, HumanSimConfig.LongDelayMax);
    }

    public void SendFiles(string[] filePaths, double waitTime, string sendShortcut = "{Enter}")
    {
        if (filePaths == null || filePaths.Length == 0)
            return;

        var existingFiles = filePaths.Where(File.Exists).ToArray();
        if (existingFiles.Length == 0)
            return;

        WindowManager.BringWindowToFront(_wxWindowHandle);
        HumanSimulator.RandomDelay(100, 300);

        ClipboardManager.SetClipboardFiles(existingFiles);
        HumanSimulator.RandomDelay(200, 400);

        EnsureWindowActive();
        SendCtrlKey(VK_V, waitTime);
        HumanSimulator.RandomDelay((int)(waitTime * 800), (int)(waitTime * 1200));

        if (sendShortcut == "{Enter}")
            SendKey(VK_RETURN, 1.0);
        else if (sendShortcut == "{Ctrl}{Enter}")
            SendCtrlKey(VK_RETURN, 1.0);

        HumanSimulator.RandomDelay(HumanSimConfig.LongDelayMin, HumanSimConfig.LongDelayMax);
    }

    public List<string> GetFriendList(string? tag = null)
    {
        var dataDir = WeChatDbManager.FindWeChatDataDir();
        if (dataDir == null)
            throw new Exception("未找到微信数据目录，请确认微信已登录过");

        var dbPath = WeChatDbManager.GetContactDbPath(dataDir);
        if (dbPath == null)
            throw new Exception("未找到联系人数据库文件");

        return WeChatDbManager.GetContacts(dbPath);
    }

    public List<string> GetChatGroupNameList()
    {
        var dataDir = WeChatDbManager.FindWeChatDataDir();
        if (dataDir == null)
            throw new Exception("未找到微信数据目录，请确认微信已登录过");

        var dbPath = WeChatDbManager.GetContactDbPath(dataDir);
        if (dbPath == null)
            throw new Exception("未找到联系人数据库文件");

        var rooms = WeChatDbManager.GetChatRoomList(dbPath);
        return rooms.Keys.ToList();
    }

    /// <summary>
    /// 获取指定群聊的成员昵称列表
    /// </summary>
    public List<string> GetChatRoomMembers(string chatRoomName)
    {
        var dataDir = WeChatDbManager.FindWeChatDataDir();
        if (dataDir == null)
            throw new Exception("未找到微信数据目录，请确认微信已登录过");

        var dbPath = WeChatDbManager.GetContactDbPath(dataDir);
        if (dbPath == null)
            throw new Exception("未找到联系人数据库文件");

        return WeChatDbManager.GetChatRoomMembers(dbPath, chatRoomName);
    }

    public void SendMessage(string name, List<string>? msgs = null, List<string>? filePaths = null,
        double textInterval = 0.05, double fileInterval = 0.5, string sendShortcut = "{Enter}")
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("用户名不能为空");

        if ((msgs == null || msgs.Count == 0) && (filePaths == null || filePaths.Count == 0))
            throw new ArgumentException("发送的消息和文件不可同时为空");

        if (!LocateWechatWindow())
            throw new Exception("微信未启动");

        GotoChatBox(name);

        if (msgs != null && msgs.Count > 0)
        {
            foreach (var msg in msgs)
            {
                SendTextMessage(msg, textInterval, sendShortcut);
            }
        }

        if (filePaths != null && filePaths.Count > 0)
        {
            SendFiles(filePaths.ToArray(), fileInterval, sendShortcut);
        }
    }

    [DllImport("user32.dll")]
    private static extern short VkKeyScan(char ch);
}
