using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using market.Models;

namespace market.Services
{
    /// <summary>
    /// 进货管理服务类
    /// </summary>
    public class PurchaseService
    {
        private readonly DatabaseService _databaseService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="databaseService">数据库服务实例</param>
        public PurchaseService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// 创建进货单
        /// </summary>
        /// <param name="order">进货单信息</param>
        /// <returns>是否成功</returns>
        public bool CreatePurchaseOrder(PurchaseOrder order)
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
                            // 生成进货单号
                            if (string.IsNullOrEmpty(order.OrderNumber))
                            {
                                order.OrderNumber = GeneratePurchaseOrderNumber();
                            }

                            // 插入进货单主表
                            string orderQuery = @"
                                INSERT INTO PurchaseOrders (
                                    OrderNumber, OrderDate, SupplierId, OperatorId, 
                                    Status, TotalAmount, TaxAmount, FinalAmount, Notes,
                                    CreatedAt, UpdatedAt
                                ) VALUES (
                                    @OrderNumber, @OrderDate, @SupplierId, @OperatorId,
                                    @Status, @TotalAmount, @TaxAmount, @FinalAmount, @Notes,
                                    @CreatedAt, @UpdatedAt
                                )";

                            using (var orderCommand = new MySqlCommand(orderQuery, connection, transaction))
                            {
                                orderCommand.Parameters.AddWithValue("@OrderNumber", order.OrderNumber);
                                orderCommand.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                                orderCommand.Parameters.AddWithValue("@SupplierId", order.SupplierId);
                                orderCommand.Parameters.AddWithValue("@OperatorId", order.OperatorId);
                                orderCommand.Parameters.AddWithValue("@Status", (int)order.Status);
                                orderCommand.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                                orderCommand.Parameters.AddWithValue("@TaxAmount", order.TaxAmount);
                                orderCommand.Parameters.AddWithValue("@FinalAmount", order.FinalAmount);
                                orderCommand.Parameters.AddWithValue("@Notes", order.Notes ?? string.Empty);
                                orderCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                                orderCommand.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                                orderCommand.ExecuteNonQuery();
                            }

