# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

WeChatMassTool 是一个基于 .NET 8 WinForms 的微信群发消息桌面工具。通过 Windows Win32 API（user32.dll）操控微信窗口，模拟键盘输入实现自动化群发。同时支持从微信本地 SQLite 数据库（SQLCipher 加密）直接读取联系人和群聊数据。

## 构建与运行

```bash
dotnet build                          # 构建（Debug）
dotnet build -c Release               # Release 构建
dotnet run                            # 运行（需 Windows + 微信客户端）
dotnet publish -c Release -r win-x64  # 发布独立部署
```

目标框架：`net8.0-windows10.0.17763.0`。仅 Windows 平台，无测试项目。NuGet 依赖：`Microsoft.Data.Sqlite`、`SQLitePCLRaw.bundle_sqlcipher`、`System.Drawing.Common`。

## 架构

项目采用 MVC 模式，单项目结构，无分层分离：

```
Program.cs          → 入口：单实例锁、缓存清理、微信进程检测、启动动画
├── Views/           → WinForms 界面
│   ├── MainForm     → 主窗体（含 Designer.cs，所有控件手动布局）
│   ├── CustomControls → 自定义控件（RoundButton、GradientProgressBar、CardPanel、TabStrip）
│   ├── SettingsMenu → 工具栏菜单与关于对话框
│   └── LoginForm    → 启动淡入淡出动画窗体
├── Controllers/
│   └── ControllerMain → 桥接 UI 与 Model，管理事件转发、配置读写、定时任务持久化与恢复
├── Models/
│   ├── SendMessageModel      → 核心：异步发送逻辑、暂停/继续、进度缓存
│   ├── SchedulerModel        → 定时任务调度：倒计时、触发、CancellationToken 取消
│   ├── ScheduledTask         → 定时任务 DTO（ScheduleState 枚举、ScheduledTaskInfo）
│   ├── MessageInfo/ExecResult → 发送消息与执行结果 DTO
│   └── RecordGeneratorModel  → 执行结果记录与 CSV 导出
├── Utils/
│   ├── WxOperation       → 核心：Win32 API 封装，操控微信窗口（查找、模拟按键、发送消息/文件）
│   ├── WeChatDbManager   → 微信数据库读取：定位数据目录、SQLCipher 密钥搜索、联系人/群聊查询
│   ├── WeChatKeyDiag     → 密钥诊断工具（Ctrl+D 触发），用于排查数据库解密问题
│   ├── HumanSimulator    → 模拟人类键盘输入节奏（随机延迟）
│   ├── ClipboardManager  → Win32 剪贴板操作（文本 + CF_HDROP 文件格式）
│   ├── UiHelper          → GDI+ 绘制工具：圆角路径、渐变填充、暗色滚动条
│   └── ...其他工具类（WindowManager、ProcessManager、FileIOManager、LogManager、HashManager、ConfigManager）
├── Config/
│   ├── AppConfig         → 常量（应用名、微信进程名、窗口类名、间隔参数、日志配置）
│   └── ThemeColors       → 统一颜色常量与圆角半径
└── Constants/AppVersion  → 版本信息
```

## 关键设计

### 微信操控（WxOperation）
通过 P/Invoke 调用 user32.dll，使用 `FindWindow`（类名 `mmui::MainWindow`）/ `EnumChildWindows` 定位微信窗口和 Edit/RichEdit 控件，通过 `keybd_event` 模拟键盘。发送流程：`Ctrl+F` 搜索联系人 → `Ctrl+V` 粘贴 → `Enter`/`Ctrl+Enter` 发送。不使用微信 SDK。

### 微信数据库读取（WeChatDbManager）
通过注册表和文件系统定位微信数据目录（`xwechat_files`/`WeChat Files` → `wxid_*`）。数据库使用 SQLCipher 加密，密钥通过 `ReadProcessMemory` 从微信核心 DLL（`Weixin.dll`/`WeChatWin.dll`）的 `.data` 段中搜索，使用三种递进策略：sqlcipher 字符串附近 → 特征码附近 → .data 段全量扫描。每次打开数据库会复制到临时目录以避免锁定。

### 异步发送与暂停（SendMessageModel）
`SendWechatMessage` 使用 `Task.Run` 后台执行，通过 `ManualResetEvent` 实现暂停/继续。UI 线程通过 `BeginInvoke` 更新进度。

### 断点续发
基于名单文件的 SHA256 哈希值作为缓存文件名，记录已发送到的索引。程序重启后可从上次中断处继续。

### 定时任务（SchedulerModel）
支持设定未来时间发送。`SchedulerModel` 内部维护 `TaskEntry` 字典，每个任务有独立的倒计时 `Task.Run` 循环和 `CancellationTokenSource`。任务触发时通过事件通知 `ControllerMain`，控制器维护发送队列（`_sendQueue`），确保同一时间只有一个发送任务执行。任务数据持久化到临时目录的 `scheduled_tasks.dat` 文件，程序重启后自动恢复（5 分钟内的过期任务仍会执行）。

### 单实例
通过临时目录下的 `.lock` 文件 + PID 校验确保只有一个实例运行。

## UI 架构

### 暗色主题系统
所有颜色集中在 `Config/ThemeColors.cs` 静态类中，按语义角色组织。Surface 层级从深到浅：`WindowBackground` → `TitleBarBackground` → `ContentBackground` → `CardBackground` → `InputBackground`。强调色为紫色系（`PrimaryAccent = 189,147,249`）。所有颜色属性为 `static Color get-only`，圆角半径为 `const int`。

**修改 UI 颜色时必须使用 `ThemeColors.*`，禁止硬编码 `Color.FromArgb(...)`。**

### 自定义控件（Views/CustomControls.cs）
- **RoundButton** — 圆角按钮，三态颜色（Base/Hover/Press），`FlatStyle.Flat` + 自绘 `OnPaint`。通过 `SetBaseColor()` 动态换色。
- **GradientProgressBar** — 渐变进度条，圆角轨道 + `LinearGradientBrush` 填充。API 兼容 `Value`/`Maximum`。
- **CardPanel** — 圆角面板，`OnPaintBackground` 填充父背景色（裁剪圆角外区域），`OnPaint` 绘制填充+边框。子控件的 `BackColor` 需显式设为 `CardBackColor` 以避免透明问题。
- **TabStrip** — 自绘 Tab 控件，选中态用强调色边框。

所有自定义控件均启用 `DoubleBuffered = true`，使用 `UiHelper.CreateRoundedPath` 绘制圆角。

### 布局模式
- MainForm 无边框，通过 Win32 `ReleaseCapture` + `SendMessage(WM_NCLBUTTONDOWN)` 实现自定义拖动和边缘缩放（6px 边缘热区）。
- 使用 Dock 布局：titleBar/strip(Dock=Top) → statusBar(Dock=Bottom) → contentArea(Dock=Fill)。内部左右分栏：leftPanel(Dock=Fill) + rightPanel(Dock=Right)。
- Toast 通知（`ToastNotification`）使用 `CreateRoundRectRgn` 圆角 + 定时器渐隐。
- 菜单栏使用 `MenuStrip` + 自定义 `DarkMenuRenderer`（继承 `ToolStripProfessionalRenderer`）+ `DarkMenuColorTable` 实现暗色主题。
- `UiHelper.EnableDarkScrollBar` 调用 `SetWindowTheme("DarkMode_Explorer")` 为 Win10 1809+ 启用暗色滚动条。

### MainForm 初始化顺序
`InitializeComponent()` → `InitializeSettingsComboBoxes()` → `InitControls()`（绑定控件事件）→ 创建 `ControllerMain` → `SetupEventHandlers()`（绑定控制器事件到 UI）→ `LoadSettings()` → `ShowLoginAnimation()`。

### ControllerMain 事件模式
控制器桥接 View 与 Model：暴露 `ProgressUpdated`、`InfoBarShown`、`TaskListChanged`、`TaskStateChanged`、`TaskCountdownTick` 事件。所有 UI 回调通过 `BeginInvoke` 跨线程，并检查 `IsDisposed`/`IsHandleCreated` 防护。

## 注意事项

- 发送操作会真实操控微信窗口，运行时微信窗口会被前置激活。
- `Resources/Themes/` 下的 `.qss` 文件是历史遗留（Qt 样式表），当前项目已不使用。
- 微信数据库密钥搜索需要管理员权限运行 `WeChatMassTool.exe`。
- Ctrl+D 快捷键可触发 `WeChatKeyDiag` 诊断工具，用于排查数据库解密问题。
- `WeChatDbManager` 的 `IsValidKey` 方法要求 32 字节密钥至少有 8 种不同字节值。
- `ProfessionalColorTable` 没有 `DropDownBackground` 属性（.NET 8），暗色下拉背景用 `ToolStripDropDownBackground` 代替。
