using System.Drawing;
using System.Windows.Forms;
using WeChatMassTool.Config;

namespace WeChatMassTool.Views
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        // 标题栏
        private Panel titleBar = null!;
        private Label lblTitle = null!;
        private RoundButton btnClose = null!;
        private RoundButton btnMaximize = null!;
        private RoundButton btnMinimize = null!;

        // 菜单栏
        private MenuStrip menuStrip = null!;
        private ToolStripMenuItem menuSettings = null!;
        private ToolStripMenuItem menuUpdate = null!;
        private ToolStripMenuItem menuAbout = null!;

        // 内容区
        private Panel contentArea = null!;
        private Panel leftPanel = null!;
        private Panel spacer = null!;

        // 消息区
        private CardPanel msgCard = null!;
        private Label lblMsgTitle = null!;
        private TextBox txtSingleMsg = null!;
        private TextBox txtMultiMsg = null!;
        private RoundButton btnClearAll = null!;

        // 文件区
        private CardPanel fileCard = null!;
        private Label lblFileTitle = null!;
        private ListBox fileListBox = null!;
        private RoundButton btnAddFile = null!;
        private RoundButton btnClearFiles = null!;

        // 对象区
        private CardPanel nameCard = null!;
        private Label lblNameTitle = null!;
        private TextBox txtNames = null!;
        private RoundButton btnImportNames = null!;
        private RoundButton btnExportContacts = null!;
        private RoundButton btnExportChatRooms = null!;

        // 发送控制区
        private CardPanel sendPanel = null!;
        private Label lblProgress = null!;
        private GradientProgressBar progressBar = null!;
        private RadioButton rbEnter = null!;
        private RadioButton rbCtrlEnter = null!;
        private Label lblTextInterval = null!;
        private ComboBox cbTextInterval = null!;
        private Label lblFileInterval = null!;
        private ComboBox cbFileInterval = null!;
        private Label lblScheduleTime = null!;
        private DateTimePicker dtpSchedule = null!;
        private RoundButton btnExport = null!;
        private RoundButton btnPause = null!;
        private RoundButton btnSchedule = null!;
        private RoundButton btnSend = null!;
        private RoundButton btnShowSchedule = null!;

        // 右侧面板
        private Panel rightPanel = null!;
        private Panel rightHeader = null!;
        private Label lblScheduleTitle = null!;
        private Label lblTaskCount = null!;
        private FlowLayoutPanel taskFlowPanel = null!;
        private Label emptyLabel = null!;

        // 状态栏
        private Panel statusBar = null!;
        private Label lblVersion = null!;
        private Label lblProvider = null!;

        private void InitializeComponent()
        {
            titleBar = new Panel();
            lblTitle = new Label();
            btnClose = new RoundButton();
            btnMaximize = new RoundButton();
            btnMinimize = new RoundButton();
            menuStrip = new MenuStrip();
            menuSettings = new ToolStripMenuItem();
            menuUpdate = new ToolStripMenuItem();
            menuAbout = new ToolStripMenuItem();
            contentArea = new Panel();
            leftPanel = new Panel();
            spacer = new Panel();
            sendPanel = new CardPanel();
            rbCtrlEnter = new RadioButton();
            lblProgress = new Label();
            progressBar = new GradientProgressBar();
            btnExport = new RoundButton();
            btnPause = new RoundButton();
            btnSchedule = new RoundButton();
            btnSend = new RoundButton();
            btnShowSchedule = new RoundButton();
            rbEnter = new RadioButton();
            lblTextInterval = new Label();
            cbTextInterval = new ComboBox();
            lblFileInterval = new Label();
            cbFileInterval = new ComboBox();
            lblScheduleTime = new Label();
            dtpSchedule = new DateTimePicker();
            nameCard = new CardPanel();
            lblNameTitle = new Label();
            txtNames = new TextBox();
            btnImportNames = new RoundButton();
            btnExportContacts = new RoundButton();
            btnExportChatRooms = new RoundButton();
            fileCard = new CardPanel();
            lblFileTitle = new Label();
            fileListBox = new ListBox();
            btnAddFile = new RoundButton();
            btnClearFiles = new RoundButton();
            msgCard = new CardPanel();
            lblMsgTitle = new Label();
            txtSingleMsg = new TextBox();
            txtMultiMsg = new TextBox();
            btnClearAll = new RoundButton();
            rightPanel = new Panel();
            rightHeader = new Panel();
            lblScheduleTitle = new Label();
            lblTaskCount = new Label();
            taskFlowPanel = new FlowLayoutPanel();
            emptyLabel = new Label();
            statusBar = new Panel();
            lblVersion = new Label();
            lblProvider = new Label();
            titleBar.SuspendLayout();
            menuStrip.SuspendLayout();
            contentArea.SuspendLayout();
            leftPanel.SuspendLayout();
            sendPanel.SuspendLayout();
            nameCard.SuspendLayout();
            fileCard.SuspendLayout();
            msgCard.SuspendLayout();
            rightPanel.SuspendLayout();
            rightHeader.SuspendLayout();
            taskFlowPanel.SuspendLayout();
            statusBar.SuspendLayout();
            SuspendLayout();
            // 
            // titleBar
            // 
            titleBar.BackColor = Color.FromArgb(33, 37, 43);
            titleBar.Controls.Add(lblTitle);
            titleBar.Controls.Add(btnClose);
            titleBar.Controls.Add(btnMaximize);
            titleBar.Controls.Add(btnMinimize);
            titleBar.Dock = DockStyle.Top;
            titleBar.Font = new Font("Microsoft YaHei UI", 12F);
            titleBar.Location = new Point(0, 0);
            titleBar.Name = "titleBar";
            titleBar.Size = new Size(1950, 76);
            titleBar.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 5);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(700, 71);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "WeChatMassTool - 微信群发";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.BackColor = Color.FromArgb(68, 81, 105);
            btnClose.BaseColor = Color.FromArgb(200, 60, 60);
            btnClose.CornerRadius = 8;
            btnClose.Cursor = Cursors.Hand;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 10F);
            btnClose.ForeColor = Color.White;
            btnClose.HoverColor = Color.FromArgb(220, 80, 80);
            btnClose.Location = new Point(1869, 9);
            btnClose.Name = "btnClose";
            btnClose.PressColor = Color.FromArgb(180, 50, 50);
            btnClose.Size = new Size(67, 56);
            btnClose.TabIndex = 1;
            btnClose.Text = "✕";
            btnClose.UseVisualStyleBackColor = false;
            // 
            // btnMaximize
            // 
            btnMaximize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMaximize.BackColor = Color.FromArgb(68, 81, 105);
            btnMaximize.BaseColor = Color.FromArgb(55, 60, 72);
            btnMaximize.CornerRadius = 8;
            btnMaximize.Cursor = Cursors.Hand;
            btnMaximize.FlatStyle = FlatStyle.Flat;
            btnMaximize.Font = new Font("Segoe UI", 10F);
            btnMaximize.ForeColor = Color.White;
            btnMaximize.HoverColor = Color.FromArgb(70, 76, 92);
            btnMaximize.Location = new Point(1796, 9);
            btnMaximize.Name = "btnMaximize";
            btnMaximize.PressColor = Color.FromArgb(45, 50, 60);
            btnMaximize.Size = new Size(72, 56);
            btnMaximize.TabIndex = 2;
            btnMaximize.Text = "☐";
            btnMaximize.UseVisualStyleBackColor = false;
            // 
            // btnMinimize
            // 
            btnMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMinimize.BackColor = Color.FromArgb(68, 81, 105);
            btnMinimize.BaseColor = Color.FromArgb(55, 60, 72);
            btnMinimize.CornerRadius = 8;
            btnMinimize.Cursor = Cursors.Hand;
            btnMinimize.FlatStyle = FlatStyle.Flat;
            btnMinimize.Font = new Font("Segoe UI", 10F);
            btnMinimize.ForeColor = Color.White;
            btnMinimize.HoverColor = Color.FromArgb(70, 76, 92);
            btnMinimize.Location = new Point(1731, 11);
            btnMinimize.Name = "btnMinimize";
            btnMinimize.PressColor = Color.FromArgb(45, 50, 60);
            btnMinimize.Size = new Size(59, 53);
            btnMinimize.TabIndex = 3;
            btnMinimize.Text = "─";
            btnMinimize.UseVisualStyleBackColor = false;
            // 
            // menuStrip
            // 
            menuStrip.BackColor = Color.FromArgb(30, 34, 41);
            menuStrip.Font = new Font("Microsoft YaHei UI", 10F);
            menuStrip.ForeColor = Color.FromArgb(170, 170, 170);
            menuStrip.ImageScalingSize = new Size(32, 32);
            menuStrip.Items.AddRange(new ToolStripItem[] { menuSettings, menuUpdate, menuAbout });
            menuStrip.Location = new Point(0, 76);
            menuStrip.Name = "menuStrip";
            menuStrip.Padding = new Padding(12, 6, 12, 6);
            menuStrip.Size = new Size(1950, 55);
            menuStrip.TabIndex = 1;
            menuStrip.Text = "menuStrip";
            // 
            // menuSettings
            // 
            menuSettings.ForeColor = Color.FromArgb(170, 170, 170);
            menuSettings.Name = "menuSettings";
            menuSettings.Padding = new Padding(6, 2, 6, 2);
            menuSettings.Size = new Size(85, 43);
            menuSettings.Text = "设置";
            // 
            // menuUpdate
            // 
            menuUpdate.ForeColor = Color.FromArgb(170, 170, 170);
            menuUpdate.Name = "menuUpdate";
            menuUpdate.Padding = new Padding(6, 2, 6, 2);
            menuUpdate.Size = new Size(139, 43);
            menuUpdate.Text = "检查更新";
            // 
            // menuAbout
            // 
            menuAbout.ForeColor = Color.FromArgb(170, 170, 170);
            menuAbout.Name = "menuAbout";
            menuAbout.Padding = new Padding(6, 2, 6, 2);
            menuAbout.Size = new Size(85, 43);
            menuAbout.Text = "关于";
            // 
            // contentArea
            // 
            contentArea.BackColor = Color.FromArgb(40, 44, 52);
            contentArea.Controls.Add(leftPanel);
            contentArea.Controls.Add(rightPanel);
            contentArea.Dock = DockStyle.Fill;
            contentArea.Location = new Point(0, 131);
            contentArea.Name = "contentArea";
            contentArea.Padding = new Padding(20);
            contentArea.Size = new Size(1950, 1079);
            contentArea.TabIndex = 2;
            // 
            // leftPanel
            // 
            leftPanel.BackColor = Color.FromArgb(40, 44, 52);
            leftPanel.Controls.Add(spacer);
            leftPanel.Controls.Add(sendPanel);
            leftPanel.Controls.Add(nameCard);
            leftPanel.Controls.Add(fileCard);
            leftPanel.Controls.Add(msgCard);
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.Location = new Point(20, 20);
            leftPanel.Name = "leftPanel";
            leftPanel.Size = new Size(1430, 1039);
            leftPanel.TabIndex = 0;
            // 
            // spacer
            // 
            spacer.BackColor = Color.FromArgb(40, 44, 52);
            spacer.Dock = DockStyle.Fill;
            spacer.Location = new Point(0, 835);
            spacer.Name = "spacer";
            spacer.Size = new Size(1430, 34);
            spacer.TabIndex = 4;
            // 
            // sendPanel
            // 
            sendPanel.CardBackColor = Color.FromArgb(35, 38, 45);
            sendPanel.Controls.Add(rbCtrlEnter);
            sendPanel.Controls.Add(lblProgress);
            sendPanel.Controls.Add(progressBar);
            sendPanel.Controls.Add(btnExport);
            sendPanel.Controls.Add(btnPause);
            sendPanel.Controls.Add(btnSchedule);
            sendPanel.Controls.Add(btnSend);
            sendPanel.Controls.Add(btnShowSchedule);
            sendPanel.Controls.Add(rbEnter);
            sendPanel.Controls.Add(lblTextInterval);
            sendPanel.Controls.Add(cbTextInterval);
            sendPanel.Controls.Add(lblFileInterval);
            sendPanel.Controls.Add(cbFileInterval);
            sendPanel.Controls.Add(lblScheduleTime);
            sendPanel.Controls.Add(dtpSchedule);
            sendPanel.CornerRadius = 10;
            sendPanel.Dock = DockStyle.Bottom;
            sendPanel.Location = new Point(0, 869);
            sendPanel.Name = "sendPanel";
            sendPanel.Padding = new Padding(1);
            sendPanel.ShowBorder = true;
            sendPanel.Size = new Size(1430, 170);
            sendPanel.TabIndex = 2;
            // 
            // rbCtrlEnter
            // 
            rbCtrlEnter.AutoSize = true;
            rbCtrlEnter.BackColor = Color.Transparent;
            rbCtrlEnter.Font = new Font("Microsoft YaHei UI", 9F);
            rbCtrlEnter.ForeColor = Color.White;
            rbCtrlEnter.Location = new Point(189, 13);
            rbCtrlEnter.Name = "rbCtrlEnter";
            rbCtrlEnter.Size = new Size(211, 35);
            rbCtrlEnter.TabIndex = 3;
            rbCtrlEnter.Text = "Ctrl+Enter发送";
            rbCtrlEnter.UseVisualStyleBackColor = false;
            // 
            // lblProgress
            // 
            lblProgress.Font = new Font("Microsoft YaHei UI", 9F);
            lblProgress.ForeColor = Color.White;
            lblProgress.Location = new Point(18, 69);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(130, 42);
            lblProgress.TabIndex = 0;
            lblProgress.Text = "需发送: 0 位";
            lblProgress.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.CornerRadius = 6;
            progressBar.GradientEnd = Color.FromArgb(255, 121, 198);
            progressBar.GradientStart = Color.FromArgb(189, 147, 249);
            progressBar.Location = new Point(18, 132);
            progressBar.Maximum = 100;
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(1394, 28);
            progressBar.TabIndex = 1;
            progressBar.TrackColor = Color.FromArgb(45, 48, 55);
            progressBar.Value = 0;
            // 
            // btnExport
            // 
            btnExport.BackColor = Color.FromArgb(68, 81, 105);
            btnExport.BaseColor = Color.FromArgb(68, 81, 105);
            btnExport.CornerRadius = 8;
            btnExport.Cursor = Cursors.Hand;
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.Font = new Font("Microsoft YaHei UI", 9.5F);
            btnExport.ForeColor = Color.White;
            btnExport.HoverColor = Color.FromArgb(82, 96, 124);
            btnExport.Location = new Point(170, 61);
            btnExport.Name = "btnExport";
            btnExport.PressColor = Color.FromArgb(55, 65, 85);
            btnExport.Size = new Size(170, 50);
            btnExport.TabIndex = 10;
            btnExport.Text = "导出结果";
            btnExport.UseVisualStyleBackColor = false;
            // 
            // btnPause
            // 
            btnPause.BackColor = Color.FromArgb(68, 81, 105);
            btnPause.BaseColor = Color.FromArgb(68, 81, 105);
            btnPause.CornerRadius = 8;
            btnPause.Cursor = Cursors.Hand;
            btnPause.FlatStyle = FlatStyle.Flat;
            btnPause.Font = new Font("Microsoft YaHei UI", 9.5F);
            btnPause.ForeColor = Color.White;
            btnPause.HoverColor = Color.FromArgb(82, 96, 124);
            btnPause.Location = new Point(358, 61);
            btnPause.Name = "btnPause";
            btnPause.PressColor = Color.FromArgb(55, 65, 85);
            btnPause.Size = new Size(170, 50);
            btnPause.TabIndex = 11;
            btnPause.Text = "暂停发送";
            btnPause.UseVisualStyleBackColor = false;
            // 
            // btnSchedule
            // 
            btnSchedule.BackColor = Color.FromArgb(68, 81, 105);
            btnSchedule.BaseColor = Color.FromArgb(52, 152, 219);
            btnSchedule.CornerRadius = 8;
            btnSchedule.Cursor = Cursors.Hand;
            btnSchedule.FlatStyle = FlatStyle.Flat;
            btnSchedule.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
            btnSchedule.ForeColor = Color.White;
            btnSchedule.HoverColor = Color.FromArgb(72, 168, 235);
            btnSchedule.Location = new Point(546, 61);
            btnSchedule.Name = "btnSchedule";
            btnSchedule.PressColor = Color.FromArgb(42, 135, 200);
            btnSchedule.Size = new Size(170, 50);
            btnSchedule.TabIndex = 12;
            btnSchedule.Text = "定时发送";
            btnSchedule.UseVisualStyleBackColor = false;
            // 
            // btnSend
            // 
            btnSend.BackColor = Color.FromArgb(68, 81, 105);
            btnSend.BaseColor = Color.FromArgb(189, 147, 249);
            btnSend.CornerRadius = 8;
            btnSend.Cursor = Cursors.Hand;
            btnSend.FlatStyle = FlatStyle.Flat;
            btnSend.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
            btnSend.ForeColor = Color.White;
            btnSend.HoverColor = Color.FromArgb(205, 165, 252);
            btnSend.Location = new Point(736, 61);
            btnSend.Name = "btnSend";
            btnSend.PressColor = Color.FromArgb(170, 130, 230);
            btnSend.Size = new Size(170, 50);
            btnSend.TabIndex = 13;
            btnSend.Text = "开始发送";
            btnSend.UseVisualStyleBackColor = false;
            // 
            // btnShowSchedule
            // 
            btnShowSchedule.BackColor = Color.FromArgb(68, 81, 105);
            btnShowSchedule.BaseColor = Color.FromArgb(52, 152, 219);
            btnShowSchedule.CornerRadius = 6;
            btnShowSchedule.Cursor = Cursors.Hand;
            btnShowSchedule.FlatStyle = FlatStyle.Flat;
            btnShowSchedule.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
            btnShowSchedule.ForeColor = Color.White;
            btnShowSchedule.HoverColor = Color.FromArgb(72, 168, 235);
            btnShowSchedule.Location = new Point(926, 61);
            btnShowSchedule.Name = "btnShowSchedule";
            btnShowSchedule.PressColor = Color.FromArgb(42, 135, 200);
            btnShowSchedule.Size = new Size(67, 56);
            btnShowSchedule.TabIndex = 14;
            btnShowSchedule.Text = "📋";
            btnShowSchedule.UseVisualStyleBackColor = false;
            // 
            // rbEnter
            // 
            rbEnter.AutoSize = true;
            rbEnter.BackColor = Color.Transparent;
            rbEnter.Checked = true;
            rbEnter.Font = new Font("Microsoft YaHei UI", 9F);
            rbEnter.ForeColor = Color.White;
            rbEnter.Location = new Point(18, 12);
            rbEnter.Name = "rbEnter";
            rbEnter.Size = new Size(153, 35);
            rbEnter.TabIndex = 2;
            rbEnter.TabStop = true;
            rbEnter.Text = "Enter发送";
            rbEnter.UseVisualStyleBackColor = false;
            // 
            // lblTextInterval
            // 
            lblTextInterval.Font = new Font("Microsoft YaHei UI", 9F);
            lblTextInterval.ForeColor = Color.FromArgb(170, 170, 170);
            lblTextInterval.Location = new Point(406, 9);
            lblTextInterval.Name = "lblTextInterval";
            lblTextInterval.Size = new Size(129, 41);
            lblTextInterval.TabIndex = 4;
            lblTextInterval.Text = "文本间隔";
            lblTextInterval.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cbTextInterval
            // 
            cbTextInterval.BackColor = Color.FromArgb(40, 44, 52);
            cbTextInterval.DropDownStyle = ComboBoxStyle.DropDownList;
            cbTextInterval.FlatStyle = FlatStyle.Flat;
            cbTextInterval.Font = new Font("Microsoft YaHei UI", 9F);
            cbTextInterval.ForeColor = Color.White;
            cbTextInterval.FormattingEnabled = true;
            cbTextInterval.Location = new Point(559, 10);
            cbTextInterval.Name = "cbTextInterval";
            cbTextInterval.Size = new Size(107, 39);
            cbTextInterval.TabIndex = 5;
            // 
            // lblFileInterval
            // 
            lblFileInterval.Font = new Font("Microsoft YaHei UI", 9F);
            lblFileInterval.ForeColor = Color.FromArgb(170, 170, 170);
            lblFileInterval.Location = new Point(690, 8);
            lblFileInterval.Name = "lblFileInterval";
            lblFileInterval.Size = new Size(136, 44);
            lblFileInterval.TabIndex = 6;
            lblFileInterval.Text = "文件间隔";
            lblFileInterval.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cbFileInterval
            // 
            cbFileInterval.BackColor = Color.FromArgb(40, 44, 52);
            cbFileInterval.DropDownStyle = ComboBoxStyle.DropDownList;
            cbFileInterval.FlatStyle = FlatStyle.Flat;
            cbFileInterval.Font = new Font("Microsoft YaHei UI", 9F);
            cbFileInterval.ForeColor = Color.White;
            cbFileInterval.FormattingEnabled = true;
            cbFileInterval.Location = new Point(832, 10);
            cbFileInterval.Name = "cbFileInterval";
            cbFileInterval.Size = new Size(119, 39);
            cbFileInterval.TabIndex = 7;
            // 
            // lblScheduleTime
            // 
            lblScheduleTime.Font = new Font("Microsoft YaHei UI", 9F);
            lblScheduleTime.ForeColor = Color.FromArgb(170, 170, 170);
            lblScheduleTime.Location = new Point(1019, 10);
            lblScheduleTime.Name = "lblScheduleTime";
            lblScheduleTime.Size = new Size(100, 37);
            lblScheduleTime.TabIndex = 8;
            lblScheduleTime.Text = "定时:";
            lblScheduleTime.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // dtpSchedule
            // 
            dtpSchedule.CustomFormat = "yyyy-MM-dd HH:mm";
            dtpSchedule.Font = new Font("Microsoft YaHei UI", 9F);
            dtpSchedule.Format = DateTimePickerFormat.Custom;
            dtpSchedule.Location = new Point(1135, 10);
            dtpSchedule.Name = "dtpSchedule";
            dtpSchedule.ShowUpDown = true;
            dtpSchedule.Size = new Size(250, 38);
            dtpSchedule.TabIndex = 9;
            // 
            // nameCard
            // 
            nameCard.CardBackColor = Color.FromArgb(35, 38, 45);
            nameCard.Controls.Add(lblNameTitle);
            nameCard.Controls.Add(txtNames);
            nameCard.Controls.Add(btnImportNames);
            nameCard.Controls.Add(btnExportContacts);
            nameCard.Controls.Add(btnExportChatRooms);
            nameCard.CornerRadius = 10;
            nameCard.Dock = DockStyle.Top;
            nameCard.Location = new Point(0, 575);
            nameCard.Name = "nameCard";
            nameCard.Padding = new Padding(1);
            nameCard.ShowBorder = true;
            nameCard.Size = new Size(1430, 260);
            nameCard.TabIndex = 3;
            // 
            // lblNameTitle
            // 
            lblNameTitle.BackColor = Color.FromArgb(35, 38, 45);
            lblNameTitle.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            lblNameTitle.ForeColor = Color.White;
            lblNameTitle.Location = new Point(18, 14);
            lblNameTitle.Name = "lblNameTitle";
            lblNameTitle.Size = new Size(200, 60);
            lblNameTitle.TabIndex = 0;
            lblNameTitle.Text = "👥 对象";
            lblNameTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtNames
            // 
            txtNames.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtNames.BackColor = Color.FromArgb(45, 48, 55);
            txtNames.BorderStyle = BorderStyle.None;
            txtNames.Font = new Font("Microsoft YaHei UI", 10F);
            txtNames.ForeColor = Color.White;
            txtNames.Location = new Point(18, 77);
            txtNames.Multiline = true;
            txtNames.Name = "txtNames";
            txtNames.PlaceholderText = "在此输入好友昵称，一行为一个好友";
            txtNames.Size = new Size(1200, 173);
            txtNames.TabIndex = 1;
            // 
            // btnImportNames
            // 
            btnImportNames.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnImportNames.BackColor = Color.FromArgb(68, 81, 105);
            btnImportNames.BaseColor = Color.FromArgb(68, 81, 105);
            btnImportNames.CornerRadius = 8;
            btnImportNames.Cursor = Cursors.Hand;
            btnImportNames.FlatStyle = FlatStyle.Flat;
            btnImportNames.Font = new Font("Microsoft YaHei UI", 10F);
            btnImportNames.ForeColor = Color.White;
            btnImportNames.HoverColor = Color.FromArgb(82, 96, 124);
            btnImportNames.Location = new Point(1244, 77);
            btnImportNames.Name = "btnImportNames";
            btnImportNames.PressColor = Color.FromArgb(55, 65, 85);
            btnImportNames.Size = new Size(170, 50);
            btnImportNames.TabIndex = 2;
            btnImportNames.Text = "导入名单";
            btnImportNames.UseVisualStyleBackColor = false;
            // 
            // btnExportContacts
            // 
            btnExportContacts.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExportContacts.BackColor = Color.FromArgb(68, 81, 105);
            btnExportContacts.BaseColor = Color.FromArgb(68, 81, 105);
            btnExportContacts.CornerRadius = 8;
            btnExportContacts.Cursor = Cursors.Hand;
            btnExportContacts.FlatStyle = FlatStyle.Flat;
            btnExportContacts.Font = new Font("Microsoft YaHei UI", 10F);
            btnExportContacts.ForeColor = Color.White;
            btnExportContacts.HoverColor = Color.FromArgb(82, 96, 124);
            btnExportContacts.Location = new Point(1244, 139);
            btnExportContacts.Name = "btnExportContacts";
            btnExportContacts.PressColor = Color.FromArgb(55, 65, 85);
            btnExportContacts.Size = new Size(170, 50);
            btnExportContacts.TabIndex = 3;
            btnExportContacts.Text = "导出联系人";
            btnExportContacts.UseVisualStyleBackColor = false;
            // 
            // btnExportChatRooms
            // 
            btnExportChatRooms.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExportChatRooms.BackColor = Color.FromArgb(68, 81, 105);
            btnExportChatRooms.BaseColor = Color.FromArgb(68, 81, 105);
            btnExportChatRooms.CornerRadius = 8;
            btnExportChatRooms.Cursor = Cursors.Hand;
            btnExportChatRooms.FlatStyle = FlatStyle.Flat;
            btnExportChatRooms.Font = new Font("Microsoft YaHei UI", 10F);
            btnExportChatRooms.ForeColor = Color.White;
            btnExportChatRooms.HoverColor = Color.FromArgb(82, 96, 124);
            btnExportChatRooms.Location = new Point(1244, 200);
            btnExportChatRooms.Name = "btnExportChatRooms";
            btnExportChatRooms.PressColor = Color.FromArgb(55, 65, 85);
            btnExportChatRooms.Size = new Size(170, 50);
            btnExportChatRooms.TabIndex = 4;
            btnExportChatRooms.Text = "导出群成员";
            btnExportChatRooms.UseVisualStyleBackColor = false;
            // 
            // fileCard
            // 
            fileCard.CardBackColor = Color.FromArgb(35, 38, 45);
            fileCard.Controls.Add(lblFileTitle);
            fileCard.Controls.Add(fileListBox);
            fileCard.Controls.Add(btnAddFile);
            fileCard.Controls.Add(btnClearFiles);
            fileCard.CornerRadius = 10;
            fileCard.Dock = DockStyle.Top;
            fileCard.Location = new Point(0, 311);
            fileCard.Name = "fileCard";
            fileCard.Padding = new Padding(1);
            fileCard.ShowBorder = true;
            fileCard.Size = new Size(1430, 264);
            fileCard.TabIndex = 1;
            // 
            // lblFileTitle
            // 
            lblFileTitle.BackColor = Color.FromArgb(35, 38, 45);
            lblFileTitle.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            lblFileTitle.ForeColor = Color.White;
            lblFileTitle.Location = new Point(18, 14);
            lblFileTitle.Name = "lblFileTitle";
            lblFileTitle.Size = new Size(200, 60);
            lblFileTitle.TabIndex = 0;
            lblFileTitle.Text = "📁 文件";
            lblFileTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // fileListBox
            // 
            fileListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            fileListBox.BackColor = Color.FromArgb(45, 48, 55);
            fileListBox.BorderStyle = BorderStyle.None;
            fileListBox.Font = new Font("Microsoft YaHei UI", 10F);
            fileListBox.ForeColor = Color.White;
            fileListBox.ItemHeight = 35;
            fileListBox.Location = new Point(18, 77);
            fileListBox.Name = "fileListBox";
            fileListBox.Size = new Size(1200, 175);
            fileListBox.TabIndex = 1;
            // 
            // btnAddFile
            // 
            btnAddFile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAddFile.BackColor = Color.FromArgb(68, 81, 105);
            btnAddFile.BaseColor = Color.FromArgb(68, 81, 105);
            btnAddFile.CornerRadius = 8;
            btnAddFile.Cursor = Cursors.Hand;
            btnAddFile.FlatStyle = FlatStyle.Flat;
            btnAddFile.Font = new Font("Microsoft YaHei UI", 10F);
            btnAddFile.ForeColor = Color.White;
            btnAddFile.HoverColor = Color.FromArgb(82, 96, 124);
            btnAddFile.Location = new Point(1244, 77);
            btnAddFile.Name = "btnAddFile";
            btnAddFile.PressColor = Color.FromArgb(55, 65, 85);
            btnAddFile.Size = new Size(170, 50);
            btnAddFile.TabIndex = 2;
            btnAddFile.Text = "添加文件";
            btnAddFile.UseVisualStyleBackColor = false;
            // 
            // btnClearFiles
            // 
            btnClearFiles.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearFiles.BackColor = Color.FromArgb(68, 81, 105);
            btnClearFiles.BaseColor = Color.FromArgb(68, 81, 105);
            btnClearFiles.CornerRadius = 8;
            btnClearFiles.Cursor = Cursors.Hand;
            btnClearFiles.FlatStyle = FlatStyle.Flat;
            btnClearFiles.Font = new Font("Microsoft YaHei UI", 10F);
            btnClearFiles.ForeColor = Color.White;
            btnClearFiles.HoverColor = Color.FromArgb(82, 96, 124);
            btnClearFiles.Location = new Point(1244, 136);
            btnClearFiles.Name = "btnClearFiles";
            btnClearFiles.PressColor = Color.FromArgb(55, 65, 85);
            btnClearFiles.Size = new Size(170, 50);
            btnClearFiles.TabIndex = 3;
            btnClearFiles.Text = "清空文件";
            btnClearFiles.UseVisualStyleBackColor = false;
            // 
            // msgCard
            // 
            msgCard.CardBackColor = Color.FromArgb(35, 38, 45);
            msgCard.Controls.Add(lblMsgTitle);
            msgCard.Controls.Add(txtSingleMsg);
            msgCard.Controls.Add(txtMultiMsg);
            msgCard.Controls.Add(btnClearAll);
            msgCard.CornerRadius = 10;
            msgCard.Dock = DockStyle.Top;
            msgCard.Location = new Point(0, 0);
            msgCard.Name = "msgCard";
            msgCard.Padding = new Padding(1);
            msgCard.ShowBorder = true;
            msgCard.Size = new Size(1430, 311);
            msgCard.TabIndex = 0;
            // 
            // lblMsgTitle
            // 
            lblMsgTitle.BackColor = Color.FromArgb(35, 38, 45);
            lblMsgTitle.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            lblMsgTitle.ForeColor = Color.White;
            lblMsgTitle.Location = new Point(18, 14);
            lblMsgTitle.Name = "lblMsgTitle";
            lblMsgTitle.Size = new Size(200, 60);
            lblMsgTitle.TabIndex = 0;
            lblMsgTitle.Text = "📝 消息";
            lblMsgTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtSingleMsg
            // 
            txtSingleMsg.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            txtSingleMsg.BackColor = Color.FromArgb(45, 48, 55);
            txtSingleMsg.BorderStyle = BorderStyle.None;
            txtSingleMsg.Font = new Font("Microsoft YaHei UI", 10F);
            txtSingleMsg.ForeColor = Color.White;
            txtSingleMsg.Location = new Point(18, 78);
            txtSingleMsg.Multiline = true;
            txtSingleMsg.Name = "txtSingleMsg";
            txtSingleMsg.PlaceholderText = "在此处输入消息，一行为一条消息";
            txtSingleMsg.Size = new Size(592, 211);
            txtSingleMsg.TabIndex = 1;
            // 
            // txtMultiMsg
            // 
            txtMultiMsg.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtMultiMsg.BackColor = Color.FromArgb(45, 48, 55);
            txtMultiMsg.BorderStyle = BorderStyle.None;
            txtMultiMsg.Font = new Font("Microsoft YaHei UI", 10F);
            txtMultiMsg.ForeColor = Color.White;
            txtMultiMsg.Location = new Point(632, 78);
            txtMultiMsg.Multiline = true;
            txtMultiMsg.Name = "txtMultiMsg";
            txtMultiMsg.PlaceholderText = "在此处输入消息，一共为一条消息";
            txtMultiMsg.Size = new Size(586, 211);
            txtMultiMsg.TabIndex = 2;
            // 
            // btnClearAll
            // 
            btnClearAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearAll.BackColor = Color.FromArgb(68, 81, 105);
            btnClearAll.BaseColor = Color.FromArgb(68, 81, 105);
            btnClearAll.CornerRadius = 8;
            btnClearAll.Cursor = Cursors.Hand;
            btnClearAll.FlatStyle = FlatStyle.Flat;
            btnClearAll.Font = new Font("Microsoft YaHei UI", 10F);
            btnClearAll.ForeColor = Color.White;
            btnClearAll.HoverColor = Color.FromArgb(82, 96, 124);
            btnClearAll.Location = new Point(1244, 78);
            btnClearAll.Name = "btnClearAll";
            btnClearAll.PressColor = Color.FromArgb(55, 65, 85);
            btnClearAll.Size = new Size(170, 50);
            btnClearAll.TabIndex = 3;
            btnClearAll.Text = "清空输入";
            btnClearAll.UseVisualStyleBackColor = false;
            // 
            // rightPanel
            // 
            rightPanel.BackColor = Color.FromArgb(35, 38, 45);
            rightPanel.Controls.Add(rightHeader);
            rightPanel.Controls.Add(taskFlowPanel);
            rightPanel.Dock = DockStyle.Right;
            rightPanel.Location = new Point(1450, 20);
            rightPanel.Name = "rightPanel";
            rightPanel.Size = new Size(480, 1039);
            rightPanel.TabIndex = 1;
            // 
            // rightHeader
            // 
            rightHeader.BackColor = Color.FromArgb(33, 37, 43);
            rightHeader.Controls.Add(lblScheduleTitle);
            rightHeader.Controls.Add(lblTaskCount);
            rightHeader.Dock = DockStyle.Top;
            rightHeader.Location = new Point(0, 0);
            rightHeader.Name = "rightHeader";
            rightHeader.Size = new Size(480, 50);
            rightHeader.TabIndex = 0;
            // 
            // lblScheduleTitle
            // 
            lblScheduleTitle.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
            lblScheduleTitle.ForeColor = Color.White;
            lblScheduleTitle.Location = new Point(12, 6);
            lblScheduleTitle.Name = "lblScheduleTitle";
            lblScheduleTitle.Size = new Size(140, 38);
            lblScheduleTitle.TabIndex = 0;
            lblScheduleTitle.Text = "🕐 定时任务";
            lblScheduleTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTaskCount
            // 
            lblTaskCount.Font = new Font("Microsoft YaHei UI", 9F);
            lblTaskCount.ForeColor = Color.FromArgb(150, 150, 150);
            lblTaskCount.Location = new Point(155, 8);
            lblTaskCount.Name = "lblTaskCount";
            lblTaskCount.Size = new Size(50, 34);
            lblTaskCount.TabIndex = 1;
            lblTaskCount.Text = "(0)";
            lblTaskCount.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // taskFlowPanel
            // 
            taskFlowPanel.AutoScroll = true;
            taskFlowPanel.BackColor = Color.FromArgb(35, 38, 45);
            taskFlowPanel.Controls.Add(emptyLabel);
            taskFlowPanel.Dock = DockStyle.Fill;
            taskFlowPanel.FlowDirection = FlowDirection.TopDown;
            taskFlowPanel.Location = new Point(0, 0);
            taskFlowPanel.Name = "taskFlowPanel";
            taskFlowPanel.Padding = new Padding(8, 6, 8, 6);
            taskFlowPanel.Size = new Size(480, 1039);
            taskFlowPanel.TabIndex = 1;
            taskFlowPanel.Visible = false;
            taskFlowPanel.WrapContents = false;
            // 
            // emptyLabel
            // 
            emptyLabel.BackColor = Color.Transparent;
            emptyLabel.Dock = DockStyle.Fill;
            emptyLabel.Font = new Font("Microsoft YaHei UI", 9F);
            emptyLabel.ForeColor = Color.FromArgb(120, 120, 120);
            emptyLabel.Location = new Point(11, 6);
            emptyLabel.Name = "emptyLabel";
            emptyLabel.Size = new Size(0, 1000);
            emptyLabel.TabIndex = 2;
            emptyLabel.Text = "暂无定时任务\r\n设定定时发送后显示在此";
            emptyLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // statusBar
            // 
            statusBar.BackColor = Color.FromArgb(33, 37, 43);
            statusBar.Controls.Add(lblVersion);
            statusBar.Controls.Add(lblProvider);
            statusBar.Dock = DockStyle.Bottom;
            statusBar.Location = new Point(0, 1210);
            statusBar.Name = "statusBar";
            statusBar.Size = new Size(1950, 38);
            statusBar.TabIndex = 3;
            // 
            // lblVersion
            // 
            lblVersion.Dock = DockStyle.Right;
            lblVersion.Font = new Font("Microsoft YaHei UI", 8F);
            lblVersion.ForeColor = Color.FromArgb(150, 150, 150);
            lblVersion.Location = new Point(1769, 0);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(181, 38);
            lblVersion.TabIndex = 0;
            lblVersion.Text = "Version: v1.0";
            lblVersion.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblProvider
            // 
            lblProvider.Dock = DockStyle.Left;
            lblProvider.Font = new Font("Microsoft YaHei UI", 8F);
            lblProvider.ForeColor = Color.FromArgb(150, 150, 150);
            lblProvider.Location = new Point(0, 0);
            lblProvider.Name = "lblProvider";
            lblProvider.Size = new Size(300, 38);
            lblProvider.TabIndex = 1;
            lblProvider.Text = "By YHL";
            lblProvider.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // MainForm
            // 
            BackColor = Color.FromArgb(27, 29, 35);
            ClientSize = new Size(1950, 1248);
            Controls.Add(contentArea);
            Controls.Add(menuStrip);
            Controls.Add(titleBar);
            Controls.Add(statusBar);
            FormBorderStyle = FormBorderStyle.None;
            MinimumSize = new Size(1000, 700);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Shown += MainForm_Shown;
            SizeChanged += MainForm_SizeChanged;
            titleBar.ResumeLayout(false);
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            contentArea.ResumeLayout(false);
            leftPanel.ResumeLayout(false);
            sendPanel.ResumeLayout(false);
            sendPanel.PerformLayout();
            nameCard.ResumeLayout(false);
            nameCard.PerformLayout();
            fileCard.ResumeLayout(false);
            msgCard.ResumeLayout(false);
            msgCard.PerformLayout();
            rightPanel.ResumeLayout(false);
            rightHeader.ResumeLayout(false);
            taskFlowPanel.ResumeLayout(false);
            statusBar.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
