using System;
using System.Threading;
using System.Windows.Forms;
using WeChatMassTool.Config;
using WeChatMassTool.Constants;
using WeChatMassTool.Utils;
using WeChatMassTool.Views;

namespace WeChatMassTool;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        LogManager.Initialize(AppConfig.AppName);
        LogManager.Info("程序启动");
        LogManager.CleanOldLogs(LogConfig.RetainDays);

        DeleteOldCacheFiles();
        EnsureSingleInstance();
        CheckWechatRunning();

        var animateOnStartup = GetAnimateStatus();

        try
        {
            LogManager.Info($"准备创建 MainForm, animateOnStartup={animateOnStartup}");
            var mainForm = new MainForm(animateOnStartup);
            LogManager.Info("MainForm 创建成功，准备运行");
            Application.Run(mainForm);
        }
        catch (Exception ex)
        {
            LogManager.Error($"程序异常退出: {ex}");
            MessageBox.Show($"程序异常: {ex.Message}\n\n{ex.StackTrace}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        LogManager.Info("程序退出");
    }

    private static void DeleteOldCacheFiles()
    {
        var tempPath = FileIOManager.GetTempFilePath(AppConfig.AppName);
        FileIOManager.DeleteOldFilesWithExtension(tempPath, days: 3, fileExtension: ".tmp");

        var cachePath = FileIOManager.GetTempFilePath(
            FileIOManager.JoinPath(AppConfig.AppName, ".cache"));
        FileIOManager.DeleteOldFilesWithExtension(cachePath, days: 0, fileExtension: ".txt");
    }

    private static void EnsureSingleInstance()
    {
        var lockFile = FileIOManager.GetTempFilePath(
            FileIOManager.JoinPath(AppConfig.AppName, AppConfig.AppLockName));

        if (FileIOManager.PathExists(lockFile))
        {
            try
            {
                var content = FileIOManager.ReadFile(lockFile);
                if (content.Count > 0 && int.TryParse(content[0], out var pid))
                {
                    if (ProcessManager.IsProcessRunning(pid, AppConfig.AppProcessName))
                    {
                        MessageBox.Show("另一个实例已经在运行。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Environment.Exit(0);
                    }
                }
                FileIOManager.DeleteFile(lockFile);
            }
            catch
            {
                FileIOManager.DeleteFile(lockFile);
            }
        }

        FileIOManager.WriteFile(lockFile, new System.Collections.Generic.List<string> { FileIOManager.GetCurrentProcessId().ToString() });
    }

    private static void CheckWechatRunning()
    {
        var wechatProcess = ProcessManager.GetSpecificProcess(AppConfig.WeChatProcessName);
        if (wechatProcess == null)
        {
            MessageBox.Show("微信未启动! 将无法使用发送功能。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static bool GetAnimateStatus()
    {
        var value = ConfigManager.GetConfig(AppConfig.AppName, AnimateConfig.Section, AnimateConfig.Option, "True");
        return bool.TryParse(value, out var result) && result;
    }
}
