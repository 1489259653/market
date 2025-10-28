using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using market.Models;

namespace market.Services
{
    /// <summary>
    /// 库存管理服务
    /// </summary>
    public class InventoryService
    {
        private readonly DatabaseService _databaseService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="databaseService">数据库服务实例</param>
        public InventoryService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// 进货操作
        /// </summary>
        /// <param name="productCode">商品编码</param>
        /// <param name="quantity">进货数量</param>
        /// <param name="purchasePrice">进货单价</param>
        /// <param name="operatorId">操作人ID</param>
        /// <param name="supplierId">供应商ID</param>
        /// <returns>是否成功</returns>
        public bool PurchaseProduct(string productCode, int quantity, decimal purchasePrice, string operatorId, string supplierId = null)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // 更新商品库存
                            string updateQuery = "UPDATE Products SET Quantity = Quantity + @Quantity, PurchasePrice = @PurchasePrice WHERE ProductCode = @ProductCode";
                            using (var updateCommand = new MySql.Data.MySqlClient.MySqlCommand(updateQuery, connection, transaction))
                            {
                                updateCommand.Parameters.AddWithValue("@ProductCode", productCode);
                                updateCommand.Parameters.AddWithValue("@Quantity", quantity);
                                updateCommand.Parameters.AddWithValue("@PurchasePrice", purchasePrice);
                                updateCommand.ExecuteNonQuery();
                            }

