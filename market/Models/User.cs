namespace market.Models
{
    /// <summary>
    /// 用户信息模型
    /// </summary>
    public class User
    {
        public string Id { get; set; } // 用户ID
        public string Username { get; set; } // 用户名
        public string PasswordHash { get; set; } // 密码哈希（MD5）
        public UserRole Role { get; set; } // 角色
    }

    /// <summary>
    /// 用户角色枚举
    /// </summary>
    public enum UserRole
    {
        Administrator, // 管理员
        Cashier,       // 收银员
        WarehouseManager // 仓库管理员
    }
}