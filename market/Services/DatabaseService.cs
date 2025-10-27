using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.IO;
using market.Models;

namespace market.Services
{
    /// <summary>
    /// 数据库服务类 - 管理MariaDB数据库连接和基本操作
    /// </summary>
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly string _serverConnectionString;

        public DatabaseService()
        {
            // 从配置文件中获取连接字符串
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MariaDBConnection"]?.ConnectionString;
            
            if (string.IsNullOrEmpty(_connectionString))
            {
                // 如果配置文件没有设置，使用默认连接字符串
                _connectionString = "Server=localhost;Database=market;Uid=root;Pwd=password;Port=3306;";
            }
            
            // 创建服务器连接字符串（不指定数据库，用于创建数据库）
            var builder = new MySqlConnectionStringBuilder(_connectionString);
            builder.Database = null; // 移除数据库名称
            _serverConnectionString = builder.ConnectionString;
            
            InitializeDatabase();
        }

        /// <summary>
        /// 初始化数据库和表结构
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                // 检查MySQL提供程序是否可用
                if (!IsMySQLProviderAvailable())
                {
                    throw new InvalidOperationException("MySQL provider is not available. Please ensure MySql.Data package is properly installed.");
                }

                // 第一步：连接服务器并检查数据库是否存在
                if (!DatabaseExists())
                {
                    CreateDatabase();
                }

