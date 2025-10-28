# CODEBUDDY.md This file provides guidance to CodeBuddy Code when working with code in this repository.

## Project Overview
This is a C# WinForms supermarket management system built on .NET Framework 4.8. The project is structured as a single Visual Studio solution with one Windows Forms application project.

## Build and Development Commands
- **Build**: Use Visual Studio build or `MSBuild market.sln`
- **Run**: Execute `market\bin\Debug\market.exe` or `market\bin\Release\market.exe`
- **Clean**: Use `MSBuild market.sln /t:Clean` or Visual Studio clean
- **Rebuild**: Use `MSBuild market.sln /t:Clean;Build`

## Architecture
- **UI Layer**: WinForms (Form1.cs) - Main application window
- **Entry Point**: Program.cs - Application startup
- **Target Framework**: .NET Framework 4.8
- **Output Type**: Windows Executable (WinExe)

## Key Files and Structure
- `market.sln` - Visual Studio solution file
- `market/market.csproj` - Project configuration
- `market/Form1.cs` - Main form class
- `market/Form1.Designer.cs` - Form designer code
- `market/Program.cs` - Application entry point
- `market/App.config` - Application configuration
- `market/Properties/` - Assembly information and resources

## Domain Model (Based on Requirements)
The system implements a supermarket management system with:
- **Product Management**: Products with codes, pricing, categories, inventory
- **Category Management**: Hierarchical product categories with tree structure
- **Inventory Tracking**: Stock levels with replenishment alerts
- **Sales Processing**: Transaction handling with receipts
- **User Management**: Role-based access (Admin, Cashier, Warehouse Manager)
- **Reporting**: Sales analytics and inventory reports

## Development Notes
- This is a Windows Forms application, not a web or console app
- Database layer uses MariaDB/MySQL with connection pooling
- Support for hierarchical product categories with unlimited levels
- Mock implementations are used for hardware integration (scanning, printing)
- Follows Chinese business requirements and terminology
- Focus on supermarket retail operations and inventory management

## 供应商管理模块

### 功能概述
供应商管理模块已完全集成到库存管理系统中，提供完整的供应商CRUD操作功能。

### 主要功能
- **供应商列表查看** - 在库存管理界面中查看所有供应商信息
- **供应商添加** - 支持新供应商的创建
- **供应商编辑** - 可修改现有供应商信息
- **供应商删除** - 删除未使用的供应商（有商品关联的供应商无法删除）
- **权限控制** - 仓库管理员拥有供应商管理权限

### 关键文件
- `Forms/SupplierEditForm.cs` - 供应商编辑界面
- `Models/Supplier.cs` - 供应商数据模型
- `Services/ProductService.cs` - 供应商CRUD操作方法
- `Forms/InventoryForm.cs` - 库存管理界面（包含供应商管理选项卡）

### 使用方法
1. 登录系统（使用仓库管理员或管理员账户）

## 条形码扫描功能

### 功能概述
条形码扫描功能已集成到商品编辑界面中，支持通过摄像头扫描条形码自动识别商品信息。

### 主要功能
- **商品编辑界面扫描** - 在商品编辑表单中添加了扫描按钮
- **摄像头访问** - 模拟摄像头访问功能（实际部署时可替换为真实摄像头）
- **条形码识别** - 支持EAN-13等标准条形码格式识别
- **自动填充** - 扫描成功后自动填充商品编码及相关信息（如果商品已存在）
- **手动输入** - 提供手动输入条形码的备选方案

### 关键文件
- `Forms/ProductEditForm.cs` - 商品编辑界面（包含扫描按钮）
- `Forms/BarcodeScannerForm.cs` - 条形码扫描界面
- `Resources/scan_icon.svg` - 扫描图标资源

### 使用方法
1. 在商品编辑界面点击"扫描"按钮
2. 在弹出的扫描窗口中点击"开始扫描"按钮
3. 将条形码对准摄像头（模拟环境下会随机触发扫描成功）
4. 扫描成功后，系统会自动填充商品信息（如果商品已存在）
5. 也可以使用手动输入功能，在输入框中输入条形码
2. 进入"库存管理" → "供应商管理"
3. 可以进行供应商的添加、编辑、删除操作

### 权限设置
- **管理员** - 拥有所有权限
- **仓库管理员** - 拥有供应商管理权限
- **收银员** - 无供应商管理权限

## Common Tasks
- Adding new forms: Create new Form classes and update navigation
- Database operations: Use MariaDB/MySQL with appropriate data access layer
- UI updates: Modify WinForms controls and event handlers
- Business logic: Implement in separate classes following the requirements specification

## 商品分类管理模块

### 功能特性
- **分层分类管理**: 支持无限层级分类结构
- **分类统计**: 实时显示各分类下的商品数量
- **分类树形视图**: 可视化展示分类层次结构
- **颜色标识**: 为分类设置颜色标识，便于区分
- **排序功能**: 支持分类排序和层级管理
- **软删除**: 禁用分类而非物理删除，保护数据完整性

### 数据模型
- `Category` 类: 包含分类ID、名称、描述、父分类ID、层级、排序等属性
- `CategoryTreeNode` 类: 用于树形结构展示
- `CategoryStatistics` 类: 分类统计信息

### 服务类
- `CategoryService` 类: 提供分类的增删改查操作
  - 分类树形结构管理
  - 分类统计功能
  - 分类验证和业务规则

### 用户界面
- `CategoryManagementForm`: 主管理界面，包含树形视图和列表视图
- `CategoryEditForm`: 分类编辑界面，支持添加和编辑分类

### 数据库表结构
```sql
CREATE TABLE Categories (
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
)
```

