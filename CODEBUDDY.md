# CODEBUDDY.md This file provides guidance to CodeBuddy Code when working with code in this repository.

## Project Overview
This is a comprehensive C# WinForms supermarket management system built on .NET 6.0 Windows. The project implements a complete supermarket retail management solution with modular architecture.

## Build and Development Commands
- **Build**: `dotnet build` or `dotnet build --configuration Release`
- **Run**: `dotnet run` or execute `market\bin\Debug\net6.0-windows\market.exe`
- **Clean**: `dotnet clean`
- **Rebuild**: `dotnet clean && dotnet build`

## Architecture
- **UI Layer**: WinForms modular interface with multiple specialized forms
- **Business Layer**: Service classes for core business logic
- **Data Layer**: SQLite database with Entity Framework-like operations
- **Entry Point**: Program.cs - Application startup with dependency injection
- **Target Framework**: .NET 6.0 Windows
- **Output Type**: Windows Executable (WinExe)
- **Database**: SQLite (primary) + MariaDB (optional)

## Key Files and Structure

### 项目根目录
- `market.sln` - Visual Studio解决方案文件
- `开发进度.md` - 项目开发进度跟踪文档
- `需求规格.md` - 完整需求规格说明书
- `test_functions.md` - 功能验证清单

### 核心项目结构 (`market/`)
- `market.csproj` - 项目配置文件 (.NET 6.0 Windows)
- `Program.cs` - 应用程序入口点
- `DataInitializer.cs` - 数据库初始化脚本

### 数据模型层 (`market/Models/`)
- `Product.cs` - 商品信息模型
- `Supplier.cs` - 供应商信息模型
- `Order.cs`, `Sale.cs` - 订单和销售模型
- `Category.cs` - 商品分类模型
- `User.cs` - 用户信息模型
- `PurchaseOrder.cs` - 进货单模型
- `ReturnOrder.cs` - 退货单模型
- `InventoryHistory.cs` - 库存历史模型

### 服务层 (`market/Services/`)
- `DatabaseService.cs` - 数据库服务 (SQLite管理)
- `AuthService.cs` - 身份验证服务
- `ProductService.cs` - 商品管理服务
- `SaleService.cs` - 销售管理服务
- `InventoryService.cs` - 库存管理服务
- `PurchaseService.cs` - 进货管理服务
- `ReturnService.cs` - 退货管理服务
- `CategoryService.cs` - 分类管理服务
- `MachineCodeService.cs` - 机器码生成服务
- `MariaDBService.cs` - MariaDB数据库连接服务

### 用户界面层 (`market/Forms/`)
- `MainForm.cs` - 主界面框架
- `LoginForm.cs` - 登录界面
- `SaleCounterForm.cs` - 销售收银界面
- `ProductManagementForm.cs` - 商品管理界面
- `ProductEditForm.cs` - 商品编辑界面
- `InventoryForm.cs` - 库存管理界面
- `BarcodeScannerForm.cs` - 条形码扫描界面
- `PaymentForm.cs` - 支付界面
- `CategoryManagementForm.cs` - 分类管理界面
- `PurchaseManagementForm.cs` - 进货管理界面
- `ReturnManagementForm.cs` - 退货管理界面
- `ReturnOrderEditForm.cs` - 退货订单编辑界面
- `ReturnOrderViewForm.cs` - 退货订单详情界面
- `ReturnOrderItemEditForm.cs` - 退货商品明细编辑界面
- 及其他相关界面文件

## 当前系统状态 (System Status)

### ✅ 已完全实现的功能模块
- **商品管理** - 完整的商品CRUD操作，支持分类、供应商、条形码
- **库存管理** - 实时库存跟踪、预警系统、进货管理
- **销售管理** - 销售收银、支付处理、订单生成
- **退货管理** - 完整的退货流程、库存自动更新
- **供应商管理** - 供应商信息管理、关联商品
- **分类管理** - 分层分类结构、树形视图
- **用户权限** - 角色控制 (管理员、收银员、仓库管理员)
- **条形码扫描** - 摄像头扫描、商品自动识别
- **进货管理** - 进货单流程、智能库存更新

### 🔧 最近修复的重大问题
- **数据库结构同步** - 修复Products表缺失IsActive列的问题
- **UI布局优化** - 修复销售收银界面DataGridView显示问题
- **内存泄露修复** - 优化条形码扫描功能的内存管理
- **订单号生成规则** - 实现"SO年份月份日期时间操作人/机器码"格式
- **编译兼容性** - 修复AForge库和线程管理相关错误

## 技术架构详情
- **目标框架**: .NET 6.0 Windows (已从.NET Framework 4.8升级)
- **数据库**: SQLite (主数据库) + MariaDB (备用数据库)
- **UI框架**: WinForms with modern控件布局
- **安全特性**: MD5密码加密、角色权限控制、操作日志
- **硬件集成**: 摄像头扫描 (AForge.Video库)、Mock支付接口
- **性能优化**: 内存管理、线程安全、资源释放

## 开发指南
- 所有数据模型采用C# 9.0 record类型和属性
- 服务层采用依赖注入模式设计
- UI层使用TableLayoutPanel和Anchor实现响应式布局
- 数据库操作使用参数化查询防止SQL注入
- 异常处理采用try-catch和日志记录机制
- 支持中文本地化界面和业务逻辑

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
- **仓库管理员** - 拥有供应商管理权限和退货管理权限
- **收银员** - 无供应商管理权限和退货管理权限

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

