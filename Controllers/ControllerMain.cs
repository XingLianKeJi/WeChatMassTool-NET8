using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WeChatMassTool.Config;
using WeChatMassTool.Models;
using WeChatMassTool.Utils;

namespace WeChatMassTool.Controllers;

public class ControllerMain
{
    private readonly SendMessageModel _model;
    private readonly SchedulerModel _scheduler = new();
    private readonly Form _view;
    private readonly Queue<(string TaskId, ScheduledTaskInfo Info)> _sendQueue = new();
    private List<string> _nameList = new();
    private string _nameListFile = string.Empty;
    private string _sha256CacheFile = string.Empty;
    private bool _animateOnStartup;
    private string? _currentSendingTaskId;

    public event Action<int, int>? ProgressUpdated;
    public event Action<bool, string>? InfoBarShown;
    public event Action? TaskListChanged;
    public event Action<string, ScheduleState>? TaskStateChanged;
    public event Action<string, TimeSpan>? TaskCountdownTick;

    public bool IsSending => _model.IsTaskActive("send_msg");

    public ControllerMain(Form view, bool animateOnStartup = true)
    {
        _view = view;
        _model = new SendMessageModel();
        _animateOnStartup = animateOnStartup;
        SetupConnections();
    }

    private void SetupConnections()
    {
        _model.ProgressUpdated += OnProgressUpdated;
        _model.InfoBarShown += OnInfoBarShown;
        _model.ExecInfoRecorded += OnExecInfoRecorded;
        _model.CacheProgress += OnCacheProgress;
        _model.DeleteCacheProgress += OnDeleteCacheProgress;
        _model.TaskStatusChanged += OnTaskStatusChanged;

        _scheduler.TaskTriggered += OnTaskTriggered;
        _scheduler.TaskCountdownTick += (id, remaining) =>
        {
            if (_view.IsDisposed || !_view.IsHandleCreated) return;
            _view.BeginInvoke(new Action(() => TaskCountdownTick?.Invoke(id, remaining)));
        };
        _scheduler.TaskStateChanged += (id, state) =>
        {
            if (_view.IsDisposed || !_view.IsHandleCreated) return;
            _view.BeginInvoke(new Action(() => TaskStateChanged?.Invoke(id, state)));
        };
        _scheduler.TaskListChanged += () =>
        {
            if (_view.IsDisposed || !_view.IsHandleCreated) return;
            _view.BeginInvoke(new Action(() => TaskListChanged?.Invoke()));
        };
        _scheduler.ScheduleInfoBar += (status, msg) =>
        {
            if (_view.IsDisposed || !_view.IsHandleCreated) return;
            _view.BeginInvoke(new Action(() => InfoBarShown?.Invoke(status, msg)));
        };
    }

    public void OnSendClicked(MessageInfo messageInfo)
    {
        var cacheIndex = GetNameListFileCacheIndex();
        messageInfo.CacheIndex = cacheIndex;
        _model.SendWechatMessage(messageInfo);
    }

    public void ToggleSendStatus()
    {
        _model.TogglePause();
    }

    public void ScheduleSend(ScheduledTaskInfo taskInfo)
    {
        _scheduler.Schedule(taskInfo);
        PersistAllTasks();
    }

    public void UpdateSchedule(string oldTaskId, ScheduledTaskInfo newTaskInfo)
    {
        _scheduler.Cancel(oldTaskId);
        _scheduler.Schedule(newTaskInfo);
        PersistAllTasks();
    }

    public void CancelSchedule(string taskId)
    {
        _scheduler.Cancel(taskId);
        PersistAllTasks();
    }

    public List<(string TaskId, ScheduledTaskInfo Info, ScheduleState State)> GetAllScheduleTasks()
    {
        return _scheduler.GetAllTasks();
    }

    public ScheduledTaskInfo? GetScheduleTask(string taskId)
    {
        return _scheduler.GetTask(taskId);
    }