                            // 插入进货明细
                            foreach (var item in order.Items)
                            {
                                string itemQuery = @"
                                    INSERT INTO PurchaseOrderItems (
                                        Id, OrderNumber, ProductCode, ProductName,
                                        Quantity, PurchasePrice, Amount, ExpiryDate, BatchNumber, Notes
                                    ) VALUES (
                                        @Id, @OrderNumber, @ProductCode, @ProductName,
                                        @Quantity, @PurchasePrice, @Amount, @ExpiryDate, @BatchNumber, @Notes
                                    )";

                                using (var itemCommand = new MySqlCommand(itemQuery, connection, transaction))
                                {
                                    itemCommand.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                                    itemCommand.Parameters.AddWithValue("@OrderNumber", order.OrderNumber);
                                    itemCommand.Parameters.AddWithValue("@ProductCode", item.ProductCode);
                                    itemCommand.Parameters.AddWithValue("@ProductName", item.ProductName);
                                    itemCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    itemCommand.Parameters.AddWithValue("@PurchasePrice", item.PurchasePrice);
                                    itemCommand.Parameters.AddWithValue("@Amount", item.Amount);
                                    itemCommand.Parameters.AddWithValue("@ExpiryDate", item.ExpiryDate.HasValue ? item.ExpiryDate.Value : DBNull.Value);
                                    itemCommand.Parameters.AddWithValue("@BatchNumber", item.BatchNumber ?? string.Empty);
                                    itemCommand.Parameters.AddWithValue("@Notes", item.Notes ?? string.Empty);

                                    itemCommand.ExecuteNonQuery();
                                }
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
                throw new Exception($"创建进货单失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 更新进货单状态
        /// </summary>
        /// <param name="orderNumber">进货单号</param>
        /// <param name="status">新状态</param>
        /// <param name="operatorId">操作人ID</param>
        /// <returns>是否成功</returns>
        public bool UpdatePurchaseOrderStatus(string orderNumber, PurchaseOrderStatus status, string operatorId)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = @"
                        UPDATE PurchaseOrders 
                        SET Status = @Status, UpdatedAt = @UpdatedAt, OperatorId = @OperatorId
                        WHERE OrderNumber = @OrderNumber";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Status", (int)status);
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                        command.Parameters.AddWithValue("@OperatorId", operatorId);
                        command.Parameters.AddWithValue("@OrderNumber", orderNumber);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"更新进货单状态失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 检查商品是否存在
        /// </summary>
        /// <param name="productCode">商品编码</param>
        /// <returns>是否存在</returns>
        public bool ProductExists(string productCode)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Products WHERE ProductCode = @ProductCode";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductCode", productCode);
                        return Convert.ToInt32(command.ExecuteScalar()) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"检查商品是否存在失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 智能处理进货商品 - 存在则更新库存，不存在则新增商品
        /// </summary>
        /// <param name="item">进货明细</param>
        /// <param name="supplierId">供应商ID</param>
        /// <param name="operatorId">操作人ID</param>
        /// <returns>是否成功</returns>
        public bool ProcessPurchaseItem(PurchaseOrderItem item, string supplierId, string operatorId)
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
                            if (ProductExists(item.ProductCode))
                            {
                                // 商品已存在，更新库存和进货价格
                                string updateQuery = @"
                                    UPDATE Products 
                                    SET Quantity = Quantity + @Quantity, 
                                        PurchasePrice = @PurchasePrice,
                                        LastUpdated = @LastUpdated
                                    WHERE ProductCode = @ProductCode";

                                using (var command = new MySqlCommand(updateQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    command.Parameters.AddWithValue("@PurchasePrice", item.PurchasePrice);
                                    command.Parameters.AddWithValue("@LastUpdated", DateTime.Now);
                                    command.Parameters.AddWithValue("@ProductCode", item.ProductCode);
                                    command.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                // 商品不存在，新增商品信息
                                string insertQuery = @"
                                    INSERT INTO Products (
                                        ProductCode, Name, Price, Quantity, Unit, 
                                        Category, ExpiryDate, StockAlertThreshold, 
                                        SupplierId, PurchasePrice, LastUpdated
                                    ) VALUES (
                                        @ProductCode, @Name, @Price, @Quantity, @Unit,
                                        @Category, @ExpiryDate, @StockAlertThreshold,
                                        @SupplierId, @PurchasePrice, @LastUpdated
                                    )";

                                using (var command = new MySqlCommand(insertQuery, connection, transaction))
                                {
                                    // 设置默认值
                                    var defaultCategory = "未分类";
                                    var defaultUnit = "个";
                                    var defaultStockAlertThreshold = 10;
                                    var defaultPrice = item.PurchasePrice * 1.2m; // 默认售价为进货价的1.2倍

                                    command.Parameters.AddWithValue("@ProductCode", item.ProductCode);
                                    command.Parameters.AddWithValue("@Name", item.ProductName);
                                    command.Parameters.AddWithValue("@Price", defaultPrice);
                                    command.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    command.Parameters.AddWithValue("@Unit", defaultUnit);
                                    command.Parameters.AddWithValue("@Category", defaultCategory);
                                    command.Parameters.AddWithValue("@ExpiryDate", item.ExpiryDate.HasValue ? item.ExpiryDate.Value : DBNull.Value);
                                    command.Parameters.AddWithValue("@StockAlertThreshold", defaultStockAlertThreshold);
                                    command.Parameters.AddWithValue("@SupplierId", supplierId);
                                    command.Parameters.AddWithValue("@PurchasePrice", item.PurchasePrice);
                                    command.Parameters.AddWithValue("@LastUpdated", DateTime.Now);
                                    command.ExecuteNonQuery();
                                }
                            }

                            // 记录库存变动历史
                            string historyQuery = @"
                                INSERT INTO InventoryHistory (
                                    Id, ProductCode, QuantityChange, OperationType, 
                                    OperationDate, OperatorId, PurchasePrice
                                ) VALUES (
                                    @Id, @ProductCode, @QuantityChange, '进货',
                                    @OperationDate, @OperatorId, @PurchasePrice
                                )";

                            using (var historyCommand = new MySqlCommand(historyQuery, connection, transaction))
                            {
                                historyCommand.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                                historyCommand.Parameters.AddWithValue("@ProductCode", item.ProductCode);
                                historyCommand.Parameters.AddWithValue("@QuantityChange", item.Quantity);
                                historyCommand.Parameters.AddWithValue("@OperationDate", DateTime.Now);
                                historyCommand.Parameters.AddWithValue("@OperatorId", operatorId);
                                historyCommand.Parameters.AddWithValue("@PurchasePrice", item.PurchasePrice);
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
                throw new Exception($"处理进货商品失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 完成进货单（智能更新库存）
        /// </summary>
        /// <param name="orderNumber">进货单号</param>
        /// <param name="operatorId">操作人ID</param>
        /// <returns>是否成功</returns>
        public bool CompletePurchaseOrder(string orderNumber, string operatorId)
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
                            // 获取进货单详情
                            var order = GetPurchaseOrder(orderNumber);
                            if (order == null)
                            {
                                throw new Exception("进货单不存在");
                            }

                            // 智能处理每个进货商品
                            foreach (var item in order.Items)
                            {
                                if (!ProcessPurchaseItem(item, order.SupplierId, operatorId))
                                {
                                    throw new Exception($"处理商品 {item.ProductName} 失败");
                                }
                            }

                            // 更新进货单状态为已完成
                            string updateOrderQuery = @"
                                UPDATE PurchaseOrders 
                                SET Status = @Status, CompletedAt = @CompletedAt, 
                                    UpdatedAt = @UpdatedAt, OperatorId = @OperatorId
                                WHERE OrderNumber = @OrderNumber";

                            using (var orderCommand = new MySqlCommand(updateOrderQuery, connection, transaction))
                            {
                                orderCommand.Parameters.AddWithValue("@Status", (int)PurchaseOrderStatus.Completed);
                                orderCommand.Parameters.AddWithValue("@CompletedAt", DateTime.Now);
                                orderCommand.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                                orderCommand.Parameters.AddWithValue("@OperatorId", operatorId);
                                orderCommand.Parameters.AddWithValue("@OrderNumber", orderNumber);
                                orderCommand.ExecuteNonQuery();
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
                throw new Exception($"完成进货单失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取进货单详情
        /// </summary>
        /// <param name="orderNumber">进货单号</param>
        /// <returns>进货单信息</returns>
        public PurchaseOrder GetPurchaseOrder(string orderNumber)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT po.*, s.Name as SupplierName, u.Username as OperatorName
                        FROM PurchaseOrders po
                        LEFT JOIN Suppliers s ON po.SupplierId = s.Id
                        LEFT JOIN Users u ON po.OperatorId = u.Id
                        WHERE po.OrderNumber = @OrderNumber";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OrderNumber", orderNumber);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var order = new PurchaseOrder
                                {
                                    OrderNumber = reader.GetString("OrderNumber"),
                                    OrderDate = reader.GetDateTime("OrderDate"),
                                    SupplierId = reader.GetString("SupplierId"),
                                    SupplierName = reader.GetString("SupplierName"),
                                    OperatorId = reader.GetString("OperatorId"),
                                    OperatorName = reader.GetString("OperatorName"),
                                    Status = (PurchaseOrderStatus)reader.GetInt32("Status"),
                                    TotalAmount = reader.GetDecimal("TotalAmount"),
                                    TaxAmount = reader.GetDecimal("TaxAmount"),
                                    FinalAmount = reader.GetDecimal("FinalAmount"),
                                    Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes"),
                                    CreatedAt = reader.GetDateTime("CreatedAt"),
                                    UpdatedAt = reader.IsDBNull("UpdatedAt") ? null : (DateTime?)reader.GetDateTime("UpdatedAt"),
                                    CompletedAt = reader.IsDBNull("CompletedAt") ? null : (DateTime?)reader.GetDateTime("CompletedAt")
                                };

                                // 获取明细
                                order.Items = GetPurchaseOrderItems(orderNumber);
                                
                                return order;
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"获取进货单详情失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取进货单明细
        /// </summary>
        /// <param name="orderNumber">进货单号</param>
        /// <returns>明细列表</returns>
        public List<PurchaseOrderItem> GetPurchaseOrderItems(string orderNumber)
        {
            var items = new List<PurchaseOrderItem>();

            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT * FROM PurchaseOrderItems 
                        WHERE OrderNumber = @OrderNumber 
                        ORDER BY ProductName";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OrderNumber", orderNumber);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(new PurchaseOrderItem
                                {
                                    Id = reader.GetString("Id"),
                                    OrderNumber = reader.GetString("OrderNumber"),
                                    ProductCode = reader.GetString("ProductCode"),
                                    ProductName = reader.GetString("ProductName"),
                                    Quantity = reader.GetInt32("Quantity"),
                                    PurchasePrice = reader.GetDecimal("PurchasePrice"),
                                    Amount = reader.GetDecimal("Amount"),
                                    ExpiryDate = reader.IsDBNull("ExpiryDate") ? null : (DateTime?)reader.GetDateTime("ExpiryDate"),
                                    BatchNumber = reader.IsDBNull("BatchNumber") ? null : reader.GetString("BatchNumber"),
                                    Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取进货单明细失败: {ex.Message}", ex);
            }

            return items;
        }

        /// <summary>
        /// 分页查询进货单
        /// </summary>
        /// <param name="query">查询条件</param>
        /// <returns>分页结果</returns>
        public PurchaseOrderListResult GetPurchaseOrdersPaged(PurchaseOrderQuery query)
        {
            var result = new PurchaseOrderListResult
            {
                Orders = new List<PurchaseOrder>(),
                CurrentPage = query.PageIndex,
                PageSize = query.PageSize
            };

            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();

                    // 构建查询条件
                    var whereClause = "WHERE 1=1";
                    var parameters = new List<MySqlParameter>();

                    if (!string.IsNullOrEmpty(query.OrderNumber))
                    {
                        whereClause += " AND po.OrderNumber LIKE @OrderNumber";
                        parameters.Add(new MySqlParameter("@OrderNumber", $"%{query.OrderNumber}%"));
                    }

                    if (!string.IsNullOrEmpty(query.SupplierId))
                    {
                        whereClause += " AND po.SupplierId = @SupplierId";
                        parameters.Add(new MySqlParameter("@SupplierId", query.SupplierId));
                    }

                    if (query.Status.HasValue)
                    {
                        whereClause += " AND po.Status = @Status";
                        parameters.Add(new MySqlParameter("@Status", (int)query.Status.Value));
                    }

                    if (query.StartDate.HasValue)
                    {
                        whereClause += " AND po.OrderDate >= @StartDate";
                        parameters.Add(new MySqlParameter("@StartDate", query.StartDate.Value));
                    }

                    if (query.EndDate.HasValue)
                    {
                        whereClause += " AND po.OrderDate <= @EndDate";
                        parameters.Add(new MySqlParameter("@EndDate", query.EndDate.Value));
                    }

                    // 计算总数
                    string countQuery = $"SELECT COUNT(*) FROM PurchaseOrders po {whereClause}";
                    using (var countCommand = new MySqlCommand(countQuery, connection))
                    {
                        foreach (var param in parameters)
                        {
                            countCommand.Parameters.Add(param);
                        }
                        result.TotalCount = Convert.ToInt32(countCommand.ExecuteScalar());
                    }

                    // 计算总页数
                    result.TotalPages = (result.TotalCount + query.PageSize - 1) / query.PageSize;

                    // 获取分页数据
                    int offset = (query.PageIndex - 1) * query.PageSize;
                    string dataQuery = @$"
                        SELECT po.*, s.Name as SupplierName, u.Username as OperatorName
                        FROM PurchaseOrders po
                        LEFT JOIN Suppliers s ON po.SupplierId = s.Id
                        LEFT JOIN Users u ON po.OperatorId = u.Id
                        {whereClause}
                        ORDER BY po.OrderDate DESC, po.OrderNumber DESC
                        LIMIT @PageSize OFFSET @Offset";

                    using (var dataCommand = new MySqlCommand(dataQuery, connection))
                    {
                        // 添加查询参数
                        foreach (var param in parameters)
                        {
                            dataCommand.Parameters.Add(param.Clone() as MySqlParameter);
                        }
                        
                        // 添加分页参数
                        dataCommand.Parameters.AddWithValue("@PageSize", query.PageSize);
                        dataCommand.Parameters.AddWithValue("@Offset", offset);

                        using (var reader = dataCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var order = new PurchaseOrder
                                {
                                    OrderNumber = reader.GetString("OrderNumber"),
                                    OrderDate = reader.GetDateTime("OrderDate"),
                                    SupplierId = reader.GetString("SupplierId"),
                                    SupplierName = reader.GetString("SupplierName"),
                                    OperatorId = reader.GetString("OperatorId"),
                                    OperatorName = reader.GetString("OperatorName"),
                                    Status = (PurchaseOrderStatus)reader.GetInt32("Status"),
                                    TotalAmount = reader.GetDecimal("TotalAmount"),
                                    TaxAmount = reader.GetDecimal("TaxAmount"),
                                    FinalAmount = reader.GetDecimal("FinalAmount"),
                                    Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes"),
                                    CreatedAt = reader.GetDateTime("CreatedAt"),
                                    UpdatedAt = reader.IsDBNull("UpdatedAt") ? null : (DateTime?)reader.GetDateTime("UpdatedAt"),
                                    CompletedAt = reader.IsDBNull("CompletedAt") ? null : (DateTime?)reader.GetDateTime("CompletedAt")
                                };

                                // 加载订单明细以获取商品数量
                                order.Items = GetPurchaseOrderItems(order.OrderNumber);
                                result.Orders.Add(order);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"分页查询进货单失败: {ex.Message}", ex);
            }

            return result;
        }

        /// <summary>
        /// 生成进货单号
        /// </summary>
        /// <returns>进货单号</returns>
        public string GeneratePurchaseOrderNumber()
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = "SELECT MAX(OrderNumber) FROM PurchaseOrders WHERE OrderNumber LIKE 'PO%'";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        var result = command.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            string lastNumber = result.ToString();
                            if (lastNumber.Length >= 12 && lastNumber.StartsWith("PO"))
                            {
                                if (int.TryParse(lastNumber.Substring(2), out int lastSeq))
                                {
                                    return $"PO{(lastSeq + 1).ToString("D10")}";
                                }
                            }
                        }
                        
                        // 默认生成第一个单号
                        return $"PO{DateTime.Now:yyyyMMdd}001";
                    }
                }
            }
            catch (Exception)
            {
                // 如果出错，返回默认单号
                return $"PO{DateTime.Now:yyyyMMdd}001";
            }
        }