## 退货管理模块

### 功能特性
- **完整退货流程**: 支持基于销售订单的退货，验证原销售订单和商品关联性
- **智能库存管理**: 退货时自动增加商品库存，保持库存准确性
- **退货状态管理**: 支持待处理、已审核、已完成、已取消四种状态流转
- **退货单号生成**: 自动生成唯一退货单号（格式：RO + 年月日时分 + 操作标识）
- **退货原因跟踪**: 支持订单级别和商品级别的退货原因记录
- **退货统计**: 提供退货统计分析和原因分布统计
- **权限控制**: 管理员和仓库管理员拥有退货管理权限

### 数据模型
- `ReturnOrder` 类: 退货订单主表信息，包含状态、金额、原销售单号等
- `ReturnOrderItem` 类: 退货明细，包含商品信息、数量、价格等
- `ReturnOrderStatus` 枚举: 退货订单状态（待处理、已审核、已完成、已取消）
- `ReturnOrderQuery` 类: 退货订单查询条件
- `ReturnStatistics` 类: 退货统计信息

### 服务类
- `ReturnService` 类: 提供退货管理的核心业务逻辑
  - 退货订单的增删改查操作
  - 退货订单状态流转管理
  - 自动验证原销售订单和商品关联性
  - 智能库存更新（退货增加库存）
  - 退货统计和分析功能

### 用户界面
- `ReturnManagementForm`: 退货单主管理界面，支持查询和列表展示
- `ReturnOrderEditForm`: 退货单编辑界面，支持新建和编辑退货单
- `ReturnOrderViewForm`: 退货单详情查看界面，展示完整退货信息
- `ReturnOrderItemEditForm`: 退货明细编辑界面，管理单个商品退货信息

### 数据库表结构
```sql
-- 退货订单主表
CREATE TABLE ReturnOrders (
    ReturnNumber VARCHAR(50) PRIMARY KEY,
    OriginalOrderNumber VARCHAR(50) NOT NULL,
    ReturnDate DATETIME NOT NULL,
    Customer VARCHAR(100) NOT NULL,
    OperatorId VARCHAR(50) NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL,
    RefundAmount DECIMAL(18,2) NOT NULL,
    Reason VARCHAR(255),
    Notes TEXT,
    CreatedAt DATETIME NOT NULL,
    FOREIGN KEY (OperatorId) REFERENCES Users(Id),
    FOREIGN KEY (OriginalOrderNumber) REFERENCES SaleOrders(OrderNumber)
);

-- 退货明细表
CREATE TABLE ReturnOrderItems (
    Id VARCHAR(50) PRIMARY KEY,
    ReturnNumber VARCHAR(50) NOT NULL,
    ProductCode VARCHAR(50) NOT NULL,
    ProductName VARCHAR(255) NOT NULL,
    Quantity INT NOT NULL,
    ReturnPrice DECIMAL(18,2) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    OriginalSalePrice DECIMAL(18,2) NOT NULL,
    Reason VARCHAR(255),
    FOREIGN KEY (ReturnNumber) REFERENCES ReturnOrders(ReturnNumber),
    FOREIGN KEY (ProductCode) REFERENCES Products(ProductCode)
);

-- 退货历史记录表
CREATE TABLE ReturnHistory (
    Id VARCHAR(50) PRIMARY KEY,
    ProductCode VARCHAR(50) NOT NULL,
    Quantity INT NOT NULL,
    ReturnPrice DECIMAL(18,2) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    ReturnNumber VARCHAR(50),
    ReturnDate DATETIME NOT NULL,
    OperatorId VARCHAR(50) NOT NULL,
    Reason VARCHAR(255),
    FOREIGN KEY (ProductCode) REFERENCES Products(ProductCode),
    FOREIGN KEY (ReturnNumber) REFERENCES ReturnOrders(ReturnNumber)
);
```

### 核心业务逻辑

#### 退货流程验证
```csharp
// 验证原销售订单是否存在
bool SaleOrderExists(string orderNumber)

// 验证退货商品是否属于原销售订单
bool SaleOrderItemExists(string orderNumber, string productCode)

// 创建退货订单（包含完整的事务处理）
bool CreateReturnOrder(ReturnOrder returnOrder)
{
    // 1. 验证原销售订单
    // 2. 验证退货商品属于原销售订单
    // 3. 插入退货订单
    // 4. 插入退货明细
    // 5. 更新商品库存（增加库存）
    // 6. 记录退货历史
}
```

#### 退货单号生成规则
```csharp
// 退货单号格式：RO + 年份(4) + 月份(2) + 日期(2) + 时间(4) + 操作标识(4)
string GenerateReturnOrderNumber()
{
    // 例如：RO202411251430001 (2024年11月25日14:30，操作标识0001)
    // 操作标识优先使用用户ID后四位，无用户时使用机器码
}
```

### 退货流程
1. **新建退货单**: 输入原销售单号，系统自动验证并加载顾客信息
2. **添加退货商品**: 选择或搜索商品，设置退货数量和原因
3. **验证退货**: 系统验证商品是否属于原销售订单
4. **保存退货**: 系统自动更新库存，生成退货记录
5. **状态管理**: 支持退货单的审核、完成和取消操作

### 权限控制
- **管理员**: 完整的退货管理权限，包括新建、编辑、审核、完成和取消
- **仓库管理员**: 完整的退货管理权限
- **收银员**: 无退货管理权限