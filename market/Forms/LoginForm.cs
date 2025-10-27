using System;
using System.Windows.Forms;
using market.Services;

namespace market.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService;

        public LoginForm(AuthService authService)
        {
            _authService = authService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "超市管理系统 - 登录";
            this.Size = new System.Drawing.Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 用户名标签
            var lblUsername = new Label
            {
                Text = "用户名:",
                Location = new System.Drawing.Point(50, 50),
                Size = new System.Drawing.Size(80, 25)
            };

            // 用户名文本框
            var txtUsername = new TextBox
            {
                Location = new System.Drawing.Point(130, 50),
                Size = new System.Drawing.Size(200, 25)
            };

            // 密码标签
            var lblPassword = new Label
            {
                Text = "密码:",
                Location = new System.Drawing.Point(50, 100),
                Size = new System.Drawing.Size(80, 25)
            };

            // 密码文本框
            var txtPassword = new TextBox
            {
                Location = new System.Drawing.Point(130, 100),
                Size = new System.Drawing.Size(200, 25),
                PasswordChar = '*'
            };

            // 登录按钮
            var btnLogin = new Button
            {
                Text = "登录",
                Location = new System.Drawing.Point(130, 150),
                Size = new System.Drawing.Size(80, 30)
            };

            // 取消按钮
            var btnCancel = new Button
            {
                Text = "取消",
                Location = new System.Drawing.Point(220, 150),
                Size = new System.Drawing.Size(80, 30)
            };

            // 事件处理
            btnLogin.Click += (s, e) =>
            {
                var username = txtUsername.Text.Trim();
                var password = txtPassword.Text;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("请输入用户名和密码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_authService.Login(username, password))
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("用户名或密码错误", "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnCancel.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            // 按键事件
            txtPassword.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    btnLogin.PerformClick();
                }
            };

            // 添加控件到窗体
            this.Controls.AddRange(new Control[] { lblUsername, txtUsername, lblPassword, txtPassword, btnLogin, btnCancel });
        }
    }
}