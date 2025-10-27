using System;
using System.Data;
using MySql.Data.MySqlClient;
using market.Models;

namespace market.Services
{
    /// <summary>
    /// MariaDB数据库服务类
    /// </summary>
    public class MariaDBService
    {
        private readonly string _connectionString;

        public MariaDBService()
        {
            // MariaDB连接字符串 - localhost:3306, root用户，无密码
            _connectionString = "Server=localhost;Port=3306;Database=market;User=root;Password=;";
            InitializeDatabase();
        }

        /// <summary>
        /// 初始化数据库和表结构
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                // 首先创建数据库（如果不存在）
                CreateDatabaseIfNotExists();
                
                // 然后创建表
                CreateTables();
                
                // 初始化默认数据
                InitializeDefaultData();
                
                System.Diagnostics.Debug.WriteLine("MariaDB数据库初始化完成");
            }
            catch (Exception ex)
            {
                throw new Exception($"MariaDB数据库初始化失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 创建数据库（如果不存在）
        /// </summary>
        private void CreateDatabaseIfNotExists()
        {
            // 先连接到默认数据库mysql
            var tempConnectionString = "Server=localhost;Port=3306;User=root;Password=;";
            
            using (var connection = new MySqlConnection(tempConnectionString))
            {
                connection.Open();
                
                // 检查market数据库是否存在
                var checkDbQuery = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'market'";
                
                using (var command = new MySqlCommand(checkDbQuery, connection))
                {
                    var result = command.ExecuteScalar();
                    if (result == null)
                    {
                        // 创建数据库
                        var createDbQuery = "CREATE DATABASE market CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci";
                        using (var createCommand = new MySqlCommand(createDbQuery, connection))
                        {
                            createCommand.ExecuteNonQuery();
                            System.Diagnostics.Debug.WriteLine("market数据库创建成功");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("market数据库已存在");
                    }
                }
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
                        PasswordHash VARCHAR(100) NOT NULL,
                        Role INT NOT NULL
                    )";

                // 创建供货方表
                var createSupplierTable = @"
                    CREATE TABLE IF NOT EXISTS Suppliers (
                        Id VARCHAR(50) PRIMARY KEY,
                        Name VARCHAR(200) NOT NULL,
                        ProductionLocation VARCHAR(200),
                        ContactInfo VARCHAR(200),
                        BusinessLicense VARCHAR(100)
                    )";

                // 创建商品表
                var createProductTable = @"
                    CREATE TABLE IF NOT EXISTS Products (
                        ProductCode VARCHAR(50) PRIMARY KEY,
                        Name VARCHAR(200) NOT NULL,
                        Price DECIMAL(10,2) NOT NULL,
                        Quantity INT NOT NULL,
                        Unit VARCHAR(20) NOT NULL,
                        Category VARCHAR(50) NOT NULL,
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
                        PaymentMethod VARCHAR(20) NOT NULL,
                        CashierId VARCHAR(50) NOT NULL
                    )";

                // 创建订单明细表
                var createOrderItemsTable = @"
                    CREATE TABLE IF NOT EXISTS OrderItems (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        OrderNumber VARCHAR(50) NOT NULL,
                        ProductCode VARCHAR(50) NOT NULL,
                        ProductName VARCHAR(200) NOT NULL,
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
                        OperationType VARCHAR(20) NOT NULL,
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

                // 执行所有创建表语句
                ExecuteTableCreation(connection, createUserTable, "Users");
                ExecuteTableCreation(connection, createSupplierTable, "Suppliers");
                ExecuteTableCreation(connection, createProductTable, "Products");
                ExecuteTableCreation(connection, createOrderTable, "Orders");
                ExecuteTableCreation(connection, createOrderItemsTable, "OrderItems");
                ExecuteTableCreation(connection, createInventoryHistoryTable, "InventoryHistory");
                ExecuteTableCreation(connection, createOperationLogTable, "OperationLogs");
            }
        }

        /// <summary>
        /// 执行表创建并记录结果
        /// </summary>
        private void ExecuteTableCreation(MySqlConnection connection, string sql, string tableName)
        {
            try
            {
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"{tableName}表创建成功");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{tableName}表创建失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 初始化默认数据
        /// </summary>
        private void InitializeDefaultData()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // 检查是否已有管理员用户
                var checkAdminQuery = "SELECT COUNT(*) FROM Users WHERE Username = 'admin'";
                
                using (var command = new MySqlCommand(checkAdminQuery, connection))
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
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("管理员用户已存在");
                    }
                }
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
                throw new Exception($"创建MariaDB连接失败: {ex.Message}", ex);
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
                System.Diagnostics.Debug.WriteLine($"MariaDB连接测试失败: {ex.Message}");
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
                    var query = $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'market' AND TABLE_NAME = '{tableName}';";
                    
                    using (var command = new MySqlCommand(query, connection))
                    {
                        var result = command.ExecuteScalar();
                        return result != null && result.ToString() == tableName;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"检查表 {tableName} 失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 验证所有必需的表是否存在
        /// </summary>
        public bool ValidateDatabaseTables()
        {
            string[] requiredTables = { "Users", "Products", "Suppliers", "Orders", "OrderItems", "InventoryHistory", "OperationLogs" };
            
            foreach (var table in requiredTables)
            {
                if (!CheckTableExists(table))
                {
                    System.Diagnostics.Debug.WriteLine($"表 {table} 不存在!");
                    return false;
                }
            }
            
            System.Diagnostics.Debug.WriteLine("所有必需的表都存在!");
            return true;
        }
    }
}