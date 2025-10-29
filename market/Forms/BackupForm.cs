using System;
using System.IO;
using System.Windows.Forms;
using market.Services;

namespace market.Forms
{
    public partial class BackupForm : Form
    {
        private readonly BackupService _backupService;
        private readonly AuthService _authService;

        public BackupForm(BackupService backupService, AuthService authService)
        {
            _backupService = backupService;
            _authService = authService;
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // BackupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 400);
            this.Name = "BackupForm";
            this.Text = "数据备份";
            this.Load += new System.EventHandler(this.BackupForm_Load);
            this.ResumeLayout(false);

            // 标题标签
            var titleLabel = new Label
            {
                Text = "数据库备份管理",
                Font = new System.Drawing.Font("微软雅黑", 14, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(560, 30)
            };
            this.Controls.Add(titleLabel);

            // 备份路径标签
            var pathLabel = new Label
            {
                Text = "备份路径:",
                Location = new System.Drawing.Point(20, 80),
                Size = new System.Drawing.Size(80, 25)
            };
            this.Controls.Add(pathLabel);

            // 备份路径文本框
            txtBackupPath = new TextBox
            {
                Location = new System.Drawing.Point(100, 80),
                Size = new System.Drawing.Size(380, 25),
                ReadOnly = true
            };
            this.Controls.Add(txtBackupPath);

            // 浏览按钮
            btnBrowse = new Button
            {
                Text = "浏览...",
                Location = new System.Drawing.Point(490, 80),
                Size = new System.Drawing.Size(80, 25)
            };
            btnBrowse.Click += BtnBrowse_Click;
            this.Controls.Add(btnBrowse);

            // 备份文件名标签
            var fileNameLabel = new Label
            {
                Text = "备份文件名:",
                Location = new System.Drawing.Point(20, 120),
                Size = new System.Drawing.Size(80, 25)
            };
            this.Controls.Add(fileNameLabel);

            // 备份文件名文本框
            txtFileName = new TextBox
            {
                Location = new System.Drawing.Point(100, 120),
                Size = new System.Drawing.Size(470, 25)
            };
            this.Controls.Add(txtFileName);

            // 执行备份按钮
            btnBackup = new Button
            {
                Text = "执行备份",
                Location = new System.Drawing.Point(100, 200),
                Size = new System.Drawing.Size(150, 40),
                Font = new System.Drawing.Font("微软雅黑", 10, System.Drawing.FontStyle.Regular)
            };
            btnBackup.Click += BtnBackup_Click;
            this.Controls.Add(btnBackup);

            // 日志文本框
            txtLog = new TextBox
            {
                Location = new System.Drawing.Point(20, 260),
                Size = new System.Drawing.Size(550, 100),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(txtLog);

            // 退出按钮
            btnExit = new Button
            {
                Text = "退出",
                Location = new System.Drawing.Point(350, 200),
                Size = new System.Drawing.Size(150, 40),
                Font = new System.Drawing.Font("微软雅黑", 10, System.Drawing.FontStyle.Regular)
            };
            btnExit.Click += BtnExit_Click;
            this.Controls.Add(btnExit);
        }

        private TextBox txtBackupPath;
        private Button btnBrowse;
        private TextBox txtFileName;
        private Button btnBackup;
        private Button btnExit;
        private TextBox txtLog;

        private void InitializeUI()
        {
            // 设置默认备份路径
            string defaultDir = _backupService.GetDefaultBackupDirectory();
            txtBackupPath.Text = defaultDir;
            
            // 生成默认备份文件名
            txtFileName.Text = _backupService.GenerateBackupFileName();
            
            // 添加日志
            AddLog("系统就绪，准备备份数据库...");
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择备份文件保存位置";
                dialog.SelectedPath = txtBackupPath.Text;
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtBackupPath.Text = dialog.SelectedPath;
                }
            }
        }

        private void BtnBackup_Click(object sender, EventArgs e)
        {
            try
            {
                // 验证输入
                if (string.IsNullOrEmpty(txtBackupPath.Text))
                {
                    MessageBox.Show("请选择备份路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                if (string.IsNullOrEmpty(txtFileName.Text))
                {
                    MessageBox.Show("请输入备份文件名", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                // 组合完整路径
                string backupFilePath = Path.Combine(txtBackupPath.Text, txtFileName.Text);
                
                // 如果文件已存在，询问是否覆盖
                if (File.Exists(backupFilePath))
                {
                    DialogResult result = MessageBox.Show("文件已存在，是否覆盖？", "确认", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes)
                    {
                        return;
                    }
                }
                
                // 开始备份
                AddLog("开始备份数据库...");
                btnBackup.Enabled = false;
                btnExit.Enabled = false;
                
                // 执行备份
                bool success = _backupService.BackupDatabase(backupFilePath);
                
                if (success)
                {
                    AddLog($"备份成功！文件保存至: {backupFilePath}");
                    MessageBox.Show("数据库备份成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // 记录操作日志
                    LogService logService = new LogService(new DatabaseService());
                    logService.LogOperation(_authService.CurrentUser?.Username ?? "未知用户", 
                        "数据备份", "成功备份数据库到文件: " + txtFileName.Text);
                }
                else
                {
                    AddLog("备份失败！");
                }
            }
            catch (Exception ex)
            {
                AddLog($"备份过程发生错误: {ex.Message}");
                MessageBox.Show($"备份失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnBackup.Enabled = true;
                btnExit.Enabled = true;
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BackupForm_Load(object sender, EventArgs e)
        {
            // 窗体加载时的初始化
        }

        private void AddLog(string message)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
            txtLog.ScrollToCaret();
        }
    }
}