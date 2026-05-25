using System.Diagnostics;
using Microsoft.Win32;

namespace WeChatMassTool.Utils;

public static class WindowManager
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    private const int WM_CLOSE = 0x0010;

    public static void MinimizeWechat(string className, string windowName)
    {
        var hWnd = FindWindow(className, windowName);
        if (hWnd != IntPtr.Zero)
        {
            SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }
    }

    public static bool WakeUpWindow(string processName = "Weixin.exe")
    {
        try
        {
            var processNameWithoutExt = Path.GetFileNameWithoutExtension(processName);
            var processes = Process.GetProcessesByName(processNameWithoutExt);
            
            if (processes.Length > 0)
            {
                foreach (var process in processes)
                {
                    if (process.MainWindowHandle != IntPtr.Zero)
                    {
                        BringWindowToFront(process.MainWindowHandle);
                        return true;
                    }
                }
            }

            var exePath = GetWechatPathFromProcess(processName);
            if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
            {
                Process.Start(exePath);
                return true;
            }

            exePath = GetWechatPathFromRegistry(processName);
            if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
            {
                Process.Start(exePath);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static string? GetWechatPathFromRegistry(string processName)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Weixin");
            if (key != null)
            {
                var installLocation = key.GetValue("InstallLocation") as string;
                if (!string.IsNullOrEmpty(installLocation))
                {
                    var exePath = Path.Combine(installLocation, processName);
                    if (File.Exists(exePath))
                        return exePath;
                }
            }
        }
        catch
        {
        }
        return null;
    }

    private static string? GetWechatPathFromProcess(string processName)
    {
        try
        {
            var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processName));
            if (processes.Length > 0)
            {
                return processes[0].MainModule?.FileName;
            }
        }
        catch
        {
        }
        return null;
    }

    public static void BringWindowToFront(IntPtr hWnd)
    {
        if (hWnd != IntPtr.Zero)
        {
            ShowWindow(hWnd, 9);
            SetForegroundWindow(hWnd);
        }
    }
}
