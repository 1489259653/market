namespace market.Models
{
    /// <summary>
    /// 会员信息模型
    /// </summary>
    public class Member
    {
        public string Id { get; set; } // 会员ID
        public string Name { get; set; } // 会员姓名
        public string PhoneNumber { get; set; } // 手机号码
        public string Email { get; set; } // 电子邮箱
        public DateTime RegistrationDate { get; set; } // 注册时间
        public decimal Points { get; set; } // 积分
        public MemberLevel Level { get; set; } // 会员等级
        public decimal Discount { get; set; } // 折扣率（例如0.98表示98折）
        public decimal TotalSpending { get; set; } // 累积消费金额
    }

    /// <summary>
    /// 会员等级枚举
    /// </summary>
    public enum MemberLevel
    {
        Bronze,   // 铜牌会员
        Silver,   // 银牌会员
        Gold,     // 金牌会员
        Platinum  // 铂金会员
    }
}