using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using market.Models;

namespace market.Services
{
    /// <summary>
    /// 退货管理服务类
    /// </summary>
    public class ReturnService
    {
        private readonly DatabaseService _databaseService;
        private readonly AuthService _authService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="databaseService">数据库服务实例</param>
        /// <param name="authService">认证服务实例</param>
        public ReturnService(DatabaseService databaseService, AuthService authService)
        {
            _databaseService = databaseService;
            _authService = authService;
        }

        /// <summary>
        /// 创建退货订单
        /// </summary>
        /// <param name="returnOrder">退货订单信息</param>
        /// <returns>是否成功</returns>
        public bool CreateReturnOrder(ReturnOrder returnOrder)
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
                            // 生成退货单号
                            if (string.IsNullOrEmpty(returnOrder.ReturnNumber))
                            {
                                returnOrder.ReturnNumber = GenerateReturnOrderNumber();
                            }

                            // 验证原销售订单是否存在
                            if (!SaleOrderExists(returnOrder.OriginalOrderNumber))
                            {
                                throw new Exception("原销售订单不存在");
                            }

                            // 验证退货商品是否属于原销售订单
                            foreach (var item in returnOrder.Items)
                            {
                                if (!SaleOrderItemExists(returnOrder.OriginalOrderNumber, item.ProductCode))
                                {
                                    throw new Exception($"商品 {item.ProductName} 不属于原销售订单");
                                }
                            }

                            // 插入退货订单主表
                            string orderQuery = @"
                                INSERT INTO ReturnOrders (
                                    ReturnNumber, OriginalOrderNumber, ReturnDate, Customer, OperatorId, 
                                    Status, TotalAmount, RefundAmount, Reason, Notes, CreatedAt
                                ) VALUES (
                                    @ReturnNumber, @OriginalOrderNumber, @ReturnDate, @Customer, @OperatorId,
                                    @Status, @TotalAmount, @RefundAmount, @Reason, @Notes, @CreatedAt
                                )";

                            using (var orderCommand = new MySqlCommand(orderQuery, connection, transaction))
                            {
                                orderCommand.Parameters.AddWithValue("@ReturnNumber", returnOrder.ReturnNumber);
                                orderCommand.Parameters.AddWithValue("@OriginalOrderNumber", returnOrder.OriginalOrderNumber);
                                orderCommand.Parameters.AddWithValue("@ReturnDate", returnOrder.ReturnDate);
                                orderCommand.Parameters.AddWithValue("@Customer", returnOrder.Customer ?? "散客");
                                orderCommand.Parameters.AddWithValue("@OperatorId", returnOrder.OperatorId);
                                orderCommand.Parameters.AddWithValue("@Status", (int)returnOrder.Status);
                                orderCommand.Parameters.AddWithValue("@TotalAmount", returnOrder.TotalAmount);
                                orderCommand.Parameters.AddWithValue("@RefundAmount", returnOrder.RefundAmount);
                                orderCommand.Parameters.AddWithValue("@Reason", returnOrder.Reason ?? string.Empty);
                                orderCommand.Parameters.AddWithValue("@Notes", returnOrder.Notes ?? string.Empty);
                                orderCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                                orderCommand.ExecuteNonQuery();
                            }

