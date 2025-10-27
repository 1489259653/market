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