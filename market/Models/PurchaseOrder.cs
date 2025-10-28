using System;
using System.Collections.Generic;

namespace market.Models
{
    /// <summary>
    /// 进货单模型
    /// </summary>
    public class PurchaseOrder
    {
        public string OrderNumber { get; set; } // 进货单号
        public DateTime OrderDate { get; set; } // 进货日期
        public string SupplierId { get; set; } // 供应商ID
        public string SupplierName { get; set; } // 供应商名称（显示用）
        public string OperatorId { get; set; } // 操作人ID
        public string OperatorName { get; set; } // 操作人姓名（显示用）
        public PurchaseOrderStatus Status { get; set; } // 订单状态
        public decimal TotalAmount { get; set; } // 总金额
        public decimal TaxAmount { get; set; } // 税额
        public decimal FinalAmount { get; set; } // 最终金额
        public string Notes { get; set; } // 备注
        public DateTime CreatedAt { get; set; } // 创建时间
        public DateTime? UpdatedAt { get; set; } // 更新时间
        public DateTime? CompletedAt { get; set; } // 完成时间
        
        /// <summary>
        /// 进货明细
        /// </summary>
        public List<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();

        /// <summary>
        /// 计算总金额
        /// </summary>
        public void CalculateTotalAmount()
        {
            TotalAmount = 0;
            foreach (var item in Items)
            {
                item.Amount = item.Quantity * item.PurchasePrice;
                TotalAmount += item.Amount;
            }
            
            // 计算最终金额（含税）
            FinalAmount = TotalAmount + TaxAmount;
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
                    case PurchaseOrderStatus.Pending:
                        return "待审核";
                    case PurchaseOrderStatus.Approved:
                        return "已审核";
                    case PurchaseOrderStatus.Delivered:
                        return "已到货";
                    case PurchaseOrderStatus.Completed:
                        return "已完成";
                    case PurchaseOrderStatus.Cancelled:
                        return "已取消";
                    default:
                        return "未知";
                }
            }
        }
    }

    /// <summary>
    /// 进货单明细模型
    /// </summary>
    public class PurchaseOrderItem
    {
        public string Id { get; set; } // 明细ID
        public string OrderNumber { get; set; } // 进货单号
        public string ProductCode { get; set; } // 商品编码
        public string ProductName { get; set; } // 商品名称
        public int Quantity { get; set; } // 进货数量
        public decimal PurchasePrice { get; set; } // 进货单价
        public decimal Amount { get; set; } // 金额
        public DateTime? ExpiryDate { get; set; } // 保质期
        public string BatchNumber { get; set; } // 批次号
        public string Notes { get; set; } // 备注

        /// <summary>
        /// 计算金额
        /// </summary>
        public void CalculateAmount()
        {
            Amount = Quantity * PurchasePrice;
        }
    }

    /// <summary>
    /// 进货单状态
    /// </summary>
    public enum PurchaseOrderStatus
    {
        /// <summary>
        /// 待审核
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// 已审核
        /// </summary>
        Approved = 1,
        
        /// <summary>
        /// 已到货
        /// </summary>
        Delivered = 2,
        
        /// <summary>
        /// 已完成
        /// </summary>
        Completed = 3,
        
        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled = 4
    }

    /// <summary>
    /// 进货单查询条件
    /// </summary>
    public class PurchaseOrderQuery
    {
        public string OrderNumber { get; set; }
        public string SupplierId { get; set; }
        public PurchaseOrderStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ProductCode { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 进货单分页结果
    /// </summary>
    public class PurchaseOrderListResult
    {
        public List<PurchaseOrder> Orders { get; set; } = new List<PurchaseOrder>();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

    /// <summary>
    /// 进货统计模型
    /// </summary>
    public class PurchaseStatistics
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalAmount { get; set; }
        public int ProductCount { get; set; }
    }
}