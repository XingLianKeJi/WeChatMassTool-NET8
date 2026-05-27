namespace WeChatMassTool.Config;

public static class AppConfig
{
    public const string AppName = "WeChatMassTool";
    public const string AppProcessName = "WeChatMassTool.exe";
    public const string AppLockName = "WeChatMassTool.lock";
    public const string WeChatProcessName = "Weixin.exe";
    public const string WindowName = "微信";
    public const string WindowClassName = "mmui::MainWindow";
    public const string TutorialLink = "https://www.bilivr.com/video/BV1rz421B7Uw/";
}

public static class ViewConfig
{
    public const int MenuWidth = 180;
    public const int LeftBoxWidth = 360;
    public const int RightBoxWidth = 240;
    public const int TimeAnimation = 500;
}

public static class IntervalConfig
{
    public const double BaseInterval = 0.1;
    public const double SendTextInterval = 0.05;
    public const double SendFileInterval = 0.25;
    public const double MaxSearchSecond = 0.1;
    public const double MaxSearchInterval = 0.05;
}

public static class AnimateConfig
{
    public const string Section = "DEFAULT";
    public const string Option = "animate_on_startup";
}

public static class ScheduleConfig
{
    public const string Section = "SCHEDULE";
    public const string OptionEnabled = "enabled";
    public const string TaskDataFile = "scheduled_tasks.dat";
}

public static class LogConfig
{
    public const string LogDir = "logs";
    public const string LogFilePrefix = "app_";
    public const int RetainDays = 7;
}

public static class HumanSimConfig
{
    // 键盘事件间隔（毫秒）
    public const int KeyPauseMin = 8;
    public const int KeyPauseMax = 25;

    // 组合键 Ctrl 与主键之间的间隔
    public const int CtrlKeyGapMin = 30;
    public const int CtrlKeyGapMax = 80;

    // 延迟档位（毫秒）
    public const int ShortDelayMin = 150;
    public const int ShortDelayMax = 500;
    public const int MediumDelayMin = 400;
    public const int MediumDelayMax = 1200;
    public const int LongDelayMin = 800;
    public const int LongDelayMax = 2000;
}
