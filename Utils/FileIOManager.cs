using System.Text;

namespace WeChatMassTool.Utils;

public static class FileIOManager
{
    public static List<string> ReadFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return new List<string>();

            var encoding = DetectEncoding(filePath);
            var lines = File.ReadAllLines(filePath, encoding);
            return lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    public static void WriteFile(string filePath, List<string> data)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllLines(filePath, data, Encoding.UTF8);
        }
        catch
        {
        }
    }

    public static void WriteFile(string filePath, string[] data)
    {
        WriteFile(filePath, data.ToList());
    }

    private static Encoding DetectEncoding(string filePath)
    {
        try
        {
            using var reader = new StreamReader(filePath, Encoding.Default, true);
            reader.Read();
            return reader.CurrentEncoding ?? Encoding.UTF8;
        }
        catch
        {
            return Encoding.UTF8;
        }
    }

    public static string GetResourcePath(string relativePath)
    {
        try
        {
            var basePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            return Path.Combine(basePath, relativePath);
        }
        catch
        {
            return relativePath;
        }
    }

    public static string GetTempFilePath(string? fileName = null)
    {
        var tempPath = Path.GetTempPath();
        return string.IsNullOrEmpty(fileName) ? tempPath : Path.Combine(tempPath, fileName);
    }

    public static bool PathExists(string path)
    {
        return File.Exists(path) || Directory.Exists(path);
    }

    public static void DeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
        catch
        {
        }
    }

    public static void DeleteOldFilesWithExtension(string directory, int days = 3, string fileExtension = ".tmp")
    {
        if (!Directory.Exists(directory))
            return;

        var cutoff = DateTime.Now.AddDays(-days).Ticks;
        var files = Directory.GetFiles(directory, $"*{fileExtension}");

        foreach (var file in files)
        {
            try
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime.Ticks < cutoff)
                {
                    DeleteFile(file);
                }
            }
            catch
            {
            }
        }
    }

    public static string JoinPath(params string[] args)
    {
        return Path.Combine(args);
    }

    public static int GetCurrentProcessId()
    {
        return Environment.ProcessId;
    }
}
