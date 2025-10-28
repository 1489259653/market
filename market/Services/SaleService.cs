using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using market.Models;

namespace market.Services
{
    /// <summary>
    /// 销售管理服务类
    /// </summary>
    public class SaleService
    {
        private readonly DatabaseService _databaseService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="databaseService">数据库服务实例</param>
        public SaleService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// 创建销售订单
        /// </summary>
        /// <param name="order">销售订单信息</param>
        /// <returns>是否成功</returns>
        public bool CreateSaleOrder(SaleOrder order)
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
                            // 生成销售单号
                            if (string.IsNullOrEmpty(order.OrderNumber))
                            {
                                order.OrderNumber = GenerateSaleOrderNumber();
                            }

                            // 插入销售订单主表
                            string orderQuery = @"
                                INSERT INTO SaleOrders (
                                    OrderNumber, OrderDate, Customer, OperatorId, 
                                    Status, TotalAmount, DiscountAmount, FinalAmount, 
                                    ReceivedAmount, ChangeAmount, PaymentMethod, Notes, CreatedAt
                                ) VALUES (
                                    @OrderNumber, @OrderDate, @Customer, @OperatorId,
                                    @Status, @TotalAmount, @DiscountAmount, @FinalAmount,
                                    @ReceivedAmount, @ChangeAmount, @PaymentMethod, @Notes, @CreatedAt
                                )";

                            using (var orderCommand = new MySqlCommand(orderQuery, connection, transaction))
                            {
                                orderCommand.Parameters.AddWithValue("@OrderNumber", order.OrderNumber);
                                orderCommand.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                                orderCommand.Parameters.AddWithValue("@Customer", order.Customer ?? "散客");
                                orderCommand.Parameters.AddWithValue("@OperatorId", order.OperatorId);
                                orderCommand.Parameters.AddWithValue("@Status", (int)order.Status);
                                orderCommand.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                                orderCommand.Parameters.AddWithValue("@DiscountAmount", order.DiscountAmount);
                                orderCommand.Parameters.AddWithValue("@FinalAmount", order.FinalAmount);
                                orderCommand.Parameters.AddWithValue("@ReceivedAmount", order.ReceivedAmount);
                                orderCommand.Parameters.AddWithValue("@ChangeAmount", order.ChangeAmount);
                                orderCommand.Parameters.AddWithValue("@PaymentMethod", (int)order.PaymentMethod);
                                orderCommand.Parameters.AddWithValue("@Notes", order.Notes ?? string.Empty);
                                orderCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                                orderCommand.ExecuteNonQuery();
                            }

                            // 插入销售明细并更新库存
                            foreach (var item in order.Items)
                            {
                                // 插入销售明细
                                string itemQuery = @"
                                    INSERT INTO SaleOrderItems (
                                        Id, OrderNumber, ProductCode, ProductName,
                                        Quantity, SalePrice, Amount, OriginalPrice, DiscountRate
                                    ) VALUES (
                                        @Id, @OrderNumber, @ProductCode, @ProductName,
                                        @Quantity, @SalePrice, @Amount, @OriginalPrice, @DiscountRate
                                    )";

                                using (var itemCommand = new MySqlCommand(itemQuery, connection, transaction))
                                {
                                    itemCommand.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                                    itemCommand.Parameters.AddWithValue("@OrderNumber", order.OrderNumber);
                                    itemCommand.Parameters.AddWithValue("@ProductCode", item.ProductCode);
                                    itemCommand.Parameters.AddWithValue("@ProductName", item.ProductName);
                                    itemCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    itemCommand.Parameters.AddWithValue("@SalePrice", item.SalePrice);
                                    itemCommand.Parameters.AddWithValue("@Amount", item.Amount);
                                    itemCommand.Parameters.AddWithValue("@OriginalPrice", item.OriginalPrice);
                                    itemCommand.Parameters.AddWithValue("@DiscountRate", item.DiscountRate);

                                    itemCommand.ExecuteNonQuery();
                                }

                                // 更新商品库存
                                string updateStockQuery = @"
                                    UPDATE Products 
                                    SET Quantity = Quantity - @Quantity, 
                                        LastUpdated = @LastUpdated
                                    WHERE ProductCode = @ProductCode AND Quantity >= @Quantity";

                                using (var stockCommand = new MySqlCommand(updateStockQuery, connection, transaction))
                                {
                                    stockCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    stockCommand.Parameters.AddWithValue("@LastUpdated", DateTime.Now);
                                    stockCommand.Parameters.AddWithValue("@ProductCode", item.ProductCode);

                                    int rowsAffected = stockCommand.ExecuteNonQuery();
                                    if (rowsAffected == 0)
                                    {
                                        throw new Exception($"商品 {item.ProductName} 库存不足");
                                    }
                                }

                                // 记录销售历史
                                string historyQuery = @"
                                    INSERT INTO SaleHistory (
                                        Id, ProductCode, Quantity, SalePrice, Amount,
                                        OrderNumber, SaleDate, OperatorId
                                    ) VALUES (
                                        @Id, @ProductCode, @Quantity, @SalePrice, @Amount,
                                        @OrderNumber, @SaleDate, @OperatorId
                                    )";

                                using (var historyCommand = new MySqlCommand(historyQuery, connection, transaction))
                                {
                                    historyCommand.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                                    historyCommand.Parameters.AddWithValue("@ProductCode", item.ProductCode);
                                    historyCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    historyCommand.Parameters.AddWithValue("@SalePrice", item.SalePrice);
                                    historyCommand.Parameters.AddWithValue("@Amount", item.Amount);
                                    historyCommand.Parameters.AddWithValue("@OrderNumber", order.OrderNumber);
                                    historyCommand.Parameters.AddWithValue("@SaleDate", order.OrderDate);
                                    historyCommand.Parameters.AddWithValue("@OperatorId", order.OperatorId);

                                    historyCommand.ExecuteNonQuery();
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
                throw new Exception($"创建销售订单失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 根据商品编码获取商品信息
        /// </summary>
        /// <param name="productCode">商品编码</param>
        /// <returns>商品信息</returns>
        public Product GetProductByCode(string productCode)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT ProductCode, Name, Price, Quantity, Unit, Category, 
                               SupplierId, PurchasePrice, StockAlertThreshold, IsActive
                        FROM Products 
                        WHERE ProductCode = @ProductCode AND IsActive = TRUE";

                    using (var command = new MySqlCommand(query, connection))
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
                                    SupplierId = reader.GetString("SupplierId"),
                                    PurchasePrice = reader.GetDecimal("PurchasePrice"),
                                    StockAlertThreshold = reader.GetInt32("StockAlertThreshold"),
                                    IsActive = reader.GetBoolean("IsActive")
                                };
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"获取商品信息失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 根据条形码获取商品信息
        /// </summary>
        /// <param name="barcode">条形码</param>
        /// <returns>商品信息</returns>
        public Product GetProductByBarcode(string barcode)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT p.ProductCode, p.Name, p.Price, p.Quantity, p.Unit, p.Category,
                               p.SupplierId, p.PurchasePrice, p.StockAlertThreshold, p.IsActive
                        FROM Products p
                        LEFT JOIN ProductBarcodes pb ON p.ProductCode = pb.ProductCode
                        WHERE pb.Barcode = @Barcode AND p.IsActive = TRUE";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Barcode", barcode);

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
                                    SupplierId = reader.GetString("SupplierId"),
                                    PurchasePrice = reader.GetDecimal("PurchasePrice"),
                                    StockAlertThreshold = reader.GetInt32("StockAlertThreshold"),
                                    IsActive = reader.GetBoolean("IsActive")
                                };
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"根据条形码获取商品信息失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 搜索商品
        /// </summary>
        /// <param name="keyword">关键词</param>
        /// <returns>商品列表</returns>
        public List<Product> SearchProducts(string keyword)
        {
            var products = new List<Product>();

            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT ProductCode, Name, Price, Quantity, Unit, Category,
                               SupplierId, PurchasePrice, StockAlertThreshold, IsActive
                        FROM Products 
                        WHERE IsActive = TRUE AND 
                              (ProductCode LIKE @Keyword OR Name LIKE @Keyword OR Category LIKE @Keyword)
                        ORDER BY Name";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Keyword", $"%{keyword}%");

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                products.Add(new Product
                                {
                                    ProductCode = reader.GetString("ProductCode"),
                                    Name = reader.GetString("Name"),
                                    Price = reader.GetDecimal("Price"),
                                    Quantity = reader.GetInt32("Quantity"),
                                    Unit = reader.GetString("Unit"),
                                    Category = reader.GetString("Category"),
                                    SupplierId = reader.GetString("SupplierId"),
                                    PurchasePrice = reader.GetDecimal("PurchasePrice"),
                                    StockAlertThreshold = reader.GetInt32("StockAlertThreshold"),
                                    IsActive = reader.GetBoolean("IsActive")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"搜索商品失败: {ex.Message}", ex);
            }

