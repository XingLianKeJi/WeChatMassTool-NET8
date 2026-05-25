using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace WeChatMassTool.Utils;

public static class ProcessManager
{
    public static bool IsProcessRunning(int pid, string procName)
    {
        try
        {
            var process = Process.GetProcessById(pid);
            return process.ProcessName.Contains(procName, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public static Process? GetSpecificProcess(string procName)
    {
        try
        {
            var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(procName));
            return processes.Length > 0 ? processes[0] : null;
        }
        catch
        {
            return null;
        }
    }

    public static string? GetWechatPath(string procName)
    {
        try
        {
            var process = GetSpecificProcess(procName);
            return process?.MainModule?.FileName;
        }
        catch
        {
            return null;
        }
    }
}

public static class HashManager
{
    public static string GetFileSha256(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(stream);
            var sb = new StringBuilder();
            foreach (var b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }
}

public static class ConfigManager
{
    private static readonly string ConfigFilePath = FileIOManager.GetTempFilePath(
        FileIOManager.JoinPath("WeChatMassTool", "config.ini"));

    public static string GetConfig(string appName, string section, string option, string defaultValue = "")
    {
        try
        {
            var lines = FileIOManager.ReadFile(ConfigFilePath);
            var currentSection = "";
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    currentSection = trimmed.Substring(1, trimmed.Length - 2);
                }
                else if (currentSection == section && trimmed.StartsWith(option + "="))
                {
                    var parts = trimmed.Split('=', 2);
                    if (parts.Length == 2)
                        return parts[1].Trim();
                }
            }
        }
        catch
        {
        }
        return defaultValue;
    }

    public static void WriteConfig(string appName, string section, string option, string value)
    {
        try
        {
            var lines = new List<string>();
            var currentContent = FileIOManager.ReadFile(ConfigFilePath);
            var found = false;
            var currentSection = "";

            foreach (var line in currentContent)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    if (found && currentSection == section)
                    {
                        lines.Add($"{option}={value}");
                        found = false;
                    }
                    currentSection = trimmed.Substring(1, trimmed.Length - 2);
                    lines.Add(line);
                }
                else if (currentSection == section && trimmed.StartsWith(option + "="))
                {
                    lines.Add($"{option}={value}");
                    found = true;
                }
                else
                {
                    lines.Add(line);
                }
            }

            if (!found)
            {
                if (!currentContent.Any(l => l.Trim().StartsWith("[") && l.Trim().EndsWith("]") && l.Trim().Substring(1, l.Trim().Length - 2) == section))
                {
                    lines.Add($"[{section}]");
                }
                lines.Add($"{option}={value}");
            }

            FileIOManager.WriteFile(ConfigFilePath, lines);
        }
        catch
        {
        }
    }
}
