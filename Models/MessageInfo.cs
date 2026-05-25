using System.Collections.Generic;

namespace WeChatMassTool.Models;

public class MessageInfo
{
    public string SingleText { get; set; } = string.Empty;
    public string MultiText { get; set; } = string.Empty;
    public List<string> FilePaths { get; set; } = new();
    public string Names { get; set; } = string.Empty;
    public List<string> NameList { get; set; } = new();
    public string[] Texts { get; set; } = Array.Empty<string>();
    public int TextNameListCount { get; set; }
    public double TextInterval { get; set; } = 0.05;
    public double FileInterval { get; set; } = 0.5;
    public string SendShortcut { get; set; } = "{Enter}";
    public int CacheIndex { get; set; }
}

public class ExecResult
{
    public string NickName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
}