                            // 记录库存变动历史
                            string historyQuery = @"
                                INSERT INTO InventoryHistory (Id, ProductCode, QuantityChange, OperationType, OperationDate, OperatorId, PurchasePrice)
                                VALUES (@Id, @ProductCode, @QuantityChange, '进货', @OperationDate, @OperatorId, @PurchasePrice)";
                            using (var historyCommand = new MySql.Data.MySqlClient.MySqlCommand(historyQuery, connection, transaction))
                            {
                                historyCommand.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                                historyCommand.Parameters.AddWithValue("@ProductCode", productCode);
                                historyCommand.Parameters.AddWithValue("@QuantityChange", quantity);
                                historyCommand.Parameters.AddWithValue("@OperationDate", DateTime.Now);
                                historyCommand.Parameters.AddWithValue("@OperatorId", operatorId);
                                historyCommand.Parameters.AddWithValue("@PurchasePrice", purchasePrice);
                                historyCommand.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"进货操作失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取库存变动历史记录
        /// </summary>
        /// <param name="productCode">商品编码（可选）</param>
        /// <param name="startDate">开始日期（可选）</param>
        /// <param name="endDate">结束日期（可选）</param>
        /// <param name="operationType">操作类型（可选）</param>
        /// <returns>库存变动历史记录列表</returns>
        public List<InventoryHistory> GetInventoryHistory(string productCode = null, DateTime? startDate = null, DateTime? endDate = null, string operationType = null)
        {
            var historyList = new List<InventoryHistory>();

            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = @"
                        SELECT ih.*, p.Name as ProductName, u.Username as OperatorName
                        FROM InventoryHistory ih
                        LEFT JOIN Products p ON ih.ProductCode = p.ProductCode
                        LEFT JOIN Users u ON ih.OperatorId = u.Id
                        WHERE 1=1";

                    using (var command = new MySql.Data.MySqlClient.MySqlCommand(query, connection))
                    {
                        // 添加查询条件
                        if (!string.IsNullOrEmpty(productCode))
                        {
                            query += " AND ih.ProductCode = @ProductCode";
                            command.Parameters.AddWithValue("@ProductCode", productCode);
                        }

                        if (startDate.HasValue)
                        {
                            query += " AND ih.OperationDate >= @StartDate";
                            command.Parameters.AddWithValue("@StartDate", startDate.Value);
                        }

                        if (endDate.HasValue)
                        {
                            query += " AND ih.OperationDate <= @EndDate";
                            command.Parameters.AddWithValue("@EndDate", endDate.Value);
                        }

                        if (!string.IsNullOrEmpty(operationType))
                        {
                            query += " AND ih.OperationType = @OperationType";
                            command.Parameters.AddWithValue("@OperationType", operationType);
                        }

                        query += " ORDER BY ih.OperationDate DESC";
                        command.CommandText = query;

                        using (var reader = command.ExecuteReader())
                        {
                            // 调试：检查返回的列名
                            var schemaTable = reader.GetSchemaTable();
                            if (schemaTable != null)
                            {
                                foreach (DataRow row in schemaTable.Rows)
                                {
                                    Console.WriteLine($"列名: {row["ColumnName"]}");
                                }
                            }
                            
                            while (reader.Read())
                            {
                                historyList.Add(new InventoryHistory
                                {
                                    Id = reader.GetString("Id"),
                                    ProductCode = reader.GetString("ProductCode"),
                                    QuantityChange = reader.GetInt32("QuantityChange"),
                                    OperationType = reader.GetString("OperationType"),
                                    OperationDate = reader.GetDateTime("OperationDate"),
                                    OperatorId = reader.GetString("OperatorId"),
                                    OrderNumber = reader.IsDBNull("OrderNumber") ? null : reader.GetString("OrderNumber"),
                                    PurchasePrice = reader.IsDBNull("PurchasePrice") ? 0 : reader.GetDecimal("PurchasePrice")
                                });
                            }
                        }
                    }
                }

                return historyList;
            }
            catch (Exception ex)
            {
                throw new Exception($"获取库存历史记录失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取商品当前库存信息
        /// </summary>
        /// <param name="productCode">商品编码</param>
        /// <returns>商品信息</returns>
        public Product GetProductStockInfo(string productCode)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = @"
                        SELECT p.*, s.Name as SupplierName
                        FROM Products p
                        LEFT JOIN Suppliers s ON p.SupplierId = s.Id
                        WHERE p.ProductCode = @ProductCode";

                    using (var command = new MySql.Data.MySqlClient.MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductCode", productCode);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Product
                                {
                                    ProductCode = reader.GetString("ProductCode"),
                                    Name = reader.GetString("Name"),
                                    Price = reader.GetDecimal("Price"),
                                    Quantity = reader.GetInt32("Quantity"),
                                    Unit = reader.GetString("Unit"),
                                    Category = reader.GetString("Category"),
                                    ExpiryDate = reader.IsDBNull("ExpiryDate") ? null : (DateTime?)reader.GetDateTime("ExpiryDate"),
                                    StockAlertThreshold = reader.GetInt32("StockAlertThreshold"),
                                    PurchasePrice = reader.IsDBNull("PurchasePrice") ? 0 : reader.GetDecimal("PurchasePrice"),
                                    SupplierId = reader.IsDBNull("SupplierId") ? null : reader.GetString("SupplierId"),
                                    SupplierName = reader.IsDBNull("SupplierName") ? null : reader.GetString("SupplierName")
                                };
                            }
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取商品库存信息失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 按分类统计库存
        /// </summary>
        /// <returns>分类库存统计列表</returns>
        public List<CategoryStockSummary> GetCategoryStockSummary()
        {
            var summaryList = new List<CategoryStockSummary>();

            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = @"
                        SELECT 
                            Category,
                            COUNT(*) as ProductCount,
                            SUM(Quantity) as TotalQuantity,
                            SUM(Quantity * Price) as TotalValue
                        FROM Products
                        GROUP BY Category
                        ORDER BY Category";

                    using (var command = new MySql.Data.MySqlClient.MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                summaryList.Add(new CategoryStockSummary
                                {
                                    Category = reader.GetString("Category"),
                                    ProductCount = reader.GetInt32("ProductCount"),
                                    TotalQuantity = reader.GetInt32("TotalQuantity"),
                                    TotalValue = reader.GetDecimal("TotalValue")
                                });
                            }
                        }
                    }
                }

                return summaryList;
            }
            catch (Exception ex)
            {
                throw new Exception($"获取分类库存统计失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 检查库存是否充足
        /// </summary>
        /// <param name="productCode">商品编码</param>
        /// <param name="requiredQuantity">需要的数量</param>
        /// <returns>库存是否充足</returns>
        public bool IsStockSufficient(string productCode, int requiredQuantity)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = "SELECT Quantity FROM Products WHERE ProductCode = @ProductCode";

                    using (var command = new MySql.Data.MySqlClient.MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductCode", productCode);
                        var quantity = (int?)command.ExecuteScalar();
                        return quantity.HasValue && quantity.Value >= requiredQuantity;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"检查库存失败: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// 分类库存统计模型
    /// </summary>
    public class CategoryStockSummary
    {
        public string Category { get; set; }
        public int ProductCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }
}