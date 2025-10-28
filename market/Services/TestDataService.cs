using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using market.Models;

namespace market.Services
{
    /// <summary>
    /// 测试数据服务类
    /// </summary>
    public class TestDataService
    {
        private readonly DatabaseService _databaseService;

        public TestDataService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// 创建进货管理模块的测试数据
        /// </summary>
        public bool CreatePurchaseTestData()
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
                            // 1. 创建用户测试数据
                            CreateUserTestData(connection, transaction);
                            
                            // 2. 创建供应商测试数据
                            CreateSupplierTestData(connection, transaction);
                            
                            // 3. 创建商品测试数据
                            CreateProductTestData(connection, transaction);
                            
                            // 4. 创建进货单测试数据
                            CreatePurchaseOrderTestData(connection, transaction);
                            
                            // 5. 创建进货明细测试数据
                            CreatePurchaseOrderItemTestData(connection, transaction);
                            
                            // 6. 创建库存变动历史记录
                            CreateInventoryHistoryTestData(connection, transaction);
                            
                            transaction.Commit();
                            Console.WriteLine("进货管理测试数据创建成功！");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"创建测试数据失败: {ex.Message}");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据库连接失败: {ex.Message}");
                return false;
            }
        }

        private void CreateUserTestData(MySqlConnection connection, MySqlTransaction transaction)
        {
            var users = new List<(string Id, string Username, string PasswordHash, UserRole Role)>
            {
                ("USER001", "admin", ComputeMD5Hash("admin123"), UserRole.Administrator),
                ("USER002", "warehouse", ComputeMD5Hash("warehouse123"), UserRole.WarehouseManager),
                ("USER003", "cashier", ComputeMD5Hash("cashier123"), UserRole.Cashier)
            };

            foreach (var user in users)
            {
                var query = @"
                    INSERT IGNORE INTO Users (Id, Username, PasswordHash, Role, CreatedAt) 
                    VALUES (@Id, @Username, @PasswordHash, @Role, NOW())";

                using (var command = new MySqlCommand(query, connection, transaction))
                {
                    command.Parameters.AddWithValue("@Id", user.Id);
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("@Role", (int)user.Role);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void CreateSupplierTestData(MySqlConnection connection, MySqlTransaction transaction)
        {
            var suppliers = new List<(string Id, string Name, string Contact, string Phone, string Email, string Address, string ProductionLocation)>
            {
                ("SUP001", "统一食品有限公司", "张经理", "13800138001", "zhang@tongyi.com", "北京市朝阳区建国路100号", "北京"),
                ("SUP002", "康师傅饮料集团", "李主任", "13800138002", "li@kangshifu.com", "上海市浦东新区张江高科技园区", "上海"),
                ("SUP003", "宝洁日用品公司", "王总监", "13800138003", "wang@baojie.com", "广州市天河区珠江新城", "广州"),
                ("SUP004", "金龙鱼粮油公司", "赵总", "13800138004", "zhao@jinlongyu.com", "深圳市南山区科技园", "深圳"),
                ("SUP005", "得力文具制造", "钱厂长", "13800138005", "qian@deli.com", "杭州市西湖区文三路", "杭州")
            };

            foreach (var supplier in suppliers)
            {
                var query = @"
                    INSERT IGNORE INTO Suppliers (Id, Name, Contact, Phone, Email, Address, ProductionLocation, CreatedAt) 
                    VALUES (@Id, @Name, @Contact, @Phone, @Email, @Address, @ProductionLocation, NOW())";

                using (var command = new MySqlCommand(query, connection, transaction))
                {
                    command.Parameters.AddWithValue("@Id", supplier.Id);
                    command.Parameters.AddWithValue("@Name", supplier.Name);
                    command.Parameters.AddWithValue("@Contact", supplier.Contact);
                    command.Parameters.AddWithValue("@Phone", supplier.Phone);
                    command.Parameters.AddWithValue("@Email", supplier.Email);
                    command.Parameters.AddWithValue("@Address", supplier.Address);
                    command.Parameters.AddWithValue("@ProductionLocation", supplier.ProductionLocation);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void CreateProductTestData(MySqlConnection connection, MySqlTransaction transaction)
        {
            var products = new List<(string ProductCode, string Name, decimal Price, int Quantity, string Unit, string Category, int StockAlertThreshold, decimal PurchasePrice, string SupplierId)>
            {
                ("P001", "统一绿茶500ml", 3.50m, 100, "瓶", "饮料", 20, 2.80m, "SUP002"),
                ("P002", "康师傅红烧牛肉面", 4.50m, 80, "袋", "食品", 15, 3.20m, "SUP001"),
                ("P003", "海飞丝去屑洗发水400ml", 35.00m, 50, "瓶", "日用品", 10, 25.00m, "SUP003"),
                ("P004", "金龙鱼大豆油5L", 65.00m, 30, "桶", "粮油", 5, 45.00m, "SUP004"),
                ("P005", "得力中性笔黑色", 2.50m, 200, "支", "文具", 50, 1.50m, "SUP005"),
                ("P006", "可口可乐330ml", 3.00m, 150, "罐", "饮料", 30, 2.20m, "SUP002"),
                ("P007", "奥利奥饼干", 8.50m, 60, "袋", "食品", 10, 6.00m, "SUP001"),
                ("P008", "飘柔洗发水750ml", 28.00m, 40, "瓶", "日用品", 8, 20.00m, "SUP003"),
                ("P009", "福临门大米10kg", 65.00m, 25, "袋", "粮油", 5, 45.00m, "SUP004"),
                ("P010", "真彩圆珠笔蓝色", 1.80m, 180, "支", "文具", 40, 1.00m, "SUP005")
            };

            foreach (var product in products)
            {
                var query = @"
                    INSERT IGNORE INTO Products (ProductCode, Name, Price, Quantity, Unit, Category, StockAlertThreshold, PurchasePrice, SupplierId, LastUpdated) 
                    VALUES (@ProductCode, @Name, @Price, @Quantity, @Unit, @Category, @StockAlertThreshold, @PurchasePrice, @SupplierId, NOW())";

                using (var command = new MySqlCommand(query, connection, transaction))
                {
                    command.Parameters.AddWithValue("@ProductCode", product.ProductCode);
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@Quantity", product.Quantity);
                    command.Parameters.AddWithValue("@Unit", product.Unit);
                    command.Parameters.AddWithValue("@Category", product.Category);
                    command.Parameters.AddWithValue("@StockAlertThreshold", product.StockAlertThreshold);
                    command.Parameters.AddWithValue("@PurchasePrice", product.PurchasePrice);
                    command.Parameters.AddWithValue("@SupplierId", product.SupplierId);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void CreatePurchaseOrderTestData(MySqlConnection connection, MySqlTransaction transaction)
        {
            var orders = new List<(string OrderNumber, DateTime OrderDate, string SupplierId, string OperatorId, PurchaseOrderStatus Status, decimal TotalAmount, decimal TaxAmount, decimal FinalAmount, string Notes, DateTime? CompletedAt)>
            {
                // 已完成订单
                ("PO20240115001", new DateTime(2024, 1, 15), "SUP001", "USER002", PurchaseOrderStatus.Completed, 12500.00m, 1250.00m, 13750.00m, "常规补货", new DateTime(2024, 1, 16, 14, 20, 0)),
                ("PO20240120001", new DateTime(2024, 1, 20), "SUP002", "USER002", PurchaseOrderStatus.Completed, 9800.00m, 980.00m, 10780.00m, "饮料促销备货", new DateTime(2024, 1, 21, 16, 30, 0)),
                ("PO20240125001", new DateTime(2024, 1, 25), "SUP003", "USER002", PurchaseOrderStatus.Completed, 15600.00m, 1560.00m, 17160.00m, "日用品季度补货", new DateTime(2024, 1, 26, 15, 45, 0)),
                
                // 已审核待收货订单
                ("PO20240201001", new DateTime(2024, 2, 1), "SUP004", "USER002", PurchaseOrderStatus.Approved, 8700.00m, 870.00m, 9570.00m, "粮油类商品补货", null),
                ("PO20240205001", new DateTime(2024, 2, 5), "SUP005", "USER002", PurchaseOrderStatus.Approved, 5600.00m, 560.00m, 6160.00m, "文具类商品补货", null),
                
                // 待审核订单
                ("PO20240210001", new DateTime(2024, 2, 10), "SUP001", "USER002", PurchaseOrderStatus.Pending, 4200.00m, 420.00m, 4620.00m, "食品临时补货", null),
                ("PO20240210002", new DateTime(2024, 2, 10), "SUP002", "USER002", PurchaseOrderStatus.Pending, 3100.00m, 310.00m, 3410.00m, "饮料临时补货", null),
                
                // 已到货订单
                ("PO20240208001", new DateTime(2024, 2, 8), "SUP003", "USER002", PurchaseOrderStatus.Delivered, 6800.00m, 680.00m, 7480.00m, "日用品到货", null),
                
                // 已取消订单
                ("PO20240130001", new DateTime(2024, 1, 30), "SUP004", "USER002", PurchaseOrderStatus.Cancelled, 9500.00m, 950.00m, 10450.00m, "供应商价格调整，重新下单", null)
            };

            foreach (var order in orders)
            {
                var query = @"
                    INSERT IGNORE INTO PurchaseOrders (OrderNumber, OrderDate, SupplierId, OperatorId, Status, TotalAmount, TaxAmount, FinalAmount, Notes, CreatedAt, UpdatedAt, CompletedAt) 
                    VALUES (@OrderNumber, @OrderDate, @SupplierId, @OperatorId, @Status, @TotalAmount, @TaxAmount, @FinalAmount, @Notes, NOW(), NOW(), @CompletedAt)";

                using (var command = new MySqlCommand(query, connection, transaction))
                {
                    command.Parameters.AddWithValue("@OrderNumber", order.OrderNumber);
                    command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                    command.Parameters.AddWithValue("@SupplierId", order.SupplierId);
                    command.Parameters.AddWithValue("@OperatorId", order.OperatorId);
                    command.Parameters.AddWithValue("@Status", (int)order.Status);
                    command.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                    command.Parameters.AddWithValue("@TaxAmount", order.TaxAmount);
                    command.Parameters.AddWithValue("@FinalAmount", order.FinalAmount);
                    command.Parameters.AddWithValue("@Notes", order.Notes);
                    command.Parameters.AddWithValue("@CompletedAt", order.CompletedAt.HasValue ? (object)order.CompletedAt.Value : DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void CreatePurchaseOrderItemTestData(MySqlConnection connection, MySqlTransaction transaction)
        {
            var items = new List<(string OrderNumber, string ProductCode, string ProductName, int Quantity, decimal PurchasePrice, decimal Amount, DateTime? ExpiryDate, string BatchNumber, string Notes)>
            {
                // PO20240115001 明细
                ("PO20240115001", "P002", "康师傅红烧牛肉面", 500, 3.20m, 1600.00m, new DateTime(2024, 7, 15), "BATCH202401001", "120g袋装"),
                ("PO20240115001", "P007", "奥利奥饼干", 300, 6.00m, 1800.00m, new DateTime(2024, 8, 20), "BATCH202401002", "原味夹心"),
                ("PO20240115001", "P001", "统一绿茶500ml", 1000, 2.80m, 2800.00m, new DateTime(2024, 6, 30), "BATCH202401003", "无糖型"),
                ("PO20240115001", "P006", "可口可乐330ml", 1500, 2.20m, 3300.00m, new DateTime(2024, 7, 10), "BATCH202401004", "罐装"),
                
                // PO20240120001 明细
                ("PO20240120001", "P001", "统一绿茶500ml", 1200, 2.80m, 3360.00m, new DateTime(2024, 7, 15), "BATCH202401005", "促销备货"),
                ("PO20240120001", "P006", "可口可乐330ml", 1600, 2.20m, 3520.00m, new DateTime(2024, 7, 20), "BATCH202401006", "夏季热销"),
                ("PO20240120001", "P002", "康师傅红烧牛肉面", 400, 3.20m, 1280.00m, new DateTime(2024, 8, 1), "BATCH202401007", "常规补货"),
                
                // PO20240125001 明细
                ("PO20240125001", "P003", "海飞丝去屑洗发水400ml", 200, 25.00m, 5000.00m, new DateTime(2025, 12, 31), "BATCH202401008", "去屑型"),
                ("PO20240125001", "P008", "飘柔洗发水750ml", 300, 20.00m, 6000.00m, new DateTime(2025, 12, 31), "BATCH202401009", "柔顺型"),
                ("PO20240125001", "P007", "奥利奥饼干", 200, 6.00m, 1200.00m, new DateTime(2024, 8, 15), "BATCH202401010", "巧克力味"),
                ("PO20240125001", "P002", "康师傅红烧牛肉面", 300, 3.20m, 960.00m, new DateTime(2024, 8, 20), "BATCH202401011", "大包装"),
                
                // 其他订单明细...
                ("PO20240201001", "P004", "金龙鱼大豆油5L", 100, 45.00m, 4500.00m, new DateTime(2024, 9, 30), "BATCH202402001", "一级压榨"),
                ("PO20240201001", "P009", "福临门大米10kg", 80, 45.00m, 3600.00m, new DateTime(2024, 10, 15), "BATCH202402002", "东北大米")
            };

            foreach (var item in items)
            {
                var query = @"
                    INSERT IGNORE INTO PurchaseOrderItems (Id, OrderNumber, ProductCode, ProductName, Quantity, PurchasePrice, Amount, ExpiryDate, BatchNumber, Notes) 
                    VALUES (UUID(), @OrderNumber, @ProductCode, @ProductName, @Quantity, @PurchasePrice, @Amount, @ExpiryDate, @BatchNumber, @Notes)";

                using (var command = new MySqlCommand(query, connection, transaction))
                {
                    command.Parameters.AddWithValue("@OrderNumber", item.OrderNumber);
                    command.Parameters.AddWithValue("@ProductCode", item.ProductCode);
                    command.Parameters.AddWithValue("@ProductName", item.ProductName);
                    command.Parameters.AddWithValue("@Quantity", item.Quantity);
                    command.Parameters.AddWithValue("@PurchasePrice", item.PurchasePrice);
                    command.Parameters.AddWithValue("@Amount", item.Amount);
                    command.Parameters.AddWithValue("@ExpiryDate", item.ExpiryDate.HasValue ? (object)item.ExpiryDate.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@BatchNumber", item.BatchNumber);
                    command.Parameters.AddWithValue("@Notes", item.Notes);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void CreateInventoryHistoryTestData(MySqlConnection connection, MySqlTransaction transaction)
        {
            var historyItems = new List<(string ProductCode, int QuantityChange, string OperationType, DateTime OperationDate, string OperatorId, decimal PurchasePrice, string OrderNumber)>
            {
                // 已完成订单的库存变动记录
                ("P002", 500, "进货", new DateTime(2024, 1, 16, 14, 20, 0), "USER002", 3.20m, "PO20240115001"),
                ("P007", 300, "进货", new DateTime(2024, 1, 16, 14, 20, 0), "USER002", 6.00m, "PO20240115001"),
                ("P001", 1000, "进货", new DateTime(2024, 1, 16, 14, 20, 0), "USER002", 2.80m, "PO20240115001"),
                ("P006", 1500, "进货", new DateTime(2024, 1, 16, 14, 20, 0), "USER002", 2.20m, "PO20240115001")
            };

            foreach (var item in historyItems)
            {
                var query = @"
                    INSERT IGNORE INTO InventoryHistory (Id, ProductCode, QuantityChange, OperationType, OperationDate, OperatorId, PurchasePrice, OrderNumber) 
                    VALUES (UUID(), @ProductCode, @QuantityChange, @OperationType, @OperationDate, @OperatorId, @PurchasePrice, @OrderNumber)";

                using (var command = new MySqlCommand(query, connection, transaction))
                {
                    command.Parameters.AddWithValue("@ProductCode", item.ProductCode);
                    command.Parameters.AddWithValue("@QuantityChange", item.QuantityChange);
                    command.Parameters.AddWithValue("@OperationType", item.OperationType);
                    command.Parameters.AddWithValue("@OperationDate", item.OperationDate);
                    command.Parameters.AddWithValue("@OperatorId", item.OperatorId);
                    command.Parameters.AddWithValue("@PurchasePrice", item.PurchasePrice);
                    command.Parameters.AddWithValue("@OrderNumber", item.OrderNumber);
                    command.ExecuteNonQuery();
                }
            }
        }

        private string ComputeMD5Hash(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}