    public void ImportNameList(string filePath)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            _nameList = FileIOManager.ReadFile(filePath);
            _nameListFile = filePath;
        }
    }

    public int GetNameListFileCacheIndex()
    {
        if (string.IsNullOrEmpty(_nameListFile))
            return 0;

        _sha256CacheFile = FileIOManager.GetTempFilePath(
            FileIOManager.JoinPath(AppConfig.AppName, HashManager.GetFileSha256(_nameListFile) + ".tmp"));

        if (FileIOManager.PathExists(_sha256CacheFile))
        {
            var content = FileIOManager.ReadFile(_sha256CacheFile);
            if (content.Count > 0 && int.TryParse(content[0], out var index))
                return index;
        }

        return 0;
    }

    private void OnProgressUpdated(int current, int total)
    {
        if (_view.IsDisposed || !_view.IsHandleCreated) return;
        _view.BeginInvoke(new Action(() =>
        {
            ProgressUpdated?.Invoke(current, total);
        }));
    }

    private void OnInfoBarShown(bool status, string tip)
    {
        if (_view.IsDisposed || !_view.IsHandleCreated) return;
        _view.BeginInvoke(new Action(() =>
        {
            InfoBarShown?.Invoke(status, tip);
        }));
    }

    private void OnExecInfoRecorded(ExecResult result)
    {
    }

    private void OnCacheProgress(string index)
    {
        if (!string.IsNullOrEmpty(_sha256CacheFile))
        {
            FileIOManager.WriteFile(_sha256CacheFile, new List<string> { index });
        }
    }

    private void OnDeleteCacheProgress(bool item)
    {
        if (!string.IsNullOrEmpty(_sha256CacheFile))
        {
            FileIOManager.DeleteFile(_sha256CacheFile);
        }
    }

    private void OnTaskTriggered(string taskId, ScheduledTaskInfo taskInfo)
    {
        if (IsSending)
        {
            LogManager.Info($"定时任务 [{taskId}] 触发，但当前正在发送，加入队列");
            _sendQueue.Enqueue((taskId, taskInfo));
            InfoBarShown?.Invoke(false, "当前正在发送，定时任务已排队等待");
            return;
        }

        ExecuteScheduleSend(taskId, taskInfo);
    }

    private void ExecuteScheduleSend(string taskId, ScheduledTaskInfo task)
    {
        _currentSendingTaskId = taskId;

        if (!string.IsNullOrEmpty(task.NameListFile) && FileIOManager.PathExists(task.NameListFile))
            ImportNameList(task.NameListFile);

        var messageInfo = new MessageInfo
        {
            SingleText = task.SingleText,
            MultiText = task.MultiText,
            FilePaths = task.FilePaths,
            Names = task.Names,
            NameList = new List<string>(_nameList),
            TextNameListCount = _nameList.Count,
            TextInterval = task.TextInterval,
            FileInterval = task.FileInterval,
            SendShortcut = task.SendShortcut,
            CacheIndex = GetNameListFileCacheIndex()
        };
        _model.SendWechatMessage(messageInfo);
    }

    private void OnTaskStatusChanged(string taskId)
    {
        if (taskId != "send_msg") return;

        if (_currentSendingTaskId != null)
        {
            var completedId = _currentSendingTaskId;
            _currentSendingTaskId = null;
            _scheduler.NotifySendCompleted(completedId);
            PersistAllTasks();
        }

        if (_sendQueue.Count > 0)
        {
            var next = _sendQueue.Dequeue();
            LogManager.Info($"从队列取出定时任务 [{next.TaskId}] 开始发送");
            ExecuteScheduleSend(next.TaskId, next.Info);
        }
    }

    public void ExportExecResult(string filePath)
    {
        var record = new RecordGeneratorModel();
        var (status, tip) = record.ExportExecResultToCsv(filePath);
        MessageBox.Show(tip, "提示", MessageBoxButtons.OK, status ? MessageBoxIcon.Information : MessageBoxIcon.Error);
    }

    public void SetAnimateStartupStatus(bool isChecked)
    {
        var value = isChecked.ToString();
        ConfigManager.WriteConfig(AppConfig.AppName, AnimateConfig.Section, AnimateConfig.Option, value);
    }

    public bool GetAnimateStartupStatus()
    {
        var value = ConfigManager.GetConfig(AppConfig.AppName, AnimateConfig.Section, AnimateConfig.Option, "True");
        return bool.TryParse(value, out var result) && result;
    }

    public List<string> ExportContactsFromDb()
    {
        var wxOp = new WxOperation();
        return wxOp.GetFriendList();
    }

    public Dictionary<string, string> ExportChatRoomListFromDb()
    {
        var dataDir = WeChatDbManager.FindWeChatDataDir();
        if (dataDir == null)
            throw new Exception("未找到微信数据目录，请确认微信已登录过");

        var dbPath = WeChatDbManager.GetContactDbPath(dataDir);
        if (dbPath == null)
            throw new Exception("未找到联系人数据库文件");

        return WeChatDbManager.GetChatRoomList(dbPath);
    }

    public List<string> ExportChatRoomMembersFromDb(string chatRoomWxid)
    {
        var dataDir = WeChatDbManager.FindWeChatDataDir();
        if (dataDir == null)
            throw new Exception("未找到微信数据目录，请确认微信已登录过");

        var dbPath = WeChatDbManager.GetContactDbPath(dataDir);
        if (dbPath == null)
            throw new Exception("未找到联系人数据库文件");

        return WeChatDbManager.GetChatRoomMembers(dbPath, chatRoomWxid);
    }

    public MessageInfo GetGuiInfo(
        string singleText,
        string multiText,
        List<string> filePaths,
        string names,
        double textInterval,
        double fileInterval,
        string sendShortcut)
    {
        var nameListCopy = new List<string>(_nameList);

        return new MessageInfo
        {
            SingleText = singleText,
            MultiText = multiText,
            FilePaths = filePaths,
            Names = names,
            NameList = nameListCopy,
            TextNameListCount = _nameList.Count,
            TextInterval = textInterval,
            FileInterval = fileInterval,
            SendShortcut = sendShortcut
        };
    }

    #region 多任务持久化

    private static string GetTaskDataFilePath()
    {
        return FileIOManager.GetTempFilePath(
            FileIOManager.JoinPath(AppConfig.AppName, ScheduleConfig.TaskDataFile));
    }

    private void PersistAllTasks()
    {
        var tasks = _scheduler.GetAllTasks()
            .Where(t => t.State == ScheduleState.Scheduled)
            .Select(t => t.Info)
            .ToList();

        if (tasks.Count == 0)
        {
            var path = GetTaskDataFilePath();
            if (FileIOManager.PathExists(path))
                FileIOManager.DeleteFile(path);
            return;
        }

        var lines = new List<string>();
        foreach (var task in tasks)
        {
            lines.Add(task.TaskId);
            lines.Add(task.ScheduledTime.ToString("O"));
            lines.Add(task.SingleText);
            lines.Add(task.MultiText);
            lines.Add(task.Names);
            lines.Add(task.TextInterval.ToString());
            lines.Add(task.FileInterval.ToString());
            lines.Add(task.SendShortcut);
            lines.Add(string.Join("|", task.FilePaths));
            lines.Add(task.NameListFile);
            lines.Add("");
        }

        FileIOManager.WriteFile(GetTaskDataFilePath(), lines);
        LogManager.Debug($"已持久化 {tasks.Count} 个定时任务");
    }

    public void TryRestoreAllTasks()
    {
        var filePath = GetTaskDataFilePath();
        if (!FileIOManager.PathExists(filePath)) return;

        var allLines = FileIOManager.ReadFile(filePath);
        if (allLines.Count == 0) return;

        var tasks = ParsePersistedTasks(allLines);
        var restored = 0;

        foreach (var taskInfo in tasks)
        {
            if (taskInfo.ScheduledTime < DateTime.Now.AddMinutes(-5))
            {
                LogManager.Info($"定时任务 [{taskInfo.TaskId}] 已过期（超过5分钟），放弃");
                continue;
            }

            if (taskInfo.ScheduledTime <= DateTime.Now)
            {
                LogManager.Info($"定时任务 [{taskInfo.TaskId}] 已过期但在5分钟内，加入发送队列");
                _sendQueue.Enqueue((taskInfo.TaskId, taskInfo));
                restored++;
            }
            else
            {
                _scheduler.Schedule(taskInfo);
                LogManager.Info($"恢复定时任务 [{taskInfo.TaskId}]，计划时间：{taskInfo.ScheduledTime:yyyy-MM-dd HH:mm}");
                restored++;
            }
        }

        if (restored > 0)
        {
            TaskListChanged?.Invoke();
            LogManager.Info($"共恢复 {restored} 个定时任务");

            if (_sendQueue.Count > 0 && !IsSending)
            {
                var next = _sendQueue.Dequeue();
                ExecuteScheduleSend(next.TaskId, next.Info);
            }
        }

        PersistAllTasks();
    }

    private static List<ScheduledTaskInfo> ParsePersistedTasks(List<string> lines)
    {
        var tasks = new List<ScheduledTaskInfo>();
        var i = 0;

        while (i + 9 < lines.Count)
        {
            if (string.IsNullOrEmpty(lines[i])) { i++; continue; }

            var startI = i;
            var task = ParseSingleTask(lines, ref i);
            if (task != null)
            {
                tasks.Add(task);
            }
            else
            {
                // 解析失败时跳过，防止无限循环
                i = startI + 1;
            }
        }

        return tasks;
    }

    private static ScheduledTaskInfo? ParseSingleTask(List<string> lines, ref int i)
    {
        if (i + 9 >= lines.Count) return null;

        var taskId = lines[i];
        if (string.IsNullOrEmpty(taskId)) return null;

        if (!DateTime.TryParse(lines[i + 1], out var scheduledTime)) return null;

        var task = new ScheduledTaskInfo
        {
            TaskId = taskId,
            ScheduledTime = scheduledTime,
            SingleText = lines[i + 2],
            MultiText = lines[i + 3],
            Names = lines[i + 4],
            TextInterval = double.TryParse(lines[i + 5], out var ti) ? ti : 0.05,
            FileInterval = double.TryParse(lines[i + 6], out var fi) ? fi : 0.5,
            SendShortcut = lines[i + 7],
            FilePaths = !string.IsNullOrEmpty(lines[i + 8])
                ? new List<string>(lines[i + 8].Split('|', StringSplitOptions.RemoveEmptyEntries))
                : new List<string>(),
            NameListFile = lines[i + 9]
        };

        i += 11;
        return task;
    }

    #endregion
}
