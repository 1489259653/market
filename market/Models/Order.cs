using System;
using System.Collections.Generic;

namespace market.Models
{
    /// <summary>
    /// 销售订单模型
    /// </summary>
    public class Order
    {
        public string OrderNumber { get; set; } // 订单号
        public DateTime OrderDate { get; set; } // 销售时间
        public List<OrderItem> Items { get; set; } = new List<OrderItem>(); // 商品明细
        public decimal TotalAmount { get; set; } // 总金额
        public string PaymentMethod { get; set; } // 付款方式
        public string CashierId { get; set; } // 收银员ID
    }

    /// <summary>
    /// 订单明细模型
    /// </summary>
    public class OrderItem
    {
        public string ProductCode { get; set; } // 商品编码
        public string ProductName { get; set; } // 商品名称
        public int Quantity { get; set; } // 销售数量
        public decimal Price { get; set; } // 单价
        public decimal Amount { get; set; } // 金额
    }
}