                // 第二步：连接到具体数据库并创建表
                CreateTables();
            }
            catch (Exception ex)
            {
                throw new Exception($"数据库初始化失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 检查数据库是否存在
        /// </summary>
        private bool DatabaseExists()
        {
            try
            {
                using (var connection = new MySqlConnection(_serverConnectionString))
                {
                    connection.Open();
                    var query = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'market'";
                    
                    using (var command = new MySqlCommand(query, connection))
                    {
                        var result = command.ExecuteScalar();
                        return result != null && result.ToString().ToLower() == "market";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"检查数据库是否存在失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 创建数据库
        /// </summary>
        private void CreateDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(_serverConnectionString))
                {
                    connection.Open();
                    var query = "CREATE DATABASE IF NOT EXISTS market CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci";
                    
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine("数据库 market 创建成功");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"创建数据库失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 创建所有表
        /// </summary>
        private void CreateTables()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                
                // 创建用户表
                var createUserTable = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id VARCHAR(50) PRIMARY KEY,
                        Username VARCHAR(100) UNIQUE NOT NULL,
                        PasswordHash VARCHAR(255) NOT NULL,
                        Role INT NOT NULL
                    )";

                // 创建供货方表
                var createSupplierTable = @"
                    CREATE TABLE IF NOT EXISTS Suppliers (
                        Id VARCHAR(50) PRIMARY KEY,
                        Name VARCHAR(255) NOT NULL,
                        ProductionLocation VARCHAR(255),
                        ContactInfo VARCHAR(255),
                        BusinessLicense VARCHAR(255)
                    )";

                // 创建商品表
                var createProductTable = @"
                    CREATE TABLE IF NOT EXISTS Products (
                        ProductCode VARCHAR(50) PRIMARY KEY,
                        Name VARCHAR(255) NOT NULL,
                        Price DECIMAL(10,2) NOT NULL,
                        Quantity INT NOT NULL,
                        Unit VARCHAR(50) NOT NULL,
                        Category VARCHAR(100) NOT NULL,
                        ExpiryDate DATE,
                        StockAlertThreshold INT DEFAULT 10,
                        SupplierId VARCHAR(50)
                    )";

                // 创建订单表
                var createOrderTable = @"
                    CREATE TABLE IF NOT EXISTS Orders (
                        OrderNumber VARCHAR(50) PRIMARY KEY,
                        OrderDate DATETIME NOT NULL,
                        TotalAmount DECIMAL(10,2) NOT NULL,
                        PaymentMethod VARCHAR(50) NOT NULL,
                        CashierId VARCHAR(50) NOT NULL
                    )";

                // 创建订单明细表
                var createOrderItemsTable = @"
                    CREATE TABLE IF NOT EXISTS OrderItems (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        OrderNumber VARCHAR(50) NOT NULL,
                        ProductCode VARCHAR(50) NOT NULL,
                        ProductName VARCHAR(255) NOT NULL,
                        Quantity INT NOT NULL,
                        Price DECIMAL(10,2) NOT NULL,
                        Amount DECIMAL(10,2) NOT NULL
                    )";

                // 创建库存变动历史表
                var createInventoryHistoryTable = @"
                    CREATE TABLE IF NOT EXISTS InventoryHistory (
                        Id VARCHAR(50) PRIMARY KEY,
                        ProductCode VARCHAR(50) NOT NULL,
                        QuantityChange INT NOT NULL,
                        OperationType VARCHAR(50) NOT NULL,
                        OperationDate DATETIME NOT NULL,
                        OperatorId VARCHAR(50) NOT NULL,
                        OrderNumber VARCHAR(50),
                        PurchasePrice DECIMAL(10,2)
                    )";

                // 创建操作日志表
                var createOperationLogTable = @"
                    CREATE TABLE IF NOT EXISTS OperationLogs (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        OperationType VARCHAR(50) NOT NULL,
                        UserId VARCHAR(50) NOT NULL,
                        OperationTime DATETIME NOT NULL,
                        Details TEXT
                    )";

                // 创建商品分类表
                var createCategoryTable = @"
                    CREATE TABLE IF NOT EXISTS Categories (
                        Id VARCHAR(50) PRIMARY KEY,
                        Name VARCHAR(255) NOT NULL,
                        Description TEXT,
                        ParentId VARCHAR(50),
                        Level INT DEFAULT 1,
                        SortOrder INT DEFAULT 0,
                        IsActive BOOLEAN DEFAULT TRUE,
                        CreatedAt DATETIME NOT NULL,
                        UpdatedAt DATETIME NOT NULL,
                        IconPath VARCHAR(500),
                        Color VARCHAR(20),
                        FOREIGN KEY (ParentId) REFERENCES Categories(Id) ON DELETE SET NULL
                    )";

                // 执行所有创建表的SQL语句
                ExecuteCreateTable(connection, createUserTable, "Users");
                ExecuteCreateTable(connection, createSupplierTable, "Suppliers");
                ExecuteCreateTable(connection, createProductTable, "Products");
                ExecuteCreateTable(connection, createOrderTable, "Orders");
                ExecuteCreateTable(connection, createOrderItemsTable, "OrderItems");
                ExecuteCreateTable(connection, createInventoryHistoryTable, "InventoryHistory");
                ExecuteCreateTable(connection, createOperationLogTable, "OperationLogs");
                ExecuteCreateTable(connection, createCategoryTable, "Categories");

                // 初始化默认数据
                InitializeDefaultData(connection);
            }
        }

        /// <summary>
        /// 执行创建表的SQL语句
        /// </summary>
        private void ExecuteCreateTable(MySqlConnection connection, string sql, string tableName)
        {
            try
            {
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"{tableName} 表创建成功");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"创建 {tableName} 表失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 初始化默认数据
        /// </summary>
        private void InitializeDefaultData(MySqlConnection connection)
        {
            InitializeDefaultUsers(connection);
            InitializeDefaultSuppliers(connection);
            InitializeDefaultProducts(connection);
            InitializeDefaultCategories(connection);
        }

        /// <summary>
        /// 初始化默认用户
        /// </summary>
        private void InitializeDefaultUsers(MySqlConnection connection)
        {
            try
            {
                var checkAdmin = "SELECT COUNT(*) FROM Users WHERE Username = 'admin'";
                using (var command = new MySqlCommand(checkAdmin, connection))
                {
                    var count = Convert.ToInt32(command.ExecuteScalar());
                    if (count == 0)
                    {
                        // 创建默认管理员用户（密码：admin123，使用MD5加密）
                        var insertAdmin = @"
                            INSERT INTO Users (Id, Username, PasswordHash, Role) 
                            VALUES ('admin001', 'admin', '0192023a7bbd73250516f069df18b500', 0)";
                        
                        using (var insertCommand = new MySqlCommand(insertAdmin, connection))
                        {
                            insertCommand.ExecuteNonQuery();
                            System.Diagnostics.Debug.WriteLine("默认管理员用户创建成功");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"初始化默认用户失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 初始化默认供货方
        /// </summary>
        private void InitializeDefaultSuppliers(MySqlConnection connection)
        {
            try
            {
                var checkSuppliers = "SELECT COUNT(*) FROM Suppliers";
                using (var command = new MySqlCommand(checkSuppliers, connection))
                {
                    var count = Convert.ToInt32(command.ExecuteScalar());
                    if (count == 0)
                    {
                        var insertSuppliers = @"
                            INSERT IGNORE INTO Suppliers (Id, Name, ProductionLocation, ContactInfo, BusinessLicense) 
                            VALUES 
                            ('supplier001', '北京食品有限公司', '北京市朝阳区', '张经理 13800138000', '京食字001'),
                            ('supplier002', '上海日用品公司', '上海市浦东新区', '李经理 13900139000', '沪日字002'),
                            ('supplier003', '广州饮料集团', '广州市天河区', '王经理 13700137000', '粤饮字003')";
                        
                        using (var insertCommand = new MySqlCommand(insertSuppliers, connection))
                        {
                            insertCommand.ExecuteNonQuery();
                            System.Diagnostics.Debug.WriteLine("默认供货方数据创建成功");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"初始化默认供货方失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 初始化默认商品
        /// </summary>
        private void InitializeDefaultProducts(MySqlConnection connection)
        {
            try
            {
                var checkProducts = "SELECT COUNT(*) FROM Products";
                using (var command = new MySqlCommand(checkProducts, connection))
                {
                    var count = Convert.ToInt32(command.ExecuteScalar());
                    if (count == 0)
                    {
                        var insertProducts = @"
                            INSERT IGNORE INTO Products (ProductCode, Name, Price, Quantity, Unit, Category, ExpiryDate, StockAlertThreshold, SupplierId) 
                            VALUES 
                            ('P001', '可口可乐', 3.50, 100, '瓶', '饮料', '2024-12-31', 10, 'supplier003'),
                            ('P002', '康师傅红烧牛肉面', 4.50, 80, '桶', '食品', '2024-10-31', 15, 'supplier001'),
                            ('P003', '海飞丝洗发水', 25.00, 50, '瓶', '日用品', '2025-06-30', 5, 'supplier002'),
                            ('P004', '金龙鱼食用油', 65.00, 30, '桶', '粮油', '2024-08-31', 3, 'supplier001'),
                            ('P005', '中华铅笔', 1.50, 200, '支', '文具', NULL, 20, 'supplier002')";
                        
                        using (var insertCommand = new MySqlCommand(insertProducts, connection))
                        {
                            insertCommand.ExecuteNonQuery();
                            System.Diagnostics.Debug.WriteLine("默认商品数据创建成功");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"初始化默认商品失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 初始化默认分类
        /// </summary>
        private void InitializeDefaultCategories(MySqlConnection connection)
        {
            try
            {
                var checkCategories = "SELECT COUNT(*) FROM Categories";
                using (var command = new MySqlCommand(checkCategories, connection))
                {
                    var count = Convert.ToInt32(command.ExecuteScalar());
                    if (count == 0)
                    {
                        var insertCategories = @"
                            INSERT IGNORE INTO Categories (Id, Name, Description, ParentId, Level, SortOrder, IsActive, CreatedAt, UpdatedAt, Color) 
                            VALUES 
                            ('cat001', '食品', '各类食品分类', NULL, 1, 1, TRUE, NOW(), NOW(), '#FF6B6B'),
                            ('cat002', '饮料', '各类饮料分类', NULL, 1, 2, TRUE, NOW(), NOW(), '#4ECDC4'),
                            ('cat003', '日用品', '日常用品分类', NULL, 1, 3, TRUE, NOW(), NOW(), '#45B7D1'),
                            ('cat004', '粮油', '粮油调味品分类', NULL, 1, 4, TRUE, NOW(), NOW(), '#96CEB4'),
                            ('cat005', '文具', '文具办公用品分类', NULL, 1, 5, TRUE, NOW(), NOW(), '#FFEAA7'),
                            ('cat006', '零食', '零食小吃', 'cat001', 2, 1, TRUE, NOW(), NOW(), '#FF9F43'),
                            ('cat007', '熟食', '熟食制品', 'cat001', 2, 2, TRUE, NOW(), NOW(), '#FECA57'),
                            ('cat008', '碳酸饮料', '碳酸饮料', 'cat002', 2, 1, TRUE, NOW(), NOW(), '#54A0FF'),
                            ('cat009', '果汁', '果汁饮料', 'cat002', 2, 2, TRUE, NOW(), NOW(), '#5F27CD')";
                        
                        using (var insertCommand = new MySqlCommand(insertCategories, connection))
                        {
                            insertCommand.ExecuteNonQuery();
                            System.Diagnostics.Debug.WriteLine("默认分类数据创建成功");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"初始化默认分类失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查MySQL提供程序是否可用
        /// </summary>
        private bool IsMySQLProviderAvailable()
        {
            try
            {
                var factory = MySql.Data.MySqlClient.MySqlClientFactory.Instance;
                return factory != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取数据库连接
        /// </summary>
        public MySqlConnection GetConnection()
        {
            try
            {
                return new MySqlConnection(_connectionString);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create MySQL connection: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 测试数据库连接
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    using (var command = new MySqlCommand("SELECT 1", connection))
                    {
                        var result = command.ExecuteScalar();
                        return result != null && Convert.ToInt32(result) == 1;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database connection test failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查表是否存在
        /// </summary>
        public bool CheckTableExists(string tableName)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var query = $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{tableName}'";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        var result = command.ExecuteScalar();
                        return result != null && result.ToString().ToLower() == tableName.ToLower();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Check table {tableName} failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 验证所有必需的表是否存在
        /// </summary>
        public bool ValidateDatabaseTables()
        {
            string[] requiredTables = { "Users", "Products", "Suppliers", "Orders", "OrderItems", "InventoryHistory", "OperationLogs", "Categories" };
            
            foreach (var table in requiredTables)
            {
                if (!CheckTableExists(table))
                {
                    System.Diagnostics.Debug.WriteLine($"Table {table} not found!");
                    return false;
                }
            }
            
            System.Diagnostics.Debug.WriteLine("All required tables exist!");
            return true;
        }
    }
}