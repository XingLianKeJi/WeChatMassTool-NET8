using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WeChatMassTool.Utils;

namespace WeChatMassTool.Models;

public class SchedulerModel
{
    private class TaskEntry
    {
        public ScheduledTaskInfo Info = null!;
        public ScheduleState State = ScheduleState.Scheduled;
        public CancellationTokenSource? Cts;
    }

    private readonly Dictionary<string, TaskEntry> _tasks = new();
    private readonly object _lockObj = new();

    public event Action<string, ScheduleState>? TaskStateChanged;
    public event Action<string, TimeSpan>? TaskCountdownTick;
    public event Action<string, ScheduledTaskInfo>? TaskTriggered;
    public event Action<bool, string>? ScheduleInfoBar;
    public event Action? TaskListChanged;

    public string Schedule(ScheduledTaskInfo taskInfo)
    {
        var entry = new TaskEntry { Info = taskInfo, State = ScheduleState.Scheduled, Cts = new CancellationTokenSource() };

        lock (_lockObj)
        {
            _tasks[taskInfo.TaskId] = entry;
        }

        LogManager.Info($"设定定时发送 [{taskInfo.TaskId}]，时间：{taskInfo.ScheduledTime:yyyy-MM-dd HH:mm}");
        TaskListChanged?.Invoke();
        StartCountdown(taskInfo.TaskId);
        return taskInfo.TaskId;
    }

    public void Cancel(string taskId)
    {
        lock (_lockObj)
        {
            if (!_tasks.TryGetValue(taskId, out var entry)) return;
            entry.Cts?.Cancel();
            _tasks.Remove(taskId);
        }

        LogManager.Info($"取消定时发送 [{taskId}]");
        TaskListChanged?.Invoke();
        ScheduleInfoBar?.Invoke(true, "定时发送已取消");
    }

    public void CancelAll()
    {
        List<string> ids;
        lock (_lockObj)
        {
            ids = _tasks.Keys.ToList();
            foreach (var entry in _tasks.Values)
                entry.Cts?.Cancel();
            _tasks.Clear();
        }

        if (ids.Count > 0)
        {
            LogManager.Info("取消所有定时发送");
            TaskListChanged?.Invoke();
        }
    }

    public List<(string TaskId, ScheduledTaskInfo Info, ScheduleState State)> GetAllTasks()
    {
        lock (_lockObj)
        {
            return _tasks.Values
                .Select(e => (e.Info.TaskId, e.Info, e.State))
                .OrderBy(t => t.Info.ScheduledTime)
                .ToList();
        }
    }

    public ScheduledTaskInfo? GetTask(string taskId)
    {
        lock (_lockObj)
        {
            return _tasks.TryGetValue(taskId, out var entry) ? entry.Info : null;
        }
    }

    public void NotifySendCompleted(string taskId)
    {
        lock (_lockObj)
        {
            _tasks.Remove(taskId);
        }

        TaskListChanged?.Invoke();
    }

    public ScheduleState? GetTaskState(string taskId)
    {
        lock (_lockObj)
        {
            return _tasks.TryGetValue(taskId, out var entry) ? entry.State : null;
        }
    }

    private void StartCountdown(string taskId)
    {
        CancellationToken ct;
        DateTime scheduledTime;

        lock (_lockObj)
        {
            if (!_tasks.TryGetValue(taskId, out var entry) || entry.Cts == null) return;
            ct = entry.Cts.Token;
            scheduledTime = entry.Info.ScheduledTime;
        }

        Task.Run(async () =>
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var remaining = scheduledTime - DateTime.Now;

                    if (remaining <= TimeSpan.Zero)
                    {
                        lock (_lockObj)
                        {
                            if (_tasks.TryGetValue(taskId, out var entry))
                                entry.State = ScheduleState.Sending;
                        }

                        LogManager.Info($"定时发送已触发 [{taskId}]");

                        TaskStateChanged?.Invoke(taskId, ScheduleState.Sending);

                        ScheduledTaskInfo? taskInfo;
                        lock (_lockObj)
                        {
                            _tasks.TryGetValue(taskId, out var entry);
                            taskInfo = entry?.Info;
                        }

                        if (taskInfo != null)
                        {
                            TaskTriggered?.Invoke(taskId, taskInfo);
                            ScheduleInfoBar?.Invoke(true, $"定时发送已触发：{taskInfo.ScheduledTime:HH:mm}");
                        }
                        return;
                    }

                    TaskCountdownTick?.Invoke(taskId, remaining);
                    await Task.Delay(1000, ct);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }, ct);
    }
}
