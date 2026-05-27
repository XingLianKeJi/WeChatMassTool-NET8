using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WeChatMassTool.Utils;

namespace WeChatMassTool.Models;

public class SendMessageModel
{
    private static readonly Lazy<SendMessageModel> _instance = new(() => new SendMessageModel());
    public static SendMessageModel Instance => _instance.Value;

    public event Action<int, int>? ProgressUpdated;
    public event Action<ExecResult>? ExecInfoRecorded;
    public event Action<bool, string>? InfoBarShown;
    public event Action<string>? CacheProgress;
    public event Action<bool>? DeleteCacheProgress;
    public event Action<string>? TaskStatusChanged;

    private readonly RecordGeneratorModel _record;
    private readonly WxOperation _wxOperation;
    private bool _isPaused;
    private readonly ManualResetEvent _pauseEvent = new(true);
    private readonly object _lockObj = new();
    private readonly Dictionary<string, bool> _taskStatusMap = new();

    private SendMessageModel()
    {
        _record = RecordGeneratorModel.Instance;
        _wxOperation = WxOperation.Instance;
    }

    public void SendWechatMessage(MessageInfo messageInfo)
    {
        var taskId = "send_msg";

        lock (_lockObj)
        {
            if (_taskStatusMap.TryGetValue(taskId, out var status) && status)
                return;
            _taskStatusMap[taskId] = true;
        }

        Task.Run(() =>
        {
            try
            {
                var processedInfo = ProcessMessageInfo(messageInfo);
                var nameList = processedInfo.NameList;
                var cacheIndex = processedInfo.CacheIndex;
                var textNameListCount = processedInfo.TextNameListCount;
                var texts = processedInfo.Texts;
                var files = processedInfo.FilePaths;
                var sendShortcut = processedInfo.SendShortcut;
                var textInterval = processedInfo.TextInterval;
                var fileInterval = processedInfo.FileInterval;

                LogManager.Info($"开始发送，共 {nameList.Count} 位联系人");

                for (int idx = 0; idx < nameList.Count; idx++)
                {
                    if (cacheIndex > 0 && idx <= cacheIndex)
                        continue;

                    WaitIfPaused();

                    var execInfo = new ExecResult();
                    try
                    {
                        _wxOperation.SendMessage(
                            nameList[idx],
                            texts?.ToList(),
                            files?.ToList(),
                            textInterval,
                            fileInterval,
                            sendShortcut
                        );

                        execInfo.NickName = nameList[idx];
                        execInfo.Text = string.Join("\n", texts ?? Array.Empty<string>());
                        execInfo.File = string.Join("\n", files);
                        execInfo.Status = "成功";

                        InfoBarShown?.Invoke(true, $"{nameList[idx].Substring(0, Math.Min(8, nameList[idx].Length))} 发送成功");
                        LogManager.Info($"发送给 {nameList[idx]}：成功");
                    }
                    catch (Exception ex)
                    {
                        execInfo.Status = "失败";
                        execInfo.Remark = ex.Message;
                        InfoBarShown?.Invoke(false, $"{nameList[idx].Substring(0, Math.Min(8, nameList[idx].Length))} {ex.Message}");
                        LogManager.Warning($"发送给 {nameList[idx]} 失败：{ex.Message}");
                    }
                    finally
                    {
                        ExecInfoRecorded?.Invoke(execInfo);
                        _record.RecordExecResult(execInfo);

                        if (textNameListCount > idx + 1)
                            CacheProgress?.Invoke(idx.ToString());

                        if (textNameListCount == idx + 1)
                            DeleteCacheProgress?.Invoke(true);
                    }

                    ProgressUpdated?.Invoke(idx + 1, nameList.Count);
                }
            }
            catch (Exception ex)
            {
                InfoBarShown?.Invoke(false, ex.Message);
                LogManager.Error("发送过程异常", ex);
            }
            finally
            {
                _taskStatusMap[taskId] = false;
                TaskStatusChanged?.Invoke(taskId);
                LogManager.Info("发送完成");
            }
        });
    }

    private MessageInfo ProcessMessageInfo(MessageInfo messageInfo)
    {
        var processed = new MessageInfo
        {
            SingleText = messageInfo.SingleText,
            MultiText = messageInfo.MultiText,
            FilePaths = messageInfo.FilePaths,
            TextInterval = messageInfo.TextInterval,
            FileInterval = messageInfo.FileInterval,
            SendShortcut = messageInfo.SendShortcut,
            CacheIndex = messageInfo.CacheIndex
        };

        var nameList = new List<string>(messageInfo.NameList);

        if (!string.IsNullOrEmpty(messageInfo.Names))
        {
            var namesFromInput = messageInfo.Names.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            nameList.AddRange(namesFromInput);
        }

        nameList = nameList.Distinct().ToList();
        processed.NameList = nameList;
        processed.TextNameListCount = nameList.Count;

        var msgList = new List<string>();
        if (!string.IsNullOrEmpty(messageInfo.SingleText))
        {
            msgList.AddRange(messageInfo.SingleText.Split('\n', StringSplitOptions.RemoveEmptyEntries));
        }
        if (!string.IsNullOrEmpty(messageInfo.MultiText))
        {
            msgList.Add(messageInfo.MultiText);
        }
        processed.Texts = msgList.ToArray();

        return processed;
    }

    public bool IsTaskActive(string taskId)
    {
        lock (_lockObj)
        {
            return _taskStatusMap.TryGetValue(taskId, out var status) && status;
        }
    }

    public void TogglePause()
    {
        lock (_lockObj)
        {
            _isPaused = !_isPaused;
            if (_isPaused)
            {
                _pauseEvent.Reset();
                LogManager.Info("发送已暂停");
            }
            else
            {
                _pauseEvent.Set();
                LogManager.Info("发送已继续");
            }
        }
    }

    private void WaitIfPaused()
    {
        _pauseEvent.WaitOne();
    }

    public void ExportNameList(string tag, string filePath)
    {
        var taskId = "name_list";

        lock (_lockObj)
        {
            if (_taskStatusMap.TryGetValue(taskId, out var status) && status)
                return;
            _taskStatusMap[taskId] = true;
        }

        Task.Run(() =>
        {
            try
            {
                var result = _wxOperation.GetFriendList(tag);
                FileIOManager.WriteFile(filePath, result);
                InfoBarShown?.Invoke(true, "文件导出成功");
            }
            catch (Exception ex)
            {
                InfoBarShown?.Invoke(false, ex.Message);
            }
            finally
            {
                _taskStatusMap[taskId] = false;
                TaskStatusChanged?.Invoke(taskId);
            }
        });
    }

    public void ExportChatGroupNameList(string filePath)
    {
        var taskId = "chat_group_name_list";

        lock (_lockObj)
        {
            if (_taskStatusMap.TryGetValue(taskId, out var status) && status)
                return;
            _taskStatusMap[taskId] = true;
        }

        Task.Run(() =>
        {
            try
            {
                var result = _wxOperation.GetChatGroupNameList();
                FileIOManager.WriteFile(filePath, result);
                InfoBarShown?.Invoke(true, "文件导出成功");
            }
            catch (Exception ex)
            {
                InfoBarShown?.Invoke(false, ex.Message);
            }
            finally
            {
                _taskStatusMap[taskId] = false;
                TaskStatusChanged?.Invoke(taskId);
            }
        });
    }
}
