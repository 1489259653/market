using System;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;
using market.Models;

namespace market.Services
{
    /// <summary>
    /// 身份验证服务类
    /// </summary>
    public class AuthService
    {
        private readonly DatabaseService _databaseService;
        private User _currentUser;

        public AuthService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// 当前登录用户
        /// </summary>
        public User CurrentUser => _currentUser;

        /// <summary>
        /// 用户是否已登录
        /// </summary>
        public bool IsLoggedIn => _currentUser != null;

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>登录是否成功</returns>
        public bool Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;

            var passwordHash = ComputeMD5Hash(password);

            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = "SELECT Id, Username, PasswordHash, Role FROM Users WHERE Username = @Username AND PasswordHash = @PasswordHash";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            _currentUser = new User
                            {
                                Id = reader.GetString(0),
                                Username = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                Role = (UserRole)reader.GetInt32(3)
                            };
                            
                            // 记录登录日志
                            LogOperation("登录", $"用户 {username} 登录系统");
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 用户登出
        /// </summary>
        public void Logout()
        {
            if (_currentUser != null)
            {
                LogOperation("登出", $"用户 {_currentUser.Username} 退出系统");
                _currentUser = null;
            }
        }

        /// <summary>
        /// 检查用户是否有权限访问指定功能
        /// </summary>
        public bool HasPermission(string operation)
        {
            if (_currentUser == null) return false;

            switch (_currentUser.Role)
            {
                case UserRole.Administrator:
                    return true; // 管理员拥有所有权限
                case UserRole.Cashier:
                    return operation switch
                    {
                        "销售" or "查询库存" or "查看销售记录" => true,
                        _ => false
                    };
                case UserRole.WarehouseManager:
                    return operation switch
                    {
                        "进货" or "库存管理" or "库存预警查看" or "进货管理" or "供应商管理" => true,
                        _ => false
                    };
                default:
                    return false;
            }
        }

        /// <summary>
        /// 计算MD5哈希值
        /// </summary>
        private string ComputeMD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// 记录操作日志
        /// </summary>
        private void LogOperation(string operationType, string details)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = @"
                        INSERT INTO OperationLogs (OperationType, UserId, OperationTime, Details) 
                        VALUES (@OperationType, @UserId, @OperationTime, @Details)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OperationType", operationType);
                        command.Parameters.AddWithValue("@UserId", _currentUser?.Id ?? "system");
                        command.Parameters.AddWithValue("@OperationTime", DateTime.Now);
                        command.Parameters.AddWithValue("@Details", details);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // 日志记录失败不影响主要功能
                System.Diagnostics.Debug.WriteLine($"日志记录失败: {ex.Message}");
            }
        }
    }
}