using System;
using System.Collections.Generic;

namespace market.Models
{
    /// <summary>
    /// 库存警报级别
    /// </summary>
    public enum StockAlertLevel
    {
        /// <summary>
        /// 正常库存
        /// </summary>
        Normal,
        /// <summary>
        /// 库存偏低
        /// </summary>
        Low,
        /// <summary>
        /// 库存严重不足
        /// </summary>
        Critical,
        /// <summary>
        /// 库存为零
        /// </summary>
        OutOfStock
    }

    /// <summary>
    /// 商品信息模型
    /// </summary>
    public class Product
    {
        public string ProductCode { get; set; } // 商品编码
        public string Name { get; set; } // 商品名称
        public decimal Price { get; set; } // 单价
        public int Quantity { get; set; } // 数量
        public string Unit { get; set; } // 销售单位
        public string Category { get; set; } // 分类
        public DateTime? ExpiryDate { get; set; } // 保质期
        public int StockAlertThreshold { get; set; } = 10; // 库存下限
        public string SupplierId { get; set; } // 供货方ID
        public string SupplierName { get; set; } // 供货方名称（显示用）
        
        /// <summary>
        /// 商品描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 进货价格
        /// </summary>
        public decimal PurchasePrice { get; set; }

        /// <summary>
        /// 最小起订量
        /// </summary>
        public int MinimumOrderQuantity { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdated { get; set; }
        
        // 计算属性
        public bool IsStockAlert => Quantity <= StockAlertThreshold;
        public decimal TotalAmount => Price * Quantity;
        public string StatusText => IsStockAlert ? "库存预警" : "正常";
        
        /// <summary>
        /// 是否低于库存警报阈值（与IsStockAlert功能相同，为了向后兼容）
        /// </summary>
        public bool IsStockLow => IsStockAlert;

        /// <summary>
        /// 是否过期
        /// </summary>
        public bool IsExpired => ExpiryDate.HasValue && DateTime.Now > ExpiryDate.Value;

        /// <summary>
        /// 是否即将过期（30天内）
        /// </summary>
        public bool IsExpiringSoon => ExpiryDate.HasValue && DateTime.Now.AddDays(30) > ExpiryDate.Value && !IsExpired;

        /// <summary>
        /// 库存警报级别
        /// </summary>
        public StockAlertLevel AlertLevel
        {
            get
            {
                if (Quantity == 0)
                    return StockAlertLevel.OutOfStock;
                else if (Quantity <= StockAlertThreshold * 0.5)
                    return StockAlertLevel.Critical;
                else if (IsStockLow)
                    return StockAlertLevel.Low;
                else
                    return StockAlertLevel.Normal;
            }
        }

        /// <summary>
        /// 获取库存状态描述
        /// </summary>
        public string StockStatusText
        {
            get
            {
                switch (AlertLevel)
                {
                    case StockAlertLevel.OutOfStock:
                        return "缺货";
                    case StockAlertLevel.Critical:
                        return "库存严重不足";
                    case StockAlertLevel.Low:
                        return "库存偏低";
                    default:
                        return "正常";
                }
            }
        }

        /// <summary>
        /// 距离过期天数（如果有过期日期）
        /// </summary>
        public int? DaysUntilExpiry
        {
            get
            {
                if (!ExpiryDate.HasValue)
                    return null;
                return (ExpiryDate.Value - DateTime.Now).Days;
            }
        }

        /// <summary>
        /// 获取过期状态描述
        /// </summary>
        public string ExpiryStatusText
        {
            get
            {
                if (!ExpiryDate.HasValue)
                    return "无过期日期";
                else if (IsExpired)
                    return $"已过期 {Math.Abs(DaysUntilExpiry.Value)} 天";
                else if (IsExpiringSoon)
                    return $"即将过期（剩余 {DaysUntilExpiry.Value} 天）";
                else
                    return $"正常（剩余 {DaysUntilExpiry.Value} 天）";
            }
        }

        /// <summary>
        /// 计算毛利率
        /// </summary>
        public decimal ProfitMargin
        {
            get
            {
                if (PurchasePrice <= 0) return 0;
                return (Price - PurchasePrice) / PurchasePrice * 100;
            }
        }
    }

    /// <summary>
    /// 商品列表分页结果
    /// </summary>
    public class ProductListResult
    {
        /// <summary>
        /// 商品列表
        /// </summary>
        public List<Product> Products { get; set; }
        
        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }
        
        /// <summary>
        /// 当前页码
        /// </summary>
        public int CurrentPage { get; set; }
        
        /// <summary>
        /// 每页记录数
        /// </summary>
        public int PageSize { get; set; }
    }
}