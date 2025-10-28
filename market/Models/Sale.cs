using System;
using System.Collections.Generic;

namespace market.Models
{
    /// <summary>
    /// 销售订单模型
    /// </summary>
    public class SaleOrder
    {
        public string OrderNumber { get; set; } // 销售单号
        public DateTime OrderDate { get; set; } // 销售日期
        public string Customer { get; set; } // 顾客姓名
        public string OperatorId { get; set; } // 操作人ID
        public string OperatorName { get; set; } // 操作人姓名
        public SaleOrderStatus Status { get; set; } // 订单状态
        public decimal TotalAmount { get; set; } // 总金额
        public decimal DiscountAmount { get; set; } // 折扣金额
        public decimal FinalAmount { get; set; } // 最终金额
        public decimal ReceivedAmount { get; set; } // 实收金额
        public decimal ChangeAmount { get; set; } // 找零金额
        public PaymentMethod PaymentMethod { get; set; } // 支付方式
        public string Notes { get; set; } // 备注
        public DateTime CreatedAt { get; set; } // 创建时间

        /// <summary>
        /// 销售明细
        /// </summary>
        public List<SaleOrderItem> Items { get; set; } = new List<SaleOrderItem>();

        /// <summary>
        /// 计算总金额
        /// </summary>
        public void CalculateAmounts()
        {
            TotalAmount = 0;
            foreach (var item in Items)
            {
                item.Amount = item.Quantity * item.SalePrice;
                TotalAmount += item.Amount;
            }
            
            // 计算最终金额
            FinalAmount = TotalAmount - DiscountAmount;
            
            // 计算找零
            if (ReceivedAmount > 0)
            {
                ChangeAmount = ReceivedAmount - FinalAmount;
            }
        }

        /// <summary>
        /// 获取订单状态描述
        /// </summary>
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case SaleOrderStatus.Pending:
                        return "待支付";
                    case SaleOrderStatus.Paid:
                        return "已支付";
                    case SaleOrderStatus.Completed:
                        return "已完成";
                    case SaleOrderStatus.Cancelled:
                        return "已取消";
                    default:
                        return "未知";
                }
            }
        }

        /// <summary>
        /// 获取支付方式描述
        /// </summary>
        public string PaymentMethodText
        {
            get
            {
                switch (PaymentMethod)
                {
                    case PaymentMethod.Cash:
                        return "现金";
                    case PaymentMethod.WeChat:
                        return "微信支付";
                    case PaymentMethod.Alipay:
                        return "支付宝";
                    case PaymentMethod.Card:
                        return "银行卡";
                    default:
                        return "未知";
                }
            }
        }
    }

    /// <summary>
    /// 销售订单明细模型
    /// </summary>
    public class SaleOrderItem
    {
        public string Id { get; set; } // 明细ID
        public string OrderNumber { get; set; } // 销售单号
        public string ProductCode { get; set; } // 商品编码
        public string ProductName { get; set; } // 商品名称
        public int Quantity { get; set; } // 销售数量
        public decimal SalePrice { get; set; } // 销售单价
        public decimal Amount { get; set; } // 金额
        public decimal OriginalPrice { get; set; } // 原价
        public decimal DiscountRate { get; set; } // 折扣率

        /// <summary>
        /// 计算金额
        /// </summary>
        public void CalculateAmount()
        {
            Amount = Quantity * SalePrice;
        }
    }

    /// <summary>
    /// 销售订单状态
    /// </summary>
    public enum SaleOrderStatus
    {
        /// <summary>
        /// 待支付
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// 已支付
        /// </summary>
        Paid = 1,
        
        /// <summary>
        /// 已完成
        /// </summary>
        Completed = 2,
        
        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled = 3
    }

    /// <summary>
    /// 支付方式
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// 现金
        /// </summary>
        Cash = 0,
        
        /// <summary>
        /// 微信支付
        /// </summary>
        WeChat = 1,
        
        /// <summary>
        /// 支付宝
        /// </summary>
        Alipay = 2,
        
        /// <summary>
        /// 银行卡
        /// </summary>
        Card = 3
    }

    /// <summary>
    /// 销售查询条件
    /// </summary>
    public class SaleOrderQuery
    {
        public string OrderNumber { get; set; }
        public string Customer { get; set; }
        public SaleOrderStatus? Status { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ProductCode { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 销售分页结果
    /// </summary>
    public class SaleOrderListResult
    {
        public List<SaleOrder> Orders { get; set; } = new List<SaleOrder>();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

    /// <summary>
    /// 销售统计模型
    /// </summary>
    public class SaleStatistics
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public int ProductCount { get; set; }
        public Dictionary<PaymentMethod, decimal> PaymentMethodDistribution { get; set; } = new Dictionary<PaymentMethod, decimal>();
    }

    /// <summary>
    /// 收银临时商品项
    /// </summary>
    public class CartItem
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal SalePrice { get; set; }
        public decimal Amount { get; set; }
        public bool IsWeight { get; set; } // 是否为称重商品

        /// <summary>
        /// 计算金额
        /// </summary>
        public void CalculateAmount()
        {
            Amount = Quantity * SalePrice;
        }
    }
}