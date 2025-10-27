using System;

namespace market.Models
{
    /// <summary>
    /// 库存变动历史记录模型
    /// </summary>
    public class InventoryHistory
    {
        public string Id { get; set; } // 记录ID
        public string ProductCode { get; set; } // 商品编码
        public int QuantityChange { get; set; } // 数量变动（+/-）
        public string OperationType { get; set; } // 操作类型：进货、销售、退货
        public DateTime OperationDate { get; set; } // 操作时间
        public string OperatorId { get; set; } // 操作人ID
        public string OrderNumber { get; set; } // 关联订单号（用于销售和退货）
        public decimal PurchasePrice { get; set; } // 进货价（仅进货时使用）
    }
}