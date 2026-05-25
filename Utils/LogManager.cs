using System;
using System.IO;
using System.Linq;

namespace WeChatMassTool.Utils;

public static class LogManager
{
    public enum LogLevel { Debug, Info, Warning, Error }

    private static string _logDir = string.Empty;
    private static string _appName = string.Empty;
    private static readonly object _lockObj = new();
    private static LogLevel _minLevel = LogLevel.Debug;

    public static void Initialize(string appName)
    {
        _appName = appName;
        _logDir = FileIOManager.GetTempFilePath(FileIOManager.JoinPath(appName, "logs"));
        Directory.CreateDirectory(_logDir);
    }

    public static void WriteLog(LogLevel level, string message)
    {
        if (level < _minLevel) return;
        if (string.IsNullOrEmpty(_logDir)) return;

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var levelStr = level.ToString().ToUpper();
        var line = $"[{timestamp}] [{levelStr}] {message}";

        var fileName = $"app_{DateTime.Now:yyyy-MM-dd}.log";
        var filePath = Path.Combine(_logDir, fileName);

        try
        {
            lock (_lockObj)
            {
                File.AppendAllText(filePath, line + Environment.NewLine);
            }
        }
        catch
        {
            // 日志写入失败时静默跳过
        }
    }

    public static void Debug(string message) => WriteLog(LogLevel.Debug, message);
    public static void Info(string message) => WriteLog(LogLevel.Info, message);
    public static void Warning(string message) => WriteLog(LogLevel.Warning, message);
    public static void Error(string message) => WriteLog(LogLevel.Error, message);

    public static void Error(string message, Exception ex)
    {
        WriteLog(LogLevel.Error, $"{message}\n{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
    }

    public static void CleanOldLogs(int retainDays = 7)
    {
        if (string.IsNullOrEmpty(_logDir) || !Directory.Exists(_logDir)) return;

        try
        {
            var cutoff = DateTime.Now.AddDays(-retainDays);
            var oldFiles = Directory.GetFiles(_logDir, "app_*.log")
                .Where(f => File.GetCreationTime(f) < cutoff);

            foreach (var file in oldFiles)
            {
                try { File.Delete(file); } catch { }
            }
        }
        catch
        {
            // 清理失败时静默跳过
        }
    }
}
