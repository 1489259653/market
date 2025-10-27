-- 超市管理系统数据库初始化脚本
-- 创建数据库
CREATE DATABASE IF NOT EXISTS market CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 使用数据库
USE market;

-- 创建用户表
CREATE TABLE IF NOT EXISTS Users (
    Id VARCHAR(50) PRIMARY KEY,
    Username VARCHAR(100) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Role INT NOT NULL
);

-- 创建供货方表
CREATE TABLE IF NOT EXISTS Suppliers (
    Id VARCHAR(50) PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    ProductionLocation VARCHAR(255),
    ContactInfo VARCHAR(255),
    BusinessLicense VARCHAR(255)
);

-- 创建商品表
CREATE TABLE IF NOT EXISTS Products (
    ProductCode VARCHAR(50) PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    Quantity INT NOT NULL,
    Unit VARCHAR(50) NOT NULL,
    Category VARCHAR(100) NOT NULL,
    ExpiryDate DATE,
    StockAlertThreshold INT DEFAULT 10,
    SupplierId VARCHAR(50),
    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
);

-- 创建订单表
CREATE TABLE IF NOT EXISTS Orders (
    OrderNumber VARCHAR(50) PRIMARY KEY,
    OrderDate DATETIME NOT NULL,
    TotalAmount DECIMAL(10,2) NOT NULL,
    PaymentMethod VARCHAR(50) NOT NULL,
    CashierId VARCHAR(50) NOT NULL,
    FOREIGN KEY (CashierId) REFERENCES Users(Id)
);

-- 创建订单明细表
CREATE TABLE IF NOT EXISTS OrderItems (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OrderNumber VARCHAR(50) NOT NULL,
    ProductCode VARCHAR(50) NOT NULL,
    ProductName VARCHAR(255) NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (OrderNumber) REFERENCES Orders(OrderNumber),
    FOREIGN KEY (ProductCode) REFERENCES Products(ProductCode)
);

-- 创建库存变动历史表
CREATE TABLE IF NOT EXISTS InventoryHistory (
    Id VARCHAR(50) PRIMARY KEY,
    ProductCode VARCHAR(50) NOT NULL,
    QuantityChange INT NOT NULL,
    OperationType VARCHAR(50) NOT NULL,
    OperationDate DATETIME NOT NULL,
    OperatorId VARCHAR(50) NOT NULL,
    OrderNumber VARCHAR(50),
    PurchasePrice DECIMAL(10,2),
    FOREIGN KEY (ProductCode) REFERENCES Products(ProductCode),
    FOREIGN KEY (OperatorId) REFERENCES Users(Id),
    FOREIGN KEY (OrderNumber) REFERENCES Orders(OrderNumber)
);

-- 创建操作日志表
CREATE TABLE IF NOT EXISTS OperationLogs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OperationType VARCHAR(50) NOT NULL,
    UserId VARCHAR(50) NOT NULL,
    OperationTime DATETIME NOT NULL,
    Details TEXT,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- 插入默认管理员用户（密码：admin123，使用MD5加密）
INSERT IGNORE INTO Users (Id, Username, PasswordHash, Role) 
VALUES ('admin001', 'admin', '0192023a7bbd73250516f069df18b500', 0);

-- 插入一些示例供货方
INSERT IGNORE INTO Suppliers (Id, Name, ProductionLocation, ContactInfo, BusinessLicense) 
VALUES 
('supplier001', '北京食品有限公司', '北京市朝阳区', '张经理 13800138000', '京食字001'),
('supplier002', '上海日用品公司', '上海市浦东新区', '李经理 13900139000', '沪日字002'),
('supplier003', '广州饮料集团', '广州市天河区', '王经理 13700137000', '粤饮字003');

-- 插入一些示例商品
INSERT IGNORE INTO Products (ProductCode, Name, Price, Quantity, Unit, Category, ExpiryDate, StockAlertThreshold, SupplierId) 
VALUES 
('P001', '可口可乐', 3.50, 100, '瓶', '饮料', '2024-12-31', 10, 'supplier003'),
('P002', '康师傅红烧牛肉面', 4.50, 80, '桶', '食品', '2024-10-31', 15, 'supplier001'),
('P003', '海飞丝洗发水', 25.00, 50, '瓶', '日用品', '2025-06-30', 5, 'supplier002'),
('P004', '金龙鱼食用油', 65.00, 30, '桶', '粮油', '2024-08-31', 3, 'supplier001'),
('P005', '中华铅笔', 1.50, 200, '支', '文具', NULL, 20, 'supplier002');

-- 显示创建的表信息
SHOW TABLES;

-- 显示表结构
DESCRIBE Users;
DESCRIBE Products;
DESCRIBE Orders;
DESCRIBE OrderItems;
DESCRIBE InventoryHistory;
DESCRIBE OperationLogs;