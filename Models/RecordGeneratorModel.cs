using System.Collections.Generic;
using System.IO;
using System.Text;
using WeChatMassTool.Config;
using WeChatMassTool.Utils;

namespace WeChatMassTool.Models;

public class RecordGeneratorModel
{
    private readonly string _cacheDir;
    private readonly string _tempFilePath;

    public RecordGeneratorModel()
    {
        _cacheDir = FileIOManager.GetTempFilePath(FileIOManager.JoinPath(AppConfig.AppName, ".cache"));
        _tempFilePath = FileIOManager.JoinPath(_cacheDir, "exec_results.txt");
        Directory.CreateDirectory(_cacheDir);
    }

    public void RecordExecResult(ExecResult result)
    {
        try
        {
            var line = $"{result.NickName}|{result.Text}|{result.File}|{result.Status}|{result.Remark}";
            File.AppendAllText(_tempFilePath, line + Environment.NewLine, Encoding.UTF8);
        }
        catch
        {
        }
    }

    public List<ExecResult> LoadExecResults()
    {
        var results = new List<ExecResult>();
        try
        {
            if (!File.Exists(_tempFilePath))
                return results;

            var lines = File.ReadAllLines(_tempFilePath, Encoding.UTF8);
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length >= 4)
                {
                    results.Add(new ExecResult
                    {
                        NickName = parts[0],
                        Text = parts[1],
                        File = parts[2],
                        Status = parts[3],
                        Remark = parts.Length > 4 ? parts[4] : string.Empty
                    });
                }
            }
        }
        catch
        {
        }
        return results;
    }

    public (bool Status, string Tip) ExportExecResultToCsv(string csvFilePath)
    {
        try
        {
            var results = LoadExecResults();
            if (results.Count == 0)
                return (false, "运行结果为空!");

            var sb = new StringBuilder();
            sb.AppendLine("昵称,文本,文件,状态,备注");

            foreach (var result in results)
            {
                sb.AppendLine($"\"{result.NickName}\",\"{result.Text}\",\"{result.File}\",\"{result.Status}\",\"{result.Remark}\"");
            }

            File.WriteAllText(csvFilePath, sb.ToString(), Encoding.UTF8);
            return (true, "导出成功!");
        }
        catch (System.Exception e)
        {
            return (false, e.Message);
        }
    }

    public void ClearCache()
    {
        try
        {
            if (File.Exists(_tempFilePath))
                File.Delete(_tempFilePath);
        }
        catch
        {
        }
    }
}