### 默认分类数据
系统包含以下默认分类：
- 食品 (零食、熟食等子分类)
- 饮料 (碳酸饮料、果汁等子分类)
- 日用品
- 粮油
- 文具

### 权限控制
- 管理员和仓库管理员有完整的分类管理权限
- 收银员只能查看分类信息，无管理权限

## 进货管理模块

### 功能特性
- **智能库存管理**: 已存在商品则更新库存，不存在商品则自动新增并录入库存
- **进货单管理**: 完整的进货流程管理，支持新建、编辑、审核、完成和取消操作
- **多商品批量进货**: 支持一次性进货多个商品
- **快速进货**: 支持快速进货单个商品，无需完整进货单流程
- **进货状态跟踪**: 跟踪进货单从创建到完成的完整状态流转
- **库存自动更新**: 完成进货后自动更新商品库存和进货价格
- **进货历史记录**: 完整的进货历史记录和库存变动跟踪
- **供应商管理**: 关联供应商信息，支持供应商选择
- **税率管理**: 支持自定义税率，自动计算税额和最终金额

### 数据模型
- `PurchaseOrder` 类: 进货单主表信息，包含状态、金额、供应商等
- `PurchaseOrderItem` 类: 进货明细，包含商品信息、数量、价格等
- `PurchaseOrderStatus` 枚举: 进货单状态（待审核、已审核、已到货、已完成、已取消）
- `PurchaseOrderQuery` 类: 进货单查询条件
- `PurchaseStatistics` 类: 进货统计信息

### 服务类
- `PurchaseService` 类: 提供进货管理的核心业务逻辑
  - 进货单的增删改查操作
  - 进货单状态流转管理
  - **智能库存更新功能**：存在商品更新库存，不存在商品自动新增
  - 快速进货单个商品功能
  - 批量进货多个商品功能
  - 进货统计和分析功能

### 用户界面
- `PurchaseManagementForm`: 进货单主管理界面，支持查询和列表展示
- `PurchaseOrderEditForm`: 进货单编辑界面，支持新建和编辑进货单
- `PurchaseOrderItemEditForm`: 进货明细编辑界面，管理单个商品进货信息
- `PurchaseOrderViewForm`: 进货单详情查看界面，展示完整进货信息

### 数据库表结构
```sql
-- 进货单主表
CREATE TABLE PurchaseOrders (
    OrderNumber VARCHAR(50) PRIMARY KEY,
    OrderDate DATETIME NOT NULL,
    SupplierId VARCHAR(50) NOT NULL,
    OperatorId VARCHAR(50) NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL,
    TaxAmount DECIMAL(18,2) NOT NULL,
    FinalAmount DECIMAL(18,2) NOT NULL,
    Notes TEXT,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME,
    CompletedAt DATETIME,
    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id),
    FOREIGN KEY (OperatorId) REFERENCES Users(Id)
);

-- 进货明细表
CREATE TABLE PurchaseOrderItems (
    Id VARCHAR(50) PRIMARY KEY,
    OrderNumber VARCHAR(50) NOT NULL,
    ProductCode VARCHAR(50) NOT NULL,
    ProductName VARCHAR(255) NOT NULL,
    Quantity INT NOT NULL,
    PurchasePrice DECIMAL(18,2) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    ExpiryDate DATETIME,
    BatchNumber VARCHAR(100),
    Notes TEXT,
    FOREIGN KEY (OrderNumber) REFERENCES PurchaseOrders(OrderNumber),
    FOREIGN KEY (ProductCode) REFERENCES Products(ProductCode)
);
```

### 核心业务逻辑

#### 智能库存管理
```csharp
// 检查商品是否存在
bool ProductExists(string productCode)

// 智能处理进货商品（核心功能）
bool ProcessPurchaseItem(PurchaseOrderItem item, string supplierId, string operatorId)
{
    if (ProductExists(item.ProductCode))
    {
        // 商品已存在：更新库存和进货价格
        UPDATE Products SET Quantity = Quantity + @Quantity, PurchasePrice = @PurchasePrice
    }
    else
    {
        // 商品不存在：新增商品信息并设置默认值
        INSERT INTO Products (ProductCode, Name, Price, Quantity, ...)
        // 默认设置：分类为"未分类"，单位"个"，售价=进货价×1.2
    }
    
    // 记录库存变动历史
    INSERT INTO InventoryHistory
}
```

#### 便捷进货方法
```csharp
// 快速进货单个商品
bool QuickPurchaseProduct(string productCode, string productName, int quantity, 
                          decimal purchasePrice, string supplierId, string operatorId)

// 批量进货多个商品
bool BatchPurchaseProducts(List<PurchaseOrderItem> items, string supplierId, string operatorId)

// 完成进货单（智能更新库存）
bool CompletePurchaseOrder(string orderNumber, string operatorId)
```

### 进货流程
1. **新建进货单**: 选择供应商，添加商品明细，设置税率和备注
2. **智能处理商品**: 系统自动判断商品是否存在，存在则更新库存，不存在则新增商品
3. **保存草稿**: 进货单初始状态为"待审核"
4. **审核通过**: 管理员审核进货单，状态变为"已审核"
5. **到货确认**: 商品到货后标记为"已到货"
6. **完成入库**: 确认入库后自动更新库存，状态变为"已完成"
7. **取消订单**: 在任意状态可以取消进货单

### 快速进货流程
- 直接调用 `QuickPurchaseProduct` 方法
- 无需完整进货单流程
- 自动处理商品存在性检查
- 适用于小批量、临时进货场景

### 权限控制
- **仓库管理员**: 完整的进货管理权限，包括新建、编辑、审核、完成和取消
- **管理员**: 查看所有进货记录和统计信息
- **收银员**: 只能查看进货记录，无管理权限