        /// <summary>
        /// 快速进货单个商品（智能处理）
        /// </summary>
        /// <param name="productCode">商品编码</param>
        /// <param name="productName">商品名称</param>
        /// <param name="quantity">进货数量</param>
        /// <param name="purchasePrice">进货单价</param>
        /// <param name="supplierId">供应商ID</param>
        /// <param name="operatorId">操作人ID</param>
        /// <param name="expiryDate">保质期（可选）</param>
        /// <param name="batchNumber">批次号（可选）</param>
        /// <returns>是否成功</returns>
        public bool QuickPurchaseProduct(string productCode, string productName, int quantity, decimal purchasePrice, 
                                       string supplierId, string operatorId, DateTime? expiryDate = null, string batchNumber = null)
        {
            try
            {
                var item = new PurchaseOrderItem
                {
                    ProductCode = productCode,
                    ProductName = productName,
                    Quantity = quantity,
                    PurchasePrice = purchasePrice,
                    ExpiryDate = expiryDate,
                    BatchNumber = batchNumber
                };

                return ProcessPurchaseItem(item, supplierId, operatorId);
            }
            catch (Exception ex)
            {
                throw new Exception($"快速进货商品失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 批量进货多个商品（智能处理）
        /// </summary>
        /// <param name="items">进货商品列表</param>
        /// <param name="supplierId">供应商ID</param>
        /// <param name="operatorId">操作人ID</param>
        /// <returns>是否成功</returns>
        public bool BatchPurchaseProducts(List<PurchaseOrderItem> items, string supplierId, string operatorId)
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
                            foreach (var item in items)
                            {
                                if (!ProcessPurchaseItem(item, supplierId, operatorId))
                                {
                                    throw new Exception($"处理商品 {item.ProductName} 失败");
                                }
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
                throw new Exception($"批量进货商品失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取进货统计
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>统计列表</returns>
        public List<PurchaseStatistics> GetPurchaseStatistics(DateTime startDate, DateTime endDate)
        {
            var statistics = new List<PurchaseStatistics>();

            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT 
                            DATE(OrderDate) as Date,
                            COUNT(*) as OrderCount,
                            SUM(FinalAmount) as TotalAmount,
                            SUM((SELECT COUNT(*) FROM PurchaseOrderItems poi WHERE poi.OrderNumber = po.OrderNumber)) as ProductCount
                        FROM PurchaseOrders po
                        WHERE OrderDate BETWEEN @StartDate AND @EndDate AND Status = @CompletedStatus
                        GROUP BY DATE(OrderDate)
                        ORDER BY Date";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StartDate", startDate);
                        command.Parameters.AddWithValue("@EndDate", endDate);
                        command.Parameters.AddWithValue("@CompletedStatus", (int)PurchaseOrderStatus.Completed);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                statistics.Add(new PurchaseStatistics
                                {
                                    Date = reader.GetDateTime("Date"),
                                    OrderCount = reader.GetInt32("OrderCount"),
                                    TotalAmount = reader.GetDecimal("TotalAmount"),
                                    ProductCount = reader.GetInt32("ProductCount")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取进货统计失败: {ex.Message}", ex);
            }

            return statistics;
        }
    }
}