using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WeChatMassTool.Utils;

/// <summary>
/// 微信密钥诊断工具，用于排查密钥获取问题
/// </summary>
public static class WeChatKeyDiag
{
    [DllImport("kernel32.dll")]
    private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int dwSize, out int lpNumberOfBytesRead);

    public static string RunDiag()
    {
        var sb = new StringBuilder();

        // 1. 检查微信进程
        var processes = Process.GetProcessesByName("Weixin");
        sb.AppendLine($"微信进程数: {processes.Length}");

        if (processes.Length == 0)
        {
            sb.AppendLine("错误: 微信未运行！");
            return sb.ToString();
        }

        var process = processes[0];
        sb.AppendLine($"PID: {process.Id}");

        // 2. 检查模块
        try
        {
            process.Refresh();
            var moduleNames = new List<string>();
            IntPtr wechatWinBase = IntPtr.Zero;
            int wechatWinSize = 0;

            foreach (ProcessModule module in process.Modules)
            {
                var name = module.ModuleName ?? "";
                if (name.Contains("Weixin", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("WeChatWin", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("wechatwin", StringComparison.OrdinalIgnoreCase))
                {
                    wechatWinBase = module.BaseAddress;
                    wechatWinSize = (int)module.ModuleMemorySize;
                    sb.AppendLine($"找到 WeChatWin.dll: Base=0x{(long)wechatWinBase:X}, Size={wechatWinSize / 1024 / 1024}MB");
                }

                if (name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    moduleNames.Add(name);
            }

            if (wechatWinBase == IntPtr.Zero)
            {
                sb.AppendLine("错误: 未找到 WeChatWin.dll 模块！");
                sb.AppendLine("已加载的 DLL:");
                foreach (var n in moduleNames.Take(30))
                    sb.AppendLine($"  {n}");
            }
            else
            {
                // 3. 尝试读取模块内存
                sb.AppendLine("\n正在读取 WeChatWin.dll 内存...");
                var buffer = new byte[wechatWinSize];
                if (ReadProcessMemory(process.Handle, wechatWinBase, buffer, buffer.Length, out int bytesRead))
                {
                    sb.AppendLine($"成功读取 {bytesRead / 1024 / 1024}MB");

                    // 搜索关键字符串
                    var searchStrings = new[] { "MicroMsg.db", "contact.db", "SelectWeChatKey", "PRAGMA key", "sqlcipher" };
                    foreach (var s in searchStrings)
                    {
                        var bytes = Encoding.ASCII.GetBytes(s);
                        var offset = FindPattern(buffer, bytes);
                        if (offset >= 0)
                            sb.AppendLine($"  找到 \"{s}\" 在偏移 0x{offset:X}");
                        else
                            sb.AppendLine($"  未找到 \"{s}\"");
                    }

                    // 4. 扫描可能的密钥
                    sb.AppendLine("\n扫描可能的 32 字节密钥...");
                    int candidateCount = 0;
                    for (int i = 0; i < buffer.Length - 8 && candidateCount < 10; i += 8)
                    {
                        var ptrValue = BitConverter.ToInt64(buffer, i);
                        if (ptrValue < 0x10000 || ptrValue > 0x7FFFFFFFFFFF) continue;

                        var keyBuffer = new byte[32];
                        if (ReadProcessMemory(process.Handle, (IntPtr)ptrValue, keyBuffer, 32, out int keyBytesRead) && keyBytesRead == 32)
                        {
                            if (IsValidKey(keyBuffer))
                            {
                                sb.AppendLine($"  候选密钥 #{candidateCount + 1} (来自偏移 0x{i:X}, 指向 0x{ptrValue:X}):");
                                sb.AppendLine($"    {BitConverter.ToString(keyBuffer).Replace("-", "")}");
                                candidateCount++;
                            }
                        }
                    }

                    if (candidateCount == 0)
                        sb.AppendLine("  未找到有效密钥候选");
                }
                else
                {
                    sb.AppendLine("错误: 无法读取进程内存（可能权限不足，请以管理员身份运行）");
                }
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine($"模块枚举失败: {ex.Message}");
            sb.AppendLine("提示: 请以管理员身份运行本程序");
        }

        // 5. 检查数据目录
        sb.AppendLine("\n--- 数据目录检查 ---");
        var dataDir = WeChatDbManager.FindWeChatDataDir();
        sb.AppendLine($"数据目录: {dataDir ?? "(未找到)"}");

        if (dataDir != null)
        {
            var dbPath = WeChatDbManager.GetContactDbPath(dataDir);
            sb.AppendLine($"联系人数据库: {dbPath ?? "(未找到)"}");
            if (dbPath != null)
            {
                sb.AppendLine($"文件存在: {File.Exists(dbPath)}");
                sb.AppendLine($"数据库加密: {WeChatDbManager.IsDbEncrypted(dbPath)}");
            }
        }

        return sb.ToString();
    }

    private static int FindPattern(byte[] buffer, byte[] pattern)
    {
        for (int i = 0; i <= buffer.Length - pattern.Length; i++)
        {
            bool found = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (buffer[i + j] != pattern[j]) { found = false; break; }
            }
            if (found) return i;
        }
        return -1;
    }

    private static bool IsValidKey(byte[] key)
    {
        if (key.Length != 32) return false;
        var seen = new HashSet<byte>();
        foreach (var b in key)
            seen.Add(b);
        // 有效密钥至少有 8 种不同字节值，且不全为零
        return seen.Count >= 8 && !key.All(b => b == 0);
    }
}
