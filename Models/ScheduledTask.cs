using System;
using System.Collections.Generic;

namespace WeChatMassTool.Models;

public enum ScheduleState { Idle, Scheduled, Sending }

public class ScheduledTaskInfo
{
    public string TaskId { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public DateTime ScheduledTime { get; set; }
    public string SingleText { get; set; } = "";
    public string MultiText { get; set; } = "";
    public List<string> FilePaths { get; set; } = new();
    public string Names { get; set; } = "";
    public double TextInterval { get; set; } = 0.05;
    public double FileInterval { get; set; } = 0.5;
    public string SendShortcut { get; set; } = "{Enter}";
    public string NameListFile { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
