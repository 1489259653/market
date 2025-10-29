using System;
using System.Collections.Generic;

namespace market.Models
{
    /// <summary>
    /// 退货订单模型
    /// </summary>
    public class ReturnOrder
    {
        public string ReturnNumber { get; set; } // 退货单号
        public string OriginalOrderNumber { get; set; } // 原销售单号
        public DateTime ReturnDate { get; set; } // 退货日期
        public string Customer { get; set; } // 顾客姓名
        public string OperatorId { get; set; } // 操作人ID
        public string OperatorName { get; set; } // 操作人姓名
        public ReturnOrderStatus Status { get; set; } // 退货状态
        public decimal TotalAmount { get; set; } // 退货总金额
        public decimal RefundAmount { get; set; } // 退款金额
        public string Reason { get; set; } // 退货原因
        public string Notes { get; set; } // 备注
        public DateTime CreatedAt { get; set; } // 创建时间

        /// <summary>
        /// 退货明细
        /// </summary>
        public List<ReturnOrderItem> Items { get; set; } = new List<ReturnOrderItem>();

        /// <summary>
        /// 计算退货金额
        /// </summary>
        public void CalculateAmounts()
        {
            TotalAmount = 0;
            foreach (var item in Items)
            {
                item.Amount = item.Quantity * item.ReturnPrice;
                TotalAmount += item.Amount;
            }
            
            // 退款金额等于退货总金额
            RefundAmount = TotalAmount;
        }

        /// <summary>
        /// 获取退货状态描述
        /// </summary>
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case ReturnOrderStatus.Pending:
                        return "待处理";
                    case ReturnOrderStatus.Approved:
                        return "已审核";
                    case ReturnOrderStatus.Completed:
                        return "已完成";
                    case ReturnOrderStatus.Cancelled:
                        return "已取消";
                    default:
                        return "未知";
                }
            }
        }
    }

    /// <summary>
    /// 退货订单明细模型
    /// </summary>
    public class ReturnOrderItem
    {
        public string Id { get; set; } // 明细ID
        public string ReturnNumber { get; set; } // 退货单号
        public string ProductCode { get; set; } // 商品编码
        public string ProductName { get; set; } // 商品名称
        public int Quantity { get; set; } // 退货数量
        public decimal ReturnPrice { get; set; } // 退货单价
        public decimal Amount { get; set; } // 退货金额
        public decimal OriginalSalePrice { get; set; } // 原销售单价
        public string Reason { get; set; } // 退货原因（商品级别）

        /// <summary>
        /// 计算金额
        /// </summary>
        public void CalculateAmount()
        {
            Amount = Quantity * ReturnPrice;
        }
    }

    /// <summary>
    /// 退货订单状态
    /// </summary>
    public enum ReturnOrderStatus
    {
        /// <summary>
        /// 待处理
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// 已审核
        /// </summary>
        Approved = 1,
        
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
    /// 退货查询条件
    /// </summary>
    public class ReturnOrderQuery
    {
        public string ReturnNumber { get; set; }
        public string OriginalOrderNumber { get; set; }
        public string Customer { get; set; }
        public ReturnOrderStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ProductCode { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 退货分页结果
    /// </summary>
    public class ReturnOrderListResult
    {
        public List<ReturnOrder> Orders { get; set; } = new List<ReturnOrder>();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

    /// <summary>
    /// 退货统计模型
    /// </summary>
    public class ReturnStatistics
    {
        public DateTime Date { get; set; }
        public int ReturnCount { get; set; }
        public decimal TotalReturnAmount { get; set; }
        public decimal AverageReturnAmount { get; set; }
        public int ProductCount { get; set; }
        public Dictionary<string, int> ReasonDistribution { get; set; } = new Dictionary<string, int>();
    }
}