            return products;
        }

        /// <summary>
        /// 检查商品库存是否充足
        /// </summary>
        /// <param name="productCode">商品编码</param>
        /// <param name="quantity">数量</param>
        /// <returns>是否充足</returns>
        public bool CheckStock(string productCode, int quantity)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = "SELECT Quantity FROM Products WHERE ProductCode = @ProductCode";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductCode", productCode);
                        
                        var currentQuantity = Convert.ToInt32(command.ExecuteScalar());
                        return currentQuantity >= quantity;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"检查库存失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 生成销售单号
        /// </summary>
        /// <returns>销售单号</returns>
        public string GenerateSaleOrderNumber()
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = "SELECT MAX(OrderNumber) FROM SaleOrders WHERE OrderNumber LIKE 'SO%'";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        var result = command.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            string lastNumber = result.ToString();
                            if (lastNumber.Length >= 12 && lastNumber.StartsWith("SO"))
                            {
                                if (int.TryParse(lastNumber.Substring(2), out int lastSeq))
                                {
                                    return $"SO{(lastSeq + 1).ToString("D10")}";
                                }
                            }
                        }
                        
                        // 默认生成第一个单号
                        return $"SO{DateTime.Now:yyyyMMdd}001";
                    }
                }
            }
            catch (Exception)
            {
                // 如果出错，返回默认单号
                return $"SO{DateTime.Now:yyyyMMdd}001";
            }
        }

        /// <summary>
        /// 获取销售订单详情
        /// </summary>
        /// <param name="orderNumber">销售单号</param>
        /// <returns>销售订单信息</returns>
        public SaleOrder GetSaleOrder(string orderNumber)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT so.*, u.Username as OperatorName
                        FROM SaleOrders so
                        LEFT JOIN Users u ON so.OperatorId = u.Id
                        WHERE so.OrderNumber = @OrderNumber";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OrderNumber", orderNumber);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var order = new SaleOrder
                                {
                                    OrderNumber = reader.GetString("OrderNumber"),
                                    OrderDate = reader.GetDateTime("OrderDate"),
                                    Customer = reader.GetString("Customer"),
                                    OperatorId = reader.GetString("OperatorId"),
                                    OperatorName = reader.GetString("OperatorName"),
                                    Status = (SaleOrderStatus)reader.GetInt32("Status"),
                                    TotalAmount = reader.GetDecimal("TotalAmount"),
                                    DiscountAmount = reader.GetDecimal("DiscountAmount"),
                                    FinalAmount = reader.GetDecimal("FinalAmount"),
                                    ReceivedAmount = reader.GetDecimal("ReceivedAmount"),
                                    ChangeAmount = reader.GetDecimal("ChangeAmount"),
                                    PaymentMethod = (PaymentMethod)reader.GetInt32("PaymentMethod"),
                                    Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes"),
                                    CreatedAt = reader.GetDateTime("CreatedAt")
                                };

                                // 获取明细
                                order.Items = GetSaleOrderItems(orderNumber);
                                
                                return order;
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"获取销售订单详情失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取销售订单明细
        /// </summary>
        /// <param name="orderNumber">销售单号</param>
        /// <returns>明细列表</returns>
        public List<SaleOrderItem> GetSaleOrderItems(string orderNumber)
        {
            var items = new List<SaleOrderItem>();

            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT * FROM SaleOrderItems 
                        WHERE OrderNumber = @OrderNumber 
                        ORDER BY ProductName";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OrderNumber", orderNumber);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(new SaleOrderItem
                                {
                                    Id = reader.GetString("Id"),
                                    OrderNumber = reader.GetString("OrderNumber"),
                                    ProductCode = reader.GetString("ProductCode"),
                                    ProductName = reader.GetString("ProductName"),
                                    Quantity = reader.GetInt32("Quantity"),
                                    SalePrice = reader.GetDecimal("SalePrice"),
                                    Amount = reader.GetDecimal("Amount"),
                                    OriginalPrice = reader.GetDecimal("OriginalPrice"),
                                    DiscountRate = reader.GetDecimal("DiscountRate")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取销售订单明细失败: {ex.Message}", ex);
            }

            return items;
        }

        /// <summary>
        /// 分页查询销售订单
        /// </summary>
        /// <param name="query">查询条件</param>
        /// <returns>分页结果</returns>
        public SaleOrderListResult GetSaleOrdersPaged(SaleOrderQuery query)
        {
            var result = new SaleOrderListResult
            {
                Orders = new List<SaleOrder>(),
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
                        whereClause += " AND so.OrderNumber LIKE @OrderNumber";
                        parameters.Add(new MySqlParameter("@OrderNumber", $"%{query.OrderNumber}%"));
                    }

                    if (!string.IsNullOrEmpty(query.Customer))
                    {
                        whereClause += " AND so.Customer LIKE @Customer";
                        parameters.Add(new MySqlParameter("@Customer", $"%{query.Customer}%"));
                    }

                    if (query.Status.HasValue)
                    {
                        whereClause += " AND so.Status = @Status";
                        parameters.Add(new MySqlParameter("@Status", (int)query.Status.Value));
                    }

                    if (query.PaymentMethod.HasValue)
                    {
                        whereClause += " AND so.PaymentMethod = @PaymentMethod";
                        parameters.Add(new MySqlParameter("@PaymentMethod", (int)query.PaymentMethod.Value));
                    }

                    if (query.StartDate.HasValue)
                    {
                        whereClause += " AND so.OrderDate >= @StartDate";
                        parameters.Add(new MySqlParameter("@StartDate", query.StartDate.Value));
                    }

                    if (query.EndDate.HasValue)
                    {
                        whereClause += " AND so.OrderDate <= @EndDate";
                        parameters.Add(new MySqlParameter("@EndDate", query.EndDate.Value));
                    }

                    // 计算总数
                    string countQuery = $"SELECT COUNT(*) FROM SaleOrders so {whereClause}";
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
                        SELECT so.*, u.Username as OperatorName
                        FROM SaleOrders so
                        LEFT JOIN Users u ON so.OperatorId = u.Id
                        {whereClause}
                        ORDER BY so.OrderDate DESC, so.OrderNumber DESC
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
                                var order = new SaleOrder
                                {
                                    OrderNumber = reader.GetString("OrderNumber"),
                                    OrderDate = reader.GetDateTime("OrderDate"),
                                    Customer = reader.GetString("Customer"),
                                    OperatorId = reader.GetString("OperatorId"),
                                    OperatorName = reader.GetString("OperatorName"),
                                    Status = (SaleOrderStatus)reader.GetInt32("Status"),
                                    TotalAmount = reader.GetDecimal("TotalAmount"),
                                    DiscountAmount = reader.GetDecimal("DiscountAmount"),
                                    FinalAmount = reader.GetDecimal("FinalAmount"),
                                    ReceivedAmount = reader.GetDecimal("ReceivedAmount"),
                                    ChangeAmount = reader.GetDecimal("ChangeAmount"),
                                    PaymentMethod = (PaymentMethod)reader.GetInt32("PaymentMethod"),
                                    Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes"),
                                    CreatedAt = reader.GetDateTime("CreatedAt")
                                };

                                // 加载订单明细
                                order.Items = GetSaleOrderItems(order.OrderNumber);
                                result.Orders.Add(order);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"分页查询销售订单失败: {ex.Message}", ex);
            }

            return result;
        }
    }
}