using System;
using System.Windows.Forms;
using market.Forms;
using market.Services;

namespace market
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            try
            {
                // 初始化数据库服务
                var databaseService = new DatabaseService();
                
                // 测试数据库连接
                if (!databaseService.TestConnection())
                {
                    MessageBox.Show("数据库连接失败，请检查MariaDB服务器是否正常运行。", "数据库错误", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // 验证数据库表是否存在
                if (!databaseService.ValidateDatabaseTables())
                {
                    MessageBox.Show("数据库表初始化失败，可能需要重新创建数据库。", "数据库错误", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // 初始化认证服务
                var authService = new AuthService(databaseService);
                
                // 显示登录窗口
                using (var loginForm = new LoginForm(authService))
                {
                    var result = loginForm.ShowDialog();
                    
                    if (result == DialogResult.OK && authService.IsLoggedIn)
                    {
                        // 登录成功，显示主窗口
                        Application.Run(new MainForm(authService, databaseService));
                    }
                    else
                    {
                        // 登录失败或取消登录，退出应用
                        MessageBox.Show("登录失败，应用程序将退出。", "登录失败", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"应用程序启动失败: {ex.Message}\n\n详细错误信息:\n{ex.InnerException?.Message}", "启动错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