                            // 插入退货明细并更新库存
                            foreach (var item in returnOrder.Items)
                            {
                                // 插入退货明细
                                string itemQuery = @"
                                    INSERT INTO ReturnOrderItems (
                                        Id, ReturnNumber, ProductCode, ProductName,
                                        Quantity, ReturnPrice, Amount, OriginalSalePrice, Reason
                                    ) VALUES (
                                        @Id, @ReturnNumber, @ProductCode, @ProductName,
                                        @Quantity, @ReturnPrice, @Amount, @OriginalSalePrice, @Reason
                                    )";

                                using (var itemCommand = new MySqlCommand(itemQuery, connection, transaction))
                                {
                                    itemCommand.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                                    itemCommand.Parameters.AddWithValue("@ReturnNumber", returnOrder.ReturnNumber);
                                    itemCommand.Parameters.AddWithValue("@ProductCode", item.ProductCode);
                                    itemCommand.Parameters.AddWithValue("@ProductName", item.ProductName);
                                    itemCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    itemCommand.Parameters.AddWithValue("@ReturnPrice", item.ReturnPrice);
                                    itemCommand.Parameters.AddWithValue("@Amount", item.Amount);
                                    itemCommand.Parameters.AddWithValue("@OriginalSalePrice", item.OriginalSalePrice);
                                    itemCommand.Parameters.AddWithValue("@Reason", item.Reason ?? string.Empty);

                                    itemCommand.ExecuteNonQuery();
                                }

                                // 更新商品库存（退货增加库存）
                                string updateStockQuery = @"
                                    UPDATE Products 
                                    SET Quantity = Quantity + @Quantity, 
                                        LastUpdated = @LastUpdated
                                    WHERE ProductCode = @ProductCode";

                                using (var stockCommand = new MySqlCommand(updateStockQuery, connection, transaction))
                                {
                                    stockCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    stockCommand.Parameters.AddWithValue("@LastUpdated", DateTime.Now);
                                    stockCommand.Parameters.AddWithValue("@ProductCode", item.ProductCode);

                                    stockCommand.ExecuteNonQuery();
                                }

                                // 记录退货历史
                                string historyQuery = @"
                                    INSERT INTO ReturnHistory (
                                        Id, ProductCode, Quantity, ReturnPrice, Amount,
                                        ReturnNumber, ReturnDate, OperatorId, Reason
                                    ) VALUES (
                                        @Id, @ProductCode, @Quantity, @ReturnPrice, @Amount,
                                        @ReturnNumber, @ReturnDate, @OperatorId, @Reason
                                    )";

                                using (var historyCommand = new MySqlCommand(historyQuery, connection, transaction))
                                {
                                    historyCommand.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                                    historyCommand.Parameters.AddWithValue("@ProductCode", item.ProductCode);
                                    historyCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    historyCommand.Parameters.AddWithValue("@ReturnPrice", item.ReturnPrice);
                                    historyCommand.Parameters.AddWithValue("@Amount", item.Amount);
                                    historyCommand.Parameters.AddWithValue("@ReturnNumber", returnOrder.ReturnNumber);
                                    historyCommand.Parameters.AddWithValue("@ReturnDate", returnOrder.ReturnDate);
                                    historyCommand.Parameters.AddWithValue("@OperatorId", returnOrder.OperatorId);
                                    historyCommand.Parameters.AddWithValue("@Reason", item.Reason ?? string.Empty);

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
                throw new Exception($"创建退货订单失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 生成退货单号
        /// 格式：RO + 年份(4) + 月份(2) + 日期(2) + 时间(4) + 操作标识(4)
        /// </summary>
        /// <returns>退货单号</returns>
        public string GenerateReturnOrderNumber()
        {
            try
            {
                // 获取当前时间
                DateTime now = DateTime.Now;
                
                // 获取操作标识符
                string operatorIdentifier = GetOperatorIdentifier();
                
                // 构建基础退货单号
                string baseReturnNumber = $"RO{now:yyyyMMddHHmm}{operatorIdentifier}";
                
                // 检查退货单号是否已存在
                int suffix = 1;
                string returnNumber = baseReturnNumber;
                
                while (ReturnOrderNumberExists(returnNumber))
                {
                    // 如果退货单号已存在，添加后缀
                    suffix++;
                    returnNumber = $"{baseReturnNumber}{suffix:D2}";
                    
                    // 防止无限循环，最多尝试100次
                    if (suffix > 100)
                    {
                        throw new Exception("无法生成唯一退货单号，请稍后重试");
                    }
                }
                
                return returnNumber;
            }
            catch (Exception)
            {
                // 如果出错，返回基于时间戳的备用品单号
                DateTime now = DateTime.Now;
                long timestamp = now.Ticks % 1000000000; // 取时间戳后9位
                return $"RO{now:yyyyMMddHHmm}{timestamp:D9}";
            }
        }
        
        /// <summary>
        /// 获取操作标识符（用户ID后四位或机器码后四位）
        /// </summary>
        /// <returns>操作标识符</returns>
        private string GetOperatorIdentifier()
        {
            // 如果当前有登录用户，使用用户ID后四位
            if (_authService?.CurrentUser != null && !string.IsNullOrEmpty(_authService.CurrentUser.Id))
            {
                string userId = _authService.CurrentUser.Id;
                if (userId.Length >= 4)
                {
                    return userId.Substring(userId.Length - 4).ToUpper();
                }
                else
                {
                    return userId.PadLeft(4, '0').ToUpper();
                }
            }
            
            // 如果没有登录用户，使用机器码后四位
            return MachineCodeService.GetMachineCode();
        }
        
        /// <summary>
        /// 检查退货单号是否已存在
        /// </summary>
        /// <param name="returnNumber">退货单号</param>
        /// <returns>是否存在</returns>
        private bool ReturnOrderNumberExists(string returnNumber)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = "SELECT COUNT(*) FROM ReturnOrders WHERE ReturnNumber = @ReturnNumber";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ReturnNumber", returnNumber);
                        
                        var count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception)
            {
                // 如果查询失败，假定退货单号不存在
                return false;
            }
        }

        /// <summary>
        /// 检查销售订单是否存在
        /// </summary>
        /// <param name="orderNumber">销售单号</param>
        /// <returns>是否存在</returns>
        private bool SaleOrderExists(string orderNumber)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = "SELECT COUNT(*) FROM SaleOrders WHERE OrderNumber = @OrderNumber";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OrderNumber", orderNumber);
                        
                        var count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 检查销售订单明细是否存在
        /// </summary>
        /// <param name="orderNumber">销售单号</param>
        /// <param name="productCode">商品编码</param>
        /// <returns>是否存在</returns>
        private bool SaleOrderItemExists(string orderNumber, string productCode)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = "SELECT COUNT(*) FROM SaleOrderItems WHERE OrderNumber = @OrderNumber AND ProductCode = @ProductCode";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OrderNumber", orderNumber);
                        command.Parameters.AddWithValue("@ProductCode", productCode);
                        
                        var count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 根据销售单号获取销售订单信息
        /// </summary>
        /// <param name="orderNumber">销售单号</param>
        /// <returns>销售订单信息</returns>
        public SaleOrder GetSaleOrderByNumber(string orderNumber)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT OrderNumber, OrderDate, Customer, OperatorId, Status, 
                               TotalAmount, DiscountAmount, FinalAmount, ReceivedAmount, 
                               ChangeAmount, PaymentMethod, Notes, CreatedAt
                        FROM SaleOrders 
                        WHERE OrderNumber = @OrderNumber";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OrderNumber", orderNumber);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new SaleOrder
                                {
                                    OrderNumber = reader.GetString("OrderNumber"),
                                    OrderDate = reader.GetDateTime("OrderDate"),
                                    Customer = reader.GetString("Customer"),
                                    OperatorId = reader.GetString("OperatorId"),
                                    Status = (SaleOrderStatus)reader.GetInt32("Status"),
                                    TotalAmount = reader.GetDecimal("TotalAmount"),
                                    DiscountAmount = reader.GetDecimal("DiscountAmount"),
                                    FinalAmount = reader.GetDecimal("FinalAmount"),
                                    ReceivedAmount = reader.GetDecimal("ReceivedAmount"),
                                    ChangeAmount = reader.GetDecimal("ChangeAmount"),
                                    PaymentMethod = (PaymentMethod)reader.GetInt32("PaymentMethod"),
                                    Notes = reader.GetString("Notes"),
                                    CreatedAt = reader.GetDateTime("CreatedAt")
                                };
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception)
            {
                throw new Exception($"获取销售订单信息失败");
            }
        }

        /// <summary>
        /// 根据销售单号获取销售订单明细
        /// </summary>
        /// <param name="orderNumber">销售单号</param>
        /// <returns>销售订单明细列表</returns>
        public List<SaleOrderItem> GetSaleOrderItems(string orderNumber)
        {
            try
            {
                var items = new List<SaleOrderItem>();
                
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT ProductCode, ProductName, Quantity, SalePrice, Amount, 
                               OriginalPrice, DiscountRate
                        FROM SaleOrderItems 
                        WHERE OrderNumber = @OrderNumber";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OrderNumber", orderNumber);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new SaleOrderItem
                                {
                                    ProductCode = reader.GetString("ProductCode"),
                                    ProductName = reader.GetString("ProductName"),
                                    Quantity = reader.GetInt32("Quantity"),
                                    SalePrice = reader.GetDecimal("SalePrice"),
                                    Amount = reader.GetDecimal("Amount"),
                                    OriginalPrice = reader.GetDecimal("OriginalPrice"),
                                    DiscountRate = reader.GetDecimal("DiscountRate")
                                };
                                items.Add(item);
                            }
                        }
                    }
                }

                return items;
            }
            catch (Exception)
            {
                throw new Exception($"获取销售订单明细失败");
            }
        }

        /// <summary>
        /// 根据退货单号获取退货订单信息
        /// </summary>
        /// <param name="returnNumber">退货单号</param>
        /// <returns>退货订单信息</returns>
        public ReturnOrder GetReturnOrder(string returnNumber)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT ReturnNumber, OriginalOrderNumber, ReturnDate, Customer, OperatorId, 
                               Status, TotalAmount, RefundAmount, Reason, Notes, CreatedAt
                        FROM ReturnOrders 
                        WHERE ReturnNumber = @ReturnNumber";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ReturnNumber", returnNumber);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var returnOrder = new ReturnOrder
                                {
                                    ReturnNumber = reader.GetString("ReturnNumber"),
                                    OriginalOrderNumber = reader.GetString("OriginalOrderNumber"),
                                    ReturnDate = reader.GetDateTime("ReturnDate"),
                                    Customer = reader.GetString("Customer"),
                                    OperatorId = reader.GetString("OperatorId"),
                                    Status = (ReturnOrderStatus)reader.GetInt32("Status"),
                                    TotalAmount = reader.GetDecimal("TotalAmount"),
                                    RefundAmount = reader.GetDecimal("RefundAmount"),
                                    Reason = reader.GetString("Reason"),
                                    Notes = reader.GetString("Notes"),
                                    CreatedAt = reader.GetDateTime("CreatedAt")
                                };

                                // 获取退货明细
                                returnOrder.Items = GetReturnOrderItems(returnNumber);
                                
                                return returnOrder;
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception)
            {
                throw new Exception($"获取退货订单信息失败");
            }
        }

        /// <summary>
        /// 根据退货单号获取退货订单明细
        /// </summary>
        /// <param name="returnNumber">退货单号</param>
        /// <returns>退货订单明细列表</returns>
        public List<ReturnOrderItem> GetReturnOrderItems(string returnNumber)
        {
            try
            {
                var items = new List<ReturnOrderItem>();
                
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT ProductCode, ProductName, Quantity, ReturnPrice, Amount, 
                               OriginalSalePrice, Reason
                        FROM ReturnOrderItems 
                        WHERE ReturnNumber = @ReturnNumber";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ReturnNumber", returnNumber);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new ReturnOrderItem
                                {
                                    ProductCode = reader.GetString("ProductCode"),
                                    ProductName = reader.GetString("ProductName"),
                                    Quantity = reader.GetInt32("Quantity"),
                                    ReturnPrice = reader.GetDecimal("ReturnPrice"),
                                    Amount = reader.GetDecimal("Amount"),
                                    OriginalSalePrice = reader.GetDecimal("OriginalSalePrice"),
                                    Reason = reader.GetString("Reason")
                                };
                                items.Add(item);
                            }
                        }
                    }
                }

                return items;
            }
            catch (Exception)
            {
                throw new Exception($"获取退货订单明细失败");
            }
        }

        /// <summary>
        /// 获取退货订单分页列表
        /// </summary>
        /// <param name="query">查询条件</param>
        /// <returns>退货订单分页结果</returns>
        public ReturnOrderListResult GetReturnOrdersPaged(ReturnOrderQuery query)
        {
            try
            {
                var result = new ReturnOrderListResult();
                
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    // 构建查询条件
                    var conditions = new List<string>();
                    var parameters = new List<MySqlParameter>();

                    if (!string.IsNullOrEmpty(query.ReturnNumber))
                    {
                        conditions.Add("ReturnNumber LIKE @ReturnNumber");
                        parameters.Add(new MySqlParameter("@ReturnNumber", $"%{query.ReturnNumber}%"));
                    }

                    if (!string.IsNullOrEmpty(query.OriginalOrderNumber))
                    {
                        conditions.Add("OriginalOrderNumber LIKE @OriginalOrderNumber");
                        parameters.Add(new MySqlParameter("@OriginalOrderNumber", $"%{query.OriginalOrderNumber}%"));
                    }

                    if (!string.IsNullOrEmpty(query.Customer))
                    {
                        conditions.Add("Customer LIKE @Customer");
                        parameters.Add(new MySqlParameter("@Customer", $"%{query.Customer}%"));
                    }

                    if (query.Status.HasValue)
                    {
                        conditions.Add("Status = @Status");
                        parameters.Add(new MySqlParameter("@Status", (int)query.Status.Value));
                    }

                    if (query.StartDate.HasValue)
                    {
                        conditions.Add("ReturnDate >= @StartDate");
                        parameters.Add(new MySqlParameter("@StartDate", query.StartDate.Value));
                    }

                    if (query.EndDate.HasValue)
                    {
                        conditions.Add("ReturnDate <= @EndDate");
                        parameters.Add(new MySqlParameter("@EndDate", query.EndDate.Value.AddDays(1)));
                    }

                    string whereClause = conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions)}" : "";

                    // 查询总数
                    string countQuery = $"SELECT COUNT(*) FROM ReturnOrders {whereClause}";
                    using (var countCommand = new MySqlCommand(countQuery, connection))
                    {
                        foreach (var param in parameters)
                        {
                            countCommand.Parameters.Add(param);
                        }
                        
                        result.TotalCount = Convert.ToInt32(countCommand.ExecuteScalar());
                    }

                    // 查询数据
                    string dataQuery = $"SELECT * FROM ReturnOrders {whereClause} ORDER BY ReturnDate DESC LIMIT @Skip, @Take";
                    
                    parameters.Add(new MySqlParameter("@Skip", (query.PageIndex - 1) * query.PageSize));
                    parameters.Add(new MySqlParameter("@Take", query.PageSize));

                    using (var dataCommand = new MySqlCommand(dataQuery, connection))
                    {
                        foreach (var param in parameters)
                        {
                            dataCommand.Parameters.Add(param);
                        }

                        using (var reader = dataCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var returnOrder = new ReturnOrder
                                {
                                    ReturnNumber = reader.GetString("ReturnNumber"),
                                    OriginalOrderNumber = reader.GetString("OriginalOrderNumber"),
                                    ReturnDate = reader.GetDateTime("ReturnDate"),
                                    Customer = reader.GetString("Customer"),
                                    OperatorId = reader.GetString("OperatorId"),
                                    Status = (ReturnOrderStatus)reader.GetInt32("Status"),
                                    TotalAmount = reader.GetDecimal("TotalAmount"),
                                    RefundAmount = reader.GetDecimal("RefundAmount"),
                                    Reason = reader.GetString("Reason"),
                                    Notes = reader.GetString("Notes"),
                                    CreatedAt = reader.GetDateTime("CreatedAt")
                                };

                                result.Orders.Add(returnOrder);
                            }
                        }
                    }

                    result.PageSize = query.PageSize;
                    result.CurrentPage = query.PageIndex;
                    result.TotalPages = (int)Math.Ceiling((double)result.TotalCount / query.PageSize);
                }

                return result;
            }
            catch (Exception)
            {
                throw new Exception($"获取退货订单列表失败");
            }
        }

        /// <summary>
        /// 更新退货订单状态
        /// </summary>
        /// <param name="returnNumber">退货单号</param>
        /// <param name="status">新状态</param>
        /// <returns>是否成功</returns>
        public bool UpdateReturnOrderStatus(string returnNumber, ReturnOrderStatus status)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = "UPDATE ReturnOrders SET Status = @Status WHERE ReturnNumber = @ReturnNumber";
                    
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Status", (int)status);
                        command.Parameters.AddWithValue("@ReturnNumber", returnNumber);
                        
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception($"更新退货订单状态失败");
            }
        }

        /// <summary>
        /// 更新退货订单
        /// </summary>
        /// <param name="returnOrder">退货订单信息</param>
        /// <returns>是否成功</returns>
        public bool UpdateReturnOrder(ReturnOrder returnOrder)
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
                            // 验证退货订单是否存在
                            if (!ReturnOrderNumberExists(returnOrder.ReturnNumber))
                            {
                                throw new Exception("退货订单不存在");
                            }

                            // 验证原销售订单是否存在
                            if (!SaleOrderExists(returnOrder.OriginalOrderNumber))
                            {
                                throw new Exception("原销售订单不存在");
                            }

                            // 验证退货商品是否属于原销售订单
                            foreach (var item in returnOrder.Items)
                            {
                                if (!SaleOrderItemExists(returnOrder.OriginalOrderNumber, item.ProductCode))
                                {
                                    throw new Exception($"商品 {item.ProductName} 不属于原销售订单");
                                }
                            }

                            // 获取原退货单的商品明细，用于计算库存差异
                            var originalItems = GetReturnOrderItemsByReturnNumber(returnOrder.ReturnNumber);
                            
                            // 更新退货订单主表
                            string orderQuery = @"
                                UPDATE ReturnOrders 
                                SET OriginalOrderNumber = @OriginalOrderNumber, 
                                    ReturnDate = @ReturnDate, 
                                    Customer = @Customer, 
                                    OperatorId = @OperatorId,
                                    Status = @Status, 
                                    TotalAmount = @TotalAmount, 
                                    RefundAmount = @RefundAmount, 
                                    Reason = @Reason, 
                                    Notes = @Notes
                                WHERE ReturnNumber = @ReturnNumber";

                            using (var orderCommand = new MySqlCommand(orderQuery, connection, transaction))
                            {
                                orderCommand.Parameters.AddWithValue("@ReturnNumber", returnOrder.ReturnNumber);
                                orderCommand.Parameters.AddWithValue("@OriginalOrderNumber", returnOrder.OriginalOrderNumber);
                                orderCommand.Parameters.AddWithValue("@ReturnDate", returnOrder.ReturnDate);
                                orderCommand.Parameters.AddWithValue("@Customer", returnOrder.Customer ?? "散客");
                                orderCommand.Parameters.AddWithValue("@OperatorId", returnOrder.OperatorId);
                                orderCommand.Parameters.AddWithValue("@Status", (int)returnOrder.Status);
                                orderCommand.Parameters.AddWithValue("@TotalAmount", returnOrder.TotalAmount);
                                orderCommand.Parameters.AddWithValue("@RefundAmount", returnOrder.RefundAmount);
                                orderCommand.Parameters.AddWithValue("@Reason", returnOrder.Reason ?? string.Empty);
                                orderCommand.Parameters.AddWithValue("@Notes", returnOrder.Notes ?? string.Empty);

                                orderCommand.ExecuteNonQuery();
                            }

                            // 删除原有的退货明细
                            string deleteItemsQuery = "DELETE FROM ReturnOrderItems WHERE ReturnNumber = @ReturnNumber";
                            using (var deleteCommand = new MySqlCommand(deleteItemsQuery, connection, transaction))
                            {
                                deleteCommand.Parameters.AddWithValue("@ReturnNumber", returnOrder.ReturnNumber);
                                deleteCommand.ExecuteNonQuery();
                            }

                            // 插入新的退货明细
                            foreach (var item in returnOrder.Items)
                            {
                                // 插入退货明细
                                string itemQuery = @"
                                    INSERT INTO ReturnOrderItems (
                                        Id, ReturnNumber, ProductCode, ProductName,
                                        Quantity, ReturnPrice, Amount, OriginalSalePrice, Reason
                                    ) VALUES (
                                        @Id, @ReturnNumber, @ProductCode, @ProductName,
                                        @Quantity, @ReturnPrice, @Amount, @OriginalSalePrice, @Reason
                                    )";

                                using (var itemCommand = new MySqlCommand(itemQuery, connection, transaction))
                                {
                                    itemCommand.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                                    itemCommand.Parameters.AddWithValue("@ReturnNumber", returnOrder.ReturnNumber);
                                    itemCommand.Parameters.AddWithValue("@ProductCode", item.ProductCode);
                                    itemCommand.Parameters.AddWithValue("@ProductName", item.ProductName);
                                    itemCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    itemCommand.Parameters.AddWithValue("@ReturnPrice", item.ReturnPrice);
                                    itemCommand.Parameters.AddWithValue("@Amount", item.Amount);
                                    itemCommand.Parameters.AddWithValue("@OriginalSalePrice", item.OriginalSalePrice);
                                    itemCommand.Parameters.AddWithValue("@Reason", item.Reason ?? string.Empty);

                                    itemCommand.ExecuteNonQuery();
                                }

                                // 记录退货历史
                                string historyQuery = @"
                                    INSERT INTO ReturnHistory (
                                        Id, ProductCode, Quantity, ReturnPrice, Amount,
                                        ReturnNumber, ReturnDate, OperatorId, Reason
                                    ) VALUES (
                                        @Id, @ProductCode, @Quantity, @ReturnPrice, @Amount,
                                        @ReturnNumber, @ReturnDate, @OperatorId, @Reason
                                    )";

                                using (var historyCommand = new MySqlCommand(historyQuery, connection, transaction))
                                {
                                    historyCommand.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                                    historyCommand.Parameters.AddWithValue("@ProductCode", item.ProductCode);
                                    historyCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    historyCommand.Parameters.AddWithValue("@ReturnPrice", item.ReturnPrice);
                                    historyCommand.Parameters.AddWithValue("@Amount", item.Amount);
                                    historyCommand.Parameters.AddWithValue("@ReturnNumber", returnOrder.ReturnNumber);
                                    historyCommand.Parameters.AddWithValue("@ReturnDate", returnOrder.ReturnDate);
                                    historyCommand.Parameters.AddWithValue("@OperatorId", returnOrder.OperatorId);
                                    historyCommand.Parameters.AddWithValue("@Reason", item.Reason ?? string.Empty);

                                    historyCommand.ExecuteNonQuery();
                                }
                            }

                            // 更新库存：先恢复原库存，再减去新库存
                            foreach (var originalItem in originalItems)
                            {
                                // 恢复原退货数量到库存（减少库存，因为退货时增加了库存）
                                string restoreStockQuery = @"
                                    UPDATE Products 
                                    SET Quantity = Quantity - @Quantity, 
                                        LastUpdated = @LastUpdated
                                    WHERE ProductCode = @ProductCode";

                                using (var restoreCommand = new MySqlCommand(restoreStockQuery, connection, transaction))
                                {
                                    restoreCommand.Parameters.AddWithValue("@Quantity", originalItem.Quantity);
                                    restoreCommand.Parameters.AddWithValue("@LastUpdated", DateTime.Now);
                                    restoreCommand.Parameters.AddWithValue("@ProductCode", originalItem.ProductCode);
                                    restoreCommand.ExecuteNonQuery();
                                }
                            }

                            // 应用新退货数量到库存
                            foreach (var newItem in returnOrder.Items)
                            {
                                // 更新商品库存（退货增加库存）
                                string updateStockQuery = @"
                                    UPDATE Products 
                                    SET Quantity = Quantity + @Quantity, 
                                        LastUpdated = @LastUpdated
                                    WHERE ProductCode = @ProductCode";

                                using (var stockCommand = new MySqlCommand(updateStockQuery, connection, transaction))
                                {
                                    stockCommand.Parameters.AddWithValue("@Quantity", newItem.Quantity);
                                    stockCommand.Parameters.AddWithValue("@LastUpdated", DateTime.Now);
                                    stockCommand.Parameters.AddWithValue("@ProductCode", newItem.ProductCode);
                                    stockCommand.ExecuteNonQuery();
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
                throw new Exception($"更新退货订单失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取退货统计信息
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>退货统计信息</returns>
        public List<ReturnStatistics> GetReturnStatistics(DateTime startDate, DateTime endDate)
        {
            try
            {
                var statistics = new List<ReturnStatistics>();
                
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT 
                            DATE(ReturnDate) as Date,
                            COUNT(*) as ReturnCount,
                            SUM(TotalAmount) as TotalReturnAmount,
                            AVG(TotalAmount) as AverageReturnAmount,
                            COUNT(DISTINCT ProductCode) as ProductCount
                        FROM ReturnOrders ro
                        LEFT JOIN ReturnOrderItems roi ON ro.ReturnNumber = roi.ReturnNumber
                        WHERE ReturnDate BETWEEN @StartDate AND @EndDate
                        GROUP BY DATE(ReturnDate)
                        ORDER BY Date";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StartDate", startDate);
                        command.Parameters.AddWithValue("@EndDate", endDate.AddDays(1));

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var stat = new ReturnStatistics
                                {
                                    Date = reader.GetDateTime("Date"),
                                    ReturnCount = reader.GetInt32("ReturnCount"),
                                    TotalReturnAmount = reader.IsDBNull("TotalReturnAmount") ? 0 : reader.GetDecimal("TotalReturnAmount"),
                                    AverageReturnAmount = reader.IsDBNull("AverageReturnAmount") ? 0 : reader.GetDecimal("AverageReturnAmount"),
                                    ProductCount = reader.GetInt32("ProductCount")
                                };
                                statistics.Add(stat);
                            }
                        }
                    }
                }

                return statistics;
            }
            catch (Exception)
            {
                throw new Exception($"获取退货统计信息失败");
            }
        }

        /// <summary>
        /// 获取退货原因分布
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>退货原因分布</returns>
        public Dictionary<string, int> GetReturnReasonDistribution(DateTime startDate, DateTime endDate)
        {
            try
            {
                var distribution = new Dictionary<string, int>();
                
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT Reason, COUNT(*) as Count
                        FROM ReturnOrders
                        WHERE ReturnDate BETWEEN @StartDate AND @EndDate AND Reason != ''
                        GROUP BY Reason
                        ORDER BY Count DESC";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StartDate", startDate);
                        command.Parameters.AddWithValue("@EndDate", endDate.AddDays(1));

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string reason = reader.GetString("Reason");
                                int count = reader.GetInt32("Count");
                                distribution[reason] = count;
                            }
                        }
                    }
                }

                return distribution;
            }
            catch (Exception)
            {
                throw new Exception($"获取退货原因分布失败");
            }
        }

        /// <summary>
        /// 根据退货单号获取退货明细
        /// </summary>
        /// <param name="returnNumber">退货单号</param>
        /// <returns>退货明细列表</returns>
        private List<ReturnOrderItem> GetReturnOrderItemsByReturnNumber(string returnNumber)
        {
            var items = new List<ReturnOrderItem>();
            
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT 
                            ProductCode, ProductName, Quantity, ReturnPrice, 
                            Amount, OriginalSalePrice, Reason
                        FROM ReturnOrderItems 
                        WHERE ReturnNumber = @ReturnNumber";
                    
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ReturnNumber", returnNumber);
                        
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new ReturnOrderItem
                                {
                                    ProductCode = reader["ProductCode"]?.ToString() ?? string.Empty,
                                    ProductName = reader["ProductName"]?.ToString() ?? string.Empty,
                                    Quantity = Convert.ToInt32(reader["Quantity"]),
                                    ReturnPrice = Convert.ToDecimal(reader["ReturnPrice"]),
                                    Amount = Convert.ToDecimal(reader["Amount"]),
                                    OriginalSalePrice = Convert.ToDecimal(reader["OriginalSalePrice"]),
                                    Reason = reader["Reason"]?.ToString() ?? string.Empty
                                };
                                
                                items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 如果查询失败，返回空列表
            }
            
            return items;
        }
    }
}