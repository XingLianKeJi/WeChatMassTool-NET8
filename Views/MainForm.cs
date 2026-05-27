using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoUpdaterDotNET;
using WeChatMassTool.Config;
using WeChatMassTool.Controllers;
using WeChatMassTool.Models;
using WeChatMassTool.Utils;

namespace WeChatMassTool.Views;

public partial class MainForm : Form
{
    private ControllerMain? _controller;
    private readonly List<string> _nameList = new();
    private string _nameListFile = "";
    private bool _isPaused;
    private string? _editingTaskId;
    private bool _rightPanelCollapsed;

    // 窗体拖动和边缘调整大小
    private const int WM_NCLBUTTONDOWN = 0xA1;
    private const int HTCAPTION = 2;
    private const int HTLEFT = 10;
    private const int HTRIGHT = 11;
    private const int HTTOP = 12;
    private const int HTTOPLEFT = 13;
    private const int HTTOPRIGHT = 14;
    private const int HTBOTTOM = 15;
    private const int HTBOTTOMLEFT = 16;
    private const int HTBOTTOMRIGHT = 17;
    private const int RESIZE_MARGIN = 6;

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    private int _resizeEdge = 0;

    public MainForm()
    {
        InitializeComponent();
        if (!_isDesignMode)
        {
            InitializeSettingsComboBoxes();
        }
        InitControls();
    }

    public MainForm(bool animateOnStartup)
    {
        try
        {
            InitializeComponent();
            LogManager.Info("InitializeComponent 完成");
        }
        catch (Exception ex)
        {
            LogManager.Error($"InitializeComponent 失败: {ex}");
            throw;
        }

        if (!_isDesignMode)
        {
            try
            {
                InitializeSettingsComboBoxes();
                InitControls();
                LogManager.Info("InitControls 完成");

                LogManager.Info("开始创建 ControllerMain");
                _controller = new ControllerMain(this, animateOnStartup);
                LogManager.Info("ControllerMain 创建完成");

                LogManager.Info("开始 SetupEventHandlers");
                SetupEventHandlers();
                LogManager.Info("SetupEventHandlers 完成");

                LoadSettings();
                LogManager.Info("Controller 初始化完成");

                dtpSchedule.Value = DateTime.Now.AddMinutes(5);
                dtpSchedule.MinDate = DateTime.Now.AddMinutes(1);

                LogManager.Info($"准备 ShowLoginAnimation, animateOnStartup={animateOnStartup}");
                if (animateOnStartup)
                {
                    ShowLoginAnimation();
                }

                LogManager.Info("构造函数全部完成");
            }
            catch (Exception ex)
            {
                LogManager.Error($"构造函数后续初始化失败: {ex}");
                throw;
            }
        }
    }

    private void InitializeSettingsComboBoxes()
    {
        if (_isDesignMode) return;

        for (double i = 0.05; i <= 0.5; i += 0.05)
            cbTextInterval.Items.Add(i.ToString("F2"));
        cbTextInterval.SelectedIndex = 0;

        for (double i = 0.25; i <= 2.5; i += 0.25)
            cbFileInterval.Items.Add(i.ToString("F2"));
        cbFileInterval.SelectedIndex = 0;
    }

    private void LoadSettings()
    {
        if (_isDesignMode || _controller == null) return;
        try
        {
        }
        catch
        {
        }
    }

    private void InitControls()
    {
        if (_isDesignMode) return;

        // 窗口按钮
        btnClose.Click += BtnClose_Click;
        btnMaximize.Click += BtnMaximize_Click;
        btnMinimize.Click += BtnMinimize_Click;

        // 功能按钮
        btnClearAll.Click += BtnClearAll_Click;
        btnAddFile.Click += BtnAddFile_Click;
        btnClearFiles.Click += BtnClearFiles_Click;
        btnImportNames.Click += BtnImportNames_Click;
        btnExportContacts.Click += BtnExportContacts_Click;
        btnExportChatRooms.Click += BtnExportChatRooms_Click;
        btnExport.Click += BtnExport_Click;
        btnPause.Click += BtnPause_Click;
        btnSend.Click += BtnSend_Click;
        btnSchedule.Click += BtnSchedule_Click;
        btnShowSchedule.Click += BtnTogglePanel_Click;

        // 菜单栏事件
        menuStrip.Renderer = new DarkMenuRenderer();
        SetupAutoUpdater();
        menuSettings.Click += (s, e) =>
        {
            MessageBox.Show("设置功能开发中，敬请期待", "设置",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        menuAbout.Click += (s, e) => SettingsMenu.ShowAboutDialog(this);
        menuUpdate.Click += (s, e) => UpdateChecker.StartCheck();

        // 暗色滚动条
        UiHelper.EnableDarkScrollBar(fileListBox);
        UiHelper.EnableDarkScrollBar(taskFlowPanel);

        // Ctrl+D 诊断快捷键
        this.KeyPreview = true;
        this.KeyDown += (s, e) =>
        {
            if (e.Control && e.KeyCode == Keys.D)
                RunDiag();
        };

        // titleBar 拖动窗体
        titleBar.MouseDown += TitleBar_MouseDown;
        lblTitle.MouseDown += TitleBar_MouseDown;

        // 边缘调整大小
        this.MouseDown += Form_MouseDown;
        this.MouseMove += Form_MouseMove;
        this.MouseUp += Form_MouseUp;

        AddResizeHandlerToControls(this.Controls);

        // 输入框滚动条：内容超出时才显示
        txtNames.TextChanged += (s, e) => UpdateScrollBar(txtNames);
        txtSingleMsg.TextChanged += (s, e) => UpdateScrollBar(txtSingleMsg);
        txtMultiMsg.TextChanged += (s, e) => UpdateScrollBar(txtMultiMsg);
        txtNames.Resize += (s, e) => UpdateScrollBar(txtNames);
        txtSingleMsg.Resize += (s, e) => UpdateScrollBar(txtSingleMsg);
        txtMultiMsg.Resize += (s, e) => UpdateScrollBar(txtMultiMsg);
    }

    /// <summary>
    /// 检测文本内容是否超出控件可见区域，动态显示/隐藏垂直滚动条
    /// </summary>
    private void UpdateScrollBar(TextBox textBox)
    {
        if (string.IsNullOrEmpty(textBox.Text))
        {
            textBox.ScrollBars = ScrollBars.None;
            return;
        }

        // 计算文本所需的总高度
        var textSize = TextRenderer.MeasureText(
            textBox.Text + "\n",
            textBox.Font,
            new Size(textBox.ClientSize.Width, int.MaxValue),
            TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl
        );

        bool needsScroll = textSize.Height > textBox.ClientSize.Height;
        var desired = needsScroll ? ScrollBars.Vertical : ScrollBars.None;

        if (textBox.ScrollBars != desired)
            textBox.ScrollBars = desired;
    }

    private void AddResizeHandlerToControls(Control.ControlCollection controls)
    {
        foreach (Control control in controls)
        {
            control.MouseDown += Form_MouseDown;
            control.MouseMove += Form_MouseMove;
            control.MouseUp += Form_MouseUp;
            if (control.Controls.Count > 0)
            {
                AddResizeHandlerToControls(control.Controls);
            }
        }
    }

    private void Form_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && _resizeEdge != 0)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, (IntPtr)_resizeEdge, IntPtr.Zero);
        }
    }

    private void Form_MouseMove(object? sender, MouseEventArgs e)
    {
        if (this.WindowState == FormWindowState.Maximized)
        {
            _resizeEdge = 0;
            this.Cursor = Cursors.Default;
            return;
        }

        Point pos = this.PointToClient(Cursor.Position);
        int x = pos.X;
        int y = pos.Y;
        int w = this.ClientSize.Width;
        int h = this.ClientSize.Height;

        bool onLeft = x <= RESIZE_MARGIN;
        bool onRight = x >= w - RESIZE_MARGIN;
        bool onTop = y <= RESIZE_MARGIN;
        bool onBottom = y >= h - RESIZE_MARGIN;

        if (onTop && onLeft)
        {
            _resizeEdge = HTTOPLEFT;
            this.Cursor = Cursors.SizeNWSE;
        }
        else if (onTop && onRight)
        {
            _resizeEdge = HTTOPRIGHT;
            this.Cursor = Cursors.SizeNESW;
        }
        else if (onBottom && onLeft)
        {
            _resizeEdge = HTBOTTOMLEFT;
            this.Cursor = Cursors.SizeNESW;
        }
        else if (onBottom && onRight)
        {
            _resizeEdge = HTBOTTOMRIGHT;
            this.Cursor = Cursors.SizeNWSE;
        }
        else if (onLeft)
        {
            _resizeEdge = HTLEFT;
            this.Cursor = Cursors.SizeWE;
        }
        else if (onRight)
        {
            _resizeEdge = HTRIGHT;
            this.Cursor = Cursors.SizeWE;
        }
        else if (onTop)
        {
            _resizeEdge = HTTOP;
            this.Cursor = Cursors.SizeNS;
        }
        else if (onBottom)
        {
            _resizeEdge = HTBOTTOM;
            this.Cursor = Cursors.SizeNS;
        }
        else
        {
            _resizeEdge = 0;
            this.Cursor = Cursors.Default;
        }
    }

    private void Form_MouseUp(object? sender, MouseEventArgs e)
    {
        _resizeEdge = 0;
    }

    private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
        }
    }

    private bool _isDesignMode
    {
        get
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return true;
            if (System.Diagnostics.Process.GetCurrentProcess().ProcessName == "devenv")
                return true;
            return false;
        }
    }

    private void ShowLoginAnimation()
    {
        var loginForm = new LoginForm();
        loginForm.LoginSuccessful += () => loginForm.Close();
        loginForm.ShowDialog();
    }

    private void SetupEventHandlers()
    {
        if (_controller == null) return;

        _controller.ProgressUpdated += (current, total) =>
        {
            this.Invoke((MethodInvoker)delegate
            {
                if (total > 0)
                    progressBar.Value = (int)((current / (double)total) * 100);
                lblProgress.Text = $"需发送: {total} 位";
            });
        };

        _controller.InfoBarShown += (success, msg) =>
        {
            this.Invoke((MethodInvoker)delegate
            {
                ShowToast(msg, success ? ToastType.Success : ToastType.Error);
            });
        };

        _controller.TaskListChanged += OnTaskListChanged;
        _controller.TaskCountdownTick += OnTaskCountdownTick;
        _controller.TaskStateChanged += OnTaskStateChanged;

        _controller.TryRestoreAllTasks();
    }

    private void ShowToast(string message, ToastType type)
    {
        var toast = new ToastNotification(this, message, type);
        toast.Show();
    }

    /// <summary>
    /// 配置 AutoUpdater 事件，使用自定义暗色 UI
    /// </summary>
    private void SetupAutoUpdater()
    {
        AutoUpdater.CheckForUpdateEvent += args =>
        {
            UpdateChecker.CloseLoading();

            if (IsDisposed) return;

            if (args.Error != null)
            {
                LogManager.Error("检查更新失败", args.Error);
                BeginInvoke(new Action(() =>
                {
                    if (!IsDisposed)
                        UpdateDialog.ShowError(this, $"检查更新失败：{args.Error.Message}");
                }));
                return;
            }

            if (!args.IsUpdateAvailable)
            {
                BeginInvoke(new Action(() =>
                {
                    if (!IsDisposed)
                        UpdateDialog.ShowNoUpdate(this);
                }));
                return;
            }

            var latestArgs = UpdateChecker.GetLatestArgs();
            var info = new UpdateInfo
            {
                HasUpdate = true,
                LatestVersion = args.CurrentVersion.ToString(),
                ReleaseNotes = $"请访问 GitHub Release 页面查看详细更新说明",
                ReleaseUrl = latestArgs?.ChangelogURL ?? "",
                DownloadUrl = latestArgs?.DownloadURL ?? "",
            };

            BeginInvoke(new Action(() =>
            {
                if (IsDisposed) return;

                UpdateDialog.ShowUpdateAvailable(this, info, onDownload: () =>
                {
                    try
                    {
                        if (AutoUpdater.DownloadUpdate(args))
                            Application.Exit();
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error("下载更新失败", ex);
                        UpdateDialog.ShowError(this, $"下载更新失败：{ex.Message}");
                    }
                });
            }));
        };
    }

    private void ExecuteSend()
    {
        if (_controller == null) return;

        var filePaths = new List<string>();
        foreach (var item in fileListBox.Items)
        {
            filePaths.Add(item.ToString() ?? string.Empty);
        }

        double textInterval = double.TryParse(cbTextInterval.SelectedItem?.ToString(), out double ti) ? ti : 0.05;
        double fileInterval = double.TryParse(cbFileInterval.SelectedItem?.ToString(), out double fi) ? fi : 0.25;

        var messageInfo = _controller.GetGuiInfo(
            txtSingleMsg.Text,
            txtMultiMsg.Text,
            filePaths,
            txtNames.Text,
            textInterval,
            fileInterval,
            rbEnter.Checked ? "{Enter}" : "{Ctrl}{Enter}"
        );

        _controller.OnSendClicked(messageInfo);
    }

    private void TogglePause()
    {
        _controller?.ToggleSendStatus();
        _isPaused = !_isPaused;
        btnPause.Text = _isPaused ? "继续" : "暂停";
    }

    private void ExportResults()
    {
        using var dlg = new SaveFileDialog { Filter = "CSV Files (*.csv)|*.csv", FileName = "运行结果.csv" };
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            _controller?.ExportExecResult(dlg.FileName);
        }
    }

    private void ClearAll()
    {
        txtSingleMsg.Clear();
        txtMultiMsg.Clear();
        txtNames.Clear();
        fileListBox.Items.Clear();
        _nameList.Clear();
        _nameListFile = "";
        progressBar.Value = 0;
        lblProgress.Text = "需发送: 0 位";
        ExitEditMode();
    }

    private void AddFiles()
    {
        using var dlg = new OpenFileDialog { Multiselect = true, Filter = "All Files (*.*)|*.*" };
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            foreach (var file in dlg.FileNames)
            {
                if (!fileListBox.Items.Contains(file))
                    fileListBox.Items.Add(file);
            }
        }
    }

    private void ImportNames()
    {
        using var dlg = new OpenFileDialog { Filter = "Text Files (*.txt)|*.txt" };
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            _nameList.Clear();
            _nameList.AddRange(FileIOManager.ReadFile(dlg.FileName));
            _nameListFile = dlg.FileName;
            txtNames.Text = string.Join(Environment.NewLine, _nameList);
            progressBar.Maximum = _nameList.Count;
            lblProgress.Text = $"需发送: {_nameList.Count} 位";
            ShowToast($"导入成功！共 {_nameList.Count} 个联系人", ToastType.Success);
            ExitEditMode();
        }
    }

    private async void ExportContacts()
    {
        if (_controller == null) return;
        if (btnExportContacts.Text == "导出中...") return;

        var originalText = btnExportContacts.Text;
        btnExportContacts.Text = "导出中...";
        btnExportContacts.Enabled = false;

        try
        {
            var contacts = await Task.Run(() => _controller.ExportContactsFromDb());
            if (contacts.Count == 0)
            {
                ShowToast("未找到联系人", ToastType.Error);
                return;
            }

            txtNames.Text = string.Join(Environment.NewLine, contacts);
            ShowToast($"导出成功！共 {contacts.Count} 个联系人", ToastType.Success);
        }
        catch (Exception ex) when (ex.Message.Contains("未找到微信数据目录"))
        {
            if (PromptManualDataDir())
            {
                btnExportContacts.Text = originalText;
                btnExportContacts.Enabled = true;
                ExportContacts();
                return;
            }
            ShowToast($"导出失败：{ex.Message}", ToastType.Error);
        }
        catch (Exception ex)
        {
            ShowToast($"导出失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            btnExportContacts.Text = originalText;
            btnExportContacts.Enabled = true;
        }
    }

    private async void ExportChatRoomMembers()
    {
        if (_controller == null) return;
        if (btnExportChatRooms.Text == "导出中...") return;

        var originalText = btnExportChatRooms.Text;
        btnExportChatRooms.Text = "导出中...";
        btnExportChatRooms.Enabled = false;

        try
        {
            var chatRooms = await Task.Run(() => _controller.ExportChatRoomListFromDb());
            if (chatRooms.Count == 0)
            {
                ShowToast("未找到群聊", ToastType.Error);
                return;
            }

            using var selectForm = new Form
            {
                Text = "选择群聊",
                Size = new Size(400, 500),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = ThemeColors.ContentBackground
            };

            var listBox = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.InputBackground,
                ForeColor = ThemeColors.TextPrimary,
                Font = new Font("Microsoft YaHei UI", 10F),
                BorderStyle = BorderStyle.None,
                CheckOnClick = true
            };
            foreach (var room in chatRooms.Keys)
                listBox.Items.Add(room);

            var btnConfirm = new Button
            {
                Text = "确定导出",
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = ThemeColors.PrimaryAccent,
                ForeColor = ThemeColors.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnConfirm.Click += async (s, e) =>
            {
                var selectedRooms = new List<string>();
                foreach (var item in listBox.CheckedItems)
                    selectedRooms.Add(item.ToString() ?? "");

                if (selectedRooms.Count == 0)
                {
                    MessageBox.Show("请至少选择一个群聊", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                btnConfirm.Text = "导出中...";
                btnConfirm.Enabled = false;
                try
                {
                    var allMembers = new List<string>();
                    foreach (var roomName in selectedRooms)
                    {
                        if (chatRooms.TryGetValue(roomName, out var wxid))
                        {
                            var members = await Task.Run(() => _controller.ExportChatRoomMembersFromDb(wxid));
                            allMembers.AddRange(members);
                        }
                    }

                    var distinctMembers = allMembers.Distinct().ToList();
                    txtNames.Text = string.Join(Environment.NewLine, distinctMembers);
                    ShowToast($"导出成功！共 {distinctMembers.Count} 个群成员", ToastType.Success);
                    selectForm.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出群成员失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnConfirm.Text = "确定导出";
                    btnConfirm.Enabled = true;
                }
            };

            selectForm.Controls.Add(listBox);
            selectForm.Controls.Add(btnConfirm);
            selectForm.ShowDialog(this);
        }
        catch (Exception ex)
        {
            ShowToast($"获取群聊失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            btnExportChatRooms.Text = originalText;
            btnExportChatRooms.Enabled = true;
        }
    }

    private void UpdateTitleBarButtons()
    {
        int w = titleBar.ClientSize.Width;
        int btnW = btnClose.Width;
        int spacing = 14;
        btnClose.Location = new Point(w - btnW - spacing, (titleBar.Height - btnW) / 2);
        btnMaximize.Location = new Point(w - btnW * 2 - spacing * 2, (titleBar.Height - btnW) / 2);
        btnMinimize.Location = new Point(w - btnW * 3 - spacing * 3, (titleBar.Height - btnW) / 2);
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        UpdateTitleBarButtons();
    }

    private void MainForm_SizeChanged(object? sender, EventArgs e)
    {
        UpdateTitleBarButtons();
    }

    private void BtnClose_Click(object? sender, EventArgs e) { this.Close(); }
    private void BtnMaximize_Click(object? sender, EventArgs e)
    {
        this.WindowState = this.WindowState == FormWindowState.Maximized
            ? FormWindowState.Normal : FormWindowState.Maximized;
    }
    private void BtnMinimize_Click(object? sender, EventArgs e) { this.WindowState = FormWindowState.Minimized; }

    private void BtnClearAll_Click(object? sender, EventArgs e) { ClearAll(); }
    private void BtnAddFile_Click(object? sender, EventArgs e) { AddFiles(); }
    private void BtnClearFiles_Click(object? sender, EventArgs e) { fileListBox.Items.Clear(); }
    private void BtnImportNames_Click(object? sender, EventArgs e) { ImportNames(); }
    private void BtnExportContacts_Click(object? sender, EventArgs e) { ExportContacts(); }
    private void BtnExportChatRooms_Click(object? sender, EventArgs e) { ExportChatRoomMembers(); }
    private void BtnExport_Click(object? sender, EventArgs e) { ExportResults(); }
    private void BtnPause_Click(object? sender, EventArgs e) { TogglePause(); }

    private void BtnSend_Click(object? sender, EventArgs e)
    {
        ExecuteSend();
    }

    private void BtnSchedule_Click(object? sender, EventArgs e)
    {
        if (_controller == null) return;

        var scheduledTime = dtpSchedule.Value;
        if (scheduledTime <= DateTime.Now)
        {
            ShowToast("定时时间必须在未来", ToastType.Error);
            return;
        }

        var taskInfo = CollectCurrentTaskInfo();
        taskInfo.ScheduledTime = scheduledTime;

        if (_editingTaskId != null)
        {
            taskInfo.TaskId = _editingTaskId;
            _controller.UpdateSchedule(_editingTaskId, taskInfo);
            ExitEditMode();
            ShowToast("定时任务已更新", ToastType.Success);
        }
        else
        {
            _controller.ScheduleSend(taskInfo);
            ShowToast("定时任务已创建", ToastType.Success);
        }
    }

    private void BtnTogglePanel_Click(object? sender, EventArgs e)
    {
        ToggleRightPanel();
    }

    private ScheduledTaskInfo CollectCurrentTaskInfo()
    {
        var filePaths = new List<string>();
        foreach (var item in fileListBox.Items)
            filePaths.Add(item.ToString() ?? string.Empty);

        double textInterval = double.TryParse(cbTextInterval.SelectedItem?.ToString(), out double ti) ? ti : 0.05;
        double fileInterval = double.TryParse(cbFileInterval.SelectedItem?.ToString(), out double fi) ? fi : 0.25;

        return new ScheduledTaskInfo
        {
            SingleText = txtSingleMsg.Text,
            MultiText = txtMultiMsg.Text,
            FilePaths = filePaths,
            Names = txtNames.Text,
            TextInterval = textInterval,
            FileInterval = fileInterval,
            SendShortcut = rbEnter.Checked ? "{Enter}" : "{Ctrl}{Enter}",
            NameListFile = _nameListFile
        };
    }

    #region 编辑模式

    private void EnterEditMode(string taskId)
    {
        var task = _controller?.GetScheduleTask(taskId);
        if (task == null) return;

        _editingTaskId = taskId;

        txtSingleMsg.Text = task.SingleText;
        txtMultiMsg.Text = task.MultiText;
        txtNames.Text = task.Names;
        fileListBox.Items.Clear();
        fileListBox.Items.AddRange(task.FilePaths.ToArray());
        dtpSchedule.Value = task.ScheduledTime > DateTime.Now ? task.ScheduledTime : DateTime.Now.AddMinutes(5);
        rbEnter.Checked = task.SendShortcut == "{Enter}";
        rbCtrlEnter.Checked = !rbEnter.Checked;

        btnSchedule.Text = "更新定时";
        btnSchedule.SetBaseColor(ThemeColors.SuccessAccent);
    }

    private void ExitEditMode()
    {
        _editingTaskId = null;
        btnSchedule.Text = "定时发送";
        btnSchedule.SetBaseColor(ThemeColors.SecondaryAccent);
    }

    #endregion

    #region 右侧面板

    private void ToggleRightPanel()
    {
        _rightPanelCollapsed = !_rightPanelCollapsed;
        rightPanel.Visible = !_rightPanelCollapsed;
    }

    private void OnTaskListChanged()
    {
        RefreshTaskList();
    }

    private void OnTaskStateChanged(string taskId, ScheduleState state)
    {
        if (state == ScheduleState.Sending && _editingTaskId == taskId)
        {
            ExitEditMode();
        }
        RefreshTaskList();
    }

    private void RefreshTaskList()
    {
        taskFlowPanel.Controls.Clear();
        var tasks = _controller?.GetAllScheduleTasks() ?? new();

        emptyLabel.Visible = tasks.Count == 0;
        taskFlowPanel.Visible = tasks.Count > 0;
        lblTaskCount.Text = $"({tasks.Count})";

        foreach (var (taskId, info, state) in tasks)
        {
            var card = CreateTaskCard(taskId, info, state);
            taskFlowPanel.Controls.Add(card);
        }
    }

    private Panel CreateTaskCard(string taskId, ScheduledTaskInfo info, ScheduleState state)
    {
        // ========== 卡片容器 ==========
        // 宽度计算：taskFlowPanel(480) - 左右Padding(12+12) - 滚动条预留(17) = 439
        const int cardW = 438;
        const int cardH = 200;
        const int padX = 14;
        // 日期行离顶部的距离
        const int padTop = 15;
        // 行高
        const int rowH = 50;
        const int rowGap = 8;

        var card = new Panel
        {
            Width = cardW,
            Height = cardH,
            BackColor = ThemeColors.InputBackground,
            Margin = new Padding(0, 0, 0, 10),
            Cursor = Cursors.Hand,
            Tag = taskId
        };

        // 圆角：用 Region 裁剪掉直角区域
        card.Region = new Region(
            UiHelper.CreateRoundedPath(new Rectangle(0, 0, cardW, cardH), 6));
        card.Paint += (s, e) =>
        {
            using var path = UiHelper.CreateRoundedPath(
                new Rectangle(0, 0, cardW - 1, cardH - 1), 6);
            using var pen = new Pen(ThemeColors.BorderSubtle, 1);
            e.Graphics.DrawPath(pen, path);
        };

        // ========== 第一行：时间（左） + 倒计时（右） ==========
        var row1Y = padTop;
        var lblTime = new Label
        {
            Text = $"{info.ScheduledTime:MM-dd HH:mm}",
            Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
            ForeColor = ThemeColors.TextPrimary,
            AutoSize = false,
            Location = new Point(padX, row1Y),
            Size = new Size(160, rowH),
            TextAlign = ContentAlignment.MiddleLeft,
            Tag = taskId
        };

        var btnCancel = new RoundButton
        {
            Text = "✕",
            Tag = taskId,
            Font = new Font("Segoe UI", 9F),
            BaseColor = ThemeColors.DangerLight,
            HoverColor = Color.FromArgb(250, 100, 95),
            PressColor = Color.FromArgb(200, 60, 55),
            ForeColor = ThemeColors.TextPrimary,
            CornerRadius = 6,
            Size = new Size(45, 45),
            Location = new Point(cardW - padX - 30, row1Y - 2),
            Cursor = Cursors.Hand
        };

        var lblCountdown = new Label
        {
            Name = $"countdown_{taskId}",
            Text = state == ScheduleState.Sending ? "发送中..." : "",
            Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
            ForeColor = ThemeColors.PrimaryAccent,
            AutoSize = false,
            Location = new Point(cardW - padX - 30 - 8 - 126, row1Y),
            Size = new Size(126, rowH),
            TextAlign = ContentAlignment.MiddleRight,
            Tag = taskId
        };

        // ========== 第二行：消息摘要 ==========
        var row2Y = row1Y + rowH + rowGap;
        var lblSummary = new Label
        {
            Text = GetSummary(info),
            Font = new Font("Microsoft YaHei UI", 9F),
            ForeColor = ThemeColors.TextSecondary,
            AutoSize = false,
            Location = new Point(padX, row2Y),
            Size = new Size(cardW - padX * 2, rowH),
            TextAlign = ContentAlignment.MiddleLeft,
            Tag = taskId
        };

        // ========== 第三行：名单（左） ==========
        var row3Y = row2Y + rowH + rowGap;
        var lblNames = new Label
        {
            Text = GetNamesPreview(info.Names),
            Font = new Font("Microsoft YaHei UI", 9F),
            ForeColor = ThemeColors.TextMuted,
            AutoSize = false,
            Location = new Point(padX, row3Y),
            Size = new Size(cardW - padX * 2, rowH),
            TextAlign = ContentAlignment.MiddleLeft,
            Tag = taskId
        };

        // 点击卡片进入编辑（取消按钮除外）
        EventHandler editHandler = (s, e) => EnterEditMode(taskId);
        card.Click += editHandler;
        lblTime.Click += editHandler;
        lblSummary.Click += editHandler;
        lblNames.Click += editHandler;
        lblCountdown.Click += editHandler;

        // 取消按钮
        btnCancel.Click += (s, e) =>
        {
            _controller?.CancelSchedule(taskId);
            if (_editingTaskId == taskId) ExitEditMode();
        };

        card.Controls.AddRange(new Control[] { lblTime, lblSummary, lblNames, lblCountdown, btnCancel });
        return card;
    }

    private static string GetSummary(ScheduledTaskInfo info)
    {
        var text = !string.IsNullOrEmpty(info.SingleText) ? info.SingleText
            : !string.IsNullOrEmpty(info.MultiText) ? info.MultiText
            : info.FilePaths.Count > 0 ? $"[{info.FilePaths.Count}个文件]"
            : "无内容";

        text = text.Replace("\n", " ").Replace("\r", "");
        return text.Length > 30 ? text[..30] + "..." : text;
    }

    private static string GetNamesPreview(string names)
    {
        if (string.IsNullOrEmpty(names)) return "";
        var firstLine = names.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
        var preview = firstLine.Length > 15 ? firstLine[..15] + "..." : firstLine;
        var lineCount = names.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
        return lineCount > 1 ? $"{preview} 等{lineCount}人" : preview;
    }

    private void OnTaskCountdownTick(string taskId, TimeSpan remaining)
    {
        if (taskFlowPanel == null || taskFlowPanel.IsDisposed) return;

        foreach (Control c in taskFlowPanel.Controls)
        {
            if (c.Tag as string == taskId)
            {
                var lbl = c.Controls[$"countdown_{taskId}"] as Label;
                if (lbl != null)
                {
                    lbl.Text = remaining.TotalHours >= 1
                        ? $"{(int)remaining.TotalHours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}"
                        : $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
                }
            }
        }
    }

    #endregion

    private bool PromptManualDataDir()
    {
        var result = MessageBox.Show(
            "自动检测不到微信数据目录，是否手动选择？\n\n" +
            "通常在：文档\\xwechat_files 或 文档\\WeChat Files 下\n" +
            "选择包含 wxid_ 开头文件夹的那个目录",
            "选择微信数据目录",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (result != DialogResult.Yes) return false;

        using var dlg = new FolderBrowserDialog
        {
            Description = "选择微信数据目录（包含 wxid_ 开头文件夹的目录，如 xwechat_files）",
            UseDescriptionForTitle = true
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            WeChatDbManager.SetManualDataDir(dlg.SelectedPath);
            return true;
        }

        return false;
    }

    private void RunDiag()
    {
        try
        {
            var result = WeChatKeyDiag.RunDiag();
            using var diagForm = new Form
            {
                Text = "微信数据库诊断",
                Size = new Size(600, 500),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = ThemeColors.ContentBackground
            };
            var textBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                BackColor = ThemeColors.DiagnosticBackground,
                ForeColor = ThemeColors.TextDiagnostic,
                Font = new Font("Consolas", 10F),
                Text = result,
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Vertical
            };
            diagForm.Controls.Add(textBox);
            diagForm.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"诊断失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

public enum ToastType { Success, Error, Info }

public class ToastNotification : Form
{
    private readonly System.Windows.Forms.Timer _timer;
    private int _opacity = 100;

    public ToastNotification(Form parent, string message, ToastType type)
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.Manual;

        var icon = type switch
        {
            ToastType.Success => "✓",
            ToastType.Error => "✕",
            _ => "ℹ"
        };
        var fullText = $"{icon}  {message}";
        var font = new Font("Microsoft YaHei UI", 13, FontStyle.Bold);

        var maxW = Math.Min(600, parent.Width - 40);
        var textSize = TextRenderer.MeasureText(fullText, font, new Size(maxW - 40, 0), TextFormatFlags.WordBreak);
        var w = Math.Max(300, textSize.Width + 40);
        var h = Math.Max(60, textSize.Height + 30);

        this.Size = new Size(w, h);
        this.Location = new Point(
            parent.Location.X + parent.Width - this.Width - 20,
            parent.Location.Y + 70
        );
        this.TopMost = true;
        this.ShowInTaskbar = false;
        this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 16, 16));

        this.BackColor = type switch
        {
            ToastType.Success => ThemeColors.ToastSuccess,
            ToastType.Error => ThemeColors.ToastError,
            _ => ThemeColors.ToastInfo
        };

        var label = new Label
        {
            Text = fullText,
            Dock = DockStyle.Fill,
            Font = font,
            ForeColor = ThemeColors.TextPrimary,
            TextAlign = ContentAlignment.MiddleCenter
        };
        this.Controls.Add(label);

        _timer = new System.Windows.Forms.Timer { Interval = 50 };
        _timer.Tick += (s, e) =>
        {
            _opacity -= 5;
            if (_opacity <= 0)
            {
                _opacity = 0;
                _timer.Stop();
                this.Close();
            }
            this.Opacity = _opacity / 100.0;
        };
    }

    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    public new void Show()
    {
        base.Show();
        var delayTimer = new System.Windows.Forms.Timer { Interval = 2000 };
        delayTimer.Tick += (s, e) =>
        {
            delayTimer.Stop();
            _timer.Start();
        };
        delayTimer.Start();
    }
}

/// <summary>
/// 暗色主题菜单渲染器
/// </summary>
internal class DarkMenuRenderer : ToolStripProfessionalRenderer
{
    public DarkMenuRenderer() : base(new DarkMenuColorTable()) { }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        if (e.Item is ToolStripMenuItem item)
        {
            e.TextColor = item.Selected ? ThemeColors.TextPrimary : ThemeColors.TextSecondary;
        }
        base.OnRenderItemText(e);
    }
}

internal class DarkMenuColorTable : ProfessionalColorTable
{
    public override Color MenuStripGradientBegin => ThemeColors.ToolbarBg;
    public override Color MenuStripGradientEnd => ThemeColors.ToolbarBg;
    public override Color MenuBorder => ThemeColors.BorderSubtle;
    public override Color MenuItemBorder => ThemeColors.PrimaryAccent;
    public override Color MenuItemSelected => ThemeColors.HoverOverlay;
    public override Color MenuItemSelectedGradientBegin => ThemeColors.HoverOverlay;
    public override Color MenuItemSelectedGradientEnd => ThemeColors.HoverOverlay;
    public override Color MenuItemPressedGradientBegin => ThemeColors.ButtonPress;
    public override Color MenuItemPressedGradientEnd => ThemeColors.ButtonPress;
    public override Color ImageMarginGradientBegin => ThemeColors.CardBackground;
    public override Color ImageMarginGradientMiddle => ThemeColors.CardBackground;
    public override Color ImageMarginGradientEnd => ThemeColors.CardBackground;
    public override Color SeparatorDark => ThemeColors.BorderSubtle;
    public override Color SeparatorLight => ThemeColors.BorderSubtle;
    public override Color ToolStripDropDownBackground => ThemeColors.CardBackground;
}
