namespace market.Models
{
    /// <summary>
    /// 供货方信息模型
    /// </summary>
    public class Supplier
    {
        public string Id { get; set; } // 供货方ID
        public string Name { get; set; } // 名称
        public string ProductionLocation { get; set; } // 生产地
        public string ContactInfo { get; set; } // 联系方式
        public string BusinessLicense { get; set; } // 营业执照编号（可选）
    }
}