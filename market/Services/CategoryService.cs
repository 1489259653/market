using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using market.Models;

namespace market.Services
{
    /// <summary>
    /// 商品分类服务类 - 管理商品分类的增删改查操作
    /// </summary>
    public class CategoryService
    {
        private readonly DatabaseService _databaseService;

        public CategoryService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            InitializeCategoryTable();
        }

        /// <summary>
        /// 初始化分类表结构
        /// </summary>
        private void InitializeCategoryTable()
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
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

                    using (var command = new MySqlCommand(createCategoryTable, connection))
                    {
                        command.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine("Categories 表创建成功");
                    }

                    // 检查是否存在默认分类数据
                    InitializeDefaultCategories(connection);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"初始化分类表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 初始化默认分类数据
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
        /// 获取所有分类
        /// </summary>
        public List<Category> GetAllCategories()
        {
            var categories = new List<Category>();
            
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = @"
                        SELECT 
                            c.Id, c.Name, c.Description, c.ParentId, c.Level, c.SortOrder, 
                            c.IsActive, c.CreatedAt, c.UpdatedAt, c.IconPath, c.Color,
                            parent.Name as ParentName,
                            (SELECT COUNT(*) FROM Products WHERE Category = c.Name) as ProductCount
                        FROM Categories c
                        LEFT JOIN Categories parent ON c.ParentId = parent.Id
                        ORDER BY c.Level, c.SortOrder, c.Name";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var category = new Category
                            {
                                Id = reader.GetString("Id"),
                                Name = reader.GetString("Name"),
                                Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                                ParentId = reader.IsDBNull("ParentId") ? null : reader.GetString("ParentId"),
                                Level = reader.GetInt32("Level"),
                                SortOrder = reader.GetInt32("SortOrder"),
                                IsActive = reader.GetBoolean("IsActive"),
                                CreatedAt = reader.GetDateTime("CreatedAt"),
                                UpdatedAt = reader.GetDateTime("UpdatedAt"),
                                IconPath = reader.IsDBNull("IconPath") ? null : reader.GetString("IconPath"),
                                Color = reader.IsDBNull("Color") ? null : reader.GetString("Color"),
                                ParentName = reader.IsDBNull("ParentName") ? null : reader.GetString("ParentName"),
                                ProductCount = reader.GetInt32("ProductCount")
                            };
                            categories.Add(category);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取所有分类失败: {ex.Message}");
                throw new Exception($"获取分类列表失败: {ex.Message}", ex);
            }
            
            return categories;
        }

        /// <summary>
        /// 根据ID获取分类
        /// </summary>
        public Category GetCategoryById(string id)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = @"
                        SELECT 
                            c.Id, c.Name, c.Description, c.ParentId, c.Level, c.SortOrder, 
                            c.IsActive, c.CreatedAt, c.UpdatedAt, c.IconPath, c.Color,
                            parent.Name as ParentName,
                            (SELECT COUNT(*) FROM Products WHERE Category = c.Name) as ProductCount
                        FROM Categories c
                        LEFT JOIN Categories parent ON c.ParentId = parent.Id
                        WHERE c.Id = @Id";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Category
                                {
                                    Id = reader.GetString("Id"),
                                    Name = reader.GetString("Name"),
                                    Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                                    ParentId = reader.IsDBNull("ParentId") ? null : reader.GetString("ParentId"),
                                    Level = reader.GetInt32("Level"),
                                    SortOrder = reader.GetInt32("SortOrder"),
                                    IsActive = reader.GetBoolean("IsActive"),
                                    CreatedAt = reader.GetDateTime("CreatedAt"),
                                    UpdatedAt = reader.GetDateTime("UpdatedAt"),
                                    IconPath = reader.IsDBNull("IconPath") ? null : reader.GetString("IconPath"),
                                    Color = reader.IsDBNull("Color") ? null : reader.GetString("Color"),
                                    ParentName = reader.IsDBNull("ParentName") ? null : reader.GetString("ParentName"),
                                    ProductCount = reader.GetInt32("ProductCount")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取分类失败: {ex.Message}");
                throw new Exception($"获取分类信息失败: {ex.Message}", ex);
            }
            
            return null;
        }

        /// <summary>
        /// 添加分类
        /// </summary>
        public bool AddCategory(Category category)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = @"
                        INSERT INTO Categories (Id, Name, Description, ParentId, Level, SortOrder, IsActive, CreatedAt, UpdatedAt, IconPath, Color)
                        VALUES (@Id, @Name, @Description, @ParentId, @Level, @SortOrder, @IsActive, @CreatedAt, @UpdatedAt, @IconPath, @Color)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", category.Id ?? Guid.NewGuid().ToString());
                        command.Parameters.AddWithValue("@Name", category.Name);
                        command.Parameters.AddWithValue("@Description", (object)category.Description ?? DBNull.Value);
                        command.Parameters.AddWithValue("@ParentId", (object)category.ParentId ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Level", category.Level);
                        command.Parameters.AddWithValue("@SortOrder", category.SortOrder);
                        command.Parameters.AddWithValue("@IsActive", category.IsActive);
                        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                        command.Parameters.AddWithValue("@IconPath", (object)category.IconPath ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Color", (object)category.Color ?? DBNull.Value);

                        var result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"添加分类失败: {ex.Message}");
                throw new Exception($"添加分类失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 更新分类
        /// </summary>
        public bool UpdateCategory(Category category)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = @"
                        UPDATE Categories 
                        SET Name = @Name, Description = @Description, ParentId = @ParentId, 
                            Level = @Level, SortOrder = @SortOrder, IsActive = @IsActive, 
                            UpdatedAt = @UpdatedAt, IconPath = @IconPath, Color = @Color
                        WHERE Id = @Id";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", category.Id);
                        command.Parameters.AddWithValue("@Name", category.Name);
                        command.Parameters.AddWithValue("@Description", (object)category.Description ?? DBNull.Value);
                        command.Parameters.AddWithValue("@ParentId", (object)category.ParentId ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Level", category.Level);
                        command.Parameters.AddWithValue("@SortOrder", category.SortOrder);
                        command.Parameters.AddWithValue("@IsActive", category.IsActive);
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                        command.Parameters.AddWithValue("@IconPath", (object)category.IconPath ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Color", (object)category.Color ?? DBNull.Value);

                        var result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新分类失败: {ex.Message}");
                throw new Exception($"更新分类失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 删除分类（软删除，将IsActive设为False）
        /// </summary>
        public bool DeleteCategory(string id)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    // 检查是否有子分类
                    var checkChildren = "SELECT COUNT(*) FROM Categories WHERE ParentId = @Id AND IsActive = TRUE";
                    using (var command = new MySqlCommand(checkChildren, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        var childCount = Convert.ToInt32(command.ExecuteScalar());
                        
                        if (childCount > 0)
                        {
                            throw new Exception("该分类下存在子分类，无法删除");
                        }
                    }
                    
                    // 检查是否有商品使用此分类
                    var category = GetCategoryById(id);
                    if (category != null && category.ProductCount > 0)
                    {
                        throw new Exception("该分类下存在商品，无法删除");
                    }
                    
                    // 软删除
                    var query = "UPDATE Categories SET IsActive = FALSE, UpdatedAt = NOW() WHERE Id = @Id";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        var result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"删除分类失败: {ex.Message}");
                throw new Exception($"删除分类失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取顶级分类（Level=1）
        /// </summary>
        public List<Category> GetTopLevelCategories()
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = @"
                        SELECT 
                            c.Id, c.Name, c.Description, c.ParentId, c.Level, c.SortOrder, 
                            c.IsActive, c.CreatedAt, c.UpdatedAt, c.IconPath, c.Color,
                            (SELECT COUNT(*) FROM Products WHERE Category = c.Name) as ProductCount
                        FROM Categories c
                        WHERE c.Level = 1 AND c.IsActive = TRUE
                        ORDER BY c.SortOrder, c.Name";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        var categories = new List<Category>();
                        while (reader.Read())
                        {
                            var category = new Category
                            {
                                Id = reader.GetString("Id"),
                                Name = reader.GetString("Name"),
                                Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                                ParentId = reader.IsDBNull("ParentId") ? null : reader.GetString("ParentId"),
                                Level = reader.GetInt32("Level"),
                                SortOrder = reader.GetInt32("SortOrder"),
                                IsActive = reader.GetBoolean("IsActive"),
                                CreatedAt = reader.GetDateTime("CreatedAt"),
                                UpdatedAt = reader.GetDateTime("UpdatedAt"),
                                IconPath = reader.IsDBNull("IconPath") ? null : reader.GetString("IconPath"),
                                Color = reader.IsDBNull("Color") ? null : reader.GetString("Color"),
                                ProductCount = reader.GetInt32("ProductCount")
                            };
                            categories.Add(category);
                        }
                        return categories;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取顶级分类失败: {ex.Message}");
                throw new Exception($"获取顶级分类失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 根据父分类ID获取子分类
        /// </summary>
        public List<Category> GetChildCategories(string parentId)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = @"
                        SELECT 
                            c.Id, c.Name, c.Description, c.ParentId, c.Level, c.SortOrder, 
                            c.IsActive, c.CreatedAt, c.UpdatedAt, c.IconPath, c.Color,
                            parent.Name as ParentName,
                            (SELECT COUNT(*) FROM Products WHERE Category = c.Name) as ProductCount
                        FROM Categories c
                        LEFT JOIN Categories parent ON c.ParentId = parent.Id
                        WHERE c.ParentId = @ParentId AND c.IsActive = TRUE
                        ORDER BY c.SortOrder, c.Name";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ParentId", parentId);
                        
                        using (var reader = command.ExecuteReader())
                        {
                            var categories = new List<Category>();
                            while (reader.Read())
                            {
                                var category = new Category
                                {
                                    Id = reader.GetString("Id"),
                                    Name = reader.GetString("Name"),
                                    Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                                    ParentId = reader.IsDBNull("ParentId") ? null : reader.GetString("ParentId"),
                                    Level = reader.GetInt32("Level"),
                                    SortOrder = reader.GetInt32("SortOrder"),
                                    IsActive = reader.GetBoolean("IsActive"),
                                    CreatedAt = reader.GetDateTime("CreatedAt"),
                                    UpdatedAt = reader.GetDateTime("UpdatedAt"),
                                    IconPath = reader.IsDBNull("IconPath") ? null : reader.GetString("IconPath"),
                                    Color = reader.IsDBNull("Color") ? null : reader.GetString("Color"),
                                    ParentName = reader.IsDBNull("ParentName") ? null : reader.GetString("ParentName"),
                                    ProductCount = reader.GetInt32("ProductCount")
                                };
                                categories.Add(category);
                            }
                            return categories;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取子分类失败: {ex.Message}");
                throw new Exception($"获取子分类失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取分类树形结构
        /// </summary>
        public List<CategoryTreeNode> GetCategoryTree()
        {
            var topLevelCategories = GetTopLevelCategories();
            var tree = new List<CategoryTreeNode>();
            
            foreach (var category in topLevelCategories)
            {
                var treeNode = new CategoryTreeNode
                {
                    Id = category.Id,
                    Name = category.Name,
                    Level = category.Level,
                    IsActive = category.IsActive,
                    ProductCount = category.ProductCount
                };
                
                // 递归获取子节点
                treeNode.Children = GetCategoryTreeRecursive(category.Id);
                tree.Add(treeNode);
            }
            
            return tree;
        }

        /// <summary>
        /// 递归获取分类树形结构
        /// </summary>
        private List<CategoryTreeNode> GetCategoryTreeRecursive(string parentId)
        {
            var children = GetChildCategories(parentId);
            var treeNodes = new List<CategoryTreeNode>();
            
            foreach (var child in children)
            {
                var treeNode = new CategoryTreeNode
                {
                    Id = child.Id,
                    Name = child.Name,
                    Level = child.Level,
                    IsActive = child.IsActive,
                    ProductCount = child.ProductCount
                };
                
                treeNode.Children = GetCategoryTreeRecursive(child.Id);
                treeNodes.Add(treeNode);
            }
            
            return treeNodes;
        }

        /// <summary>
        /// 检查分类名称是否已存在
        /// </summary>
        public bool CategoryNameExists(string name, string excludeId = null)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = "SELECT COUNT(*) FROM Categories WHERE Name = @Name AND IsActive = TRUE";
                    
                    if (!string.IsNullOrEmpty(excludeId))
                    {
                        query += " AND Id != @ExcludeId";
                    }
                    
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", name);
                        if (!string.IsNullOrEmpty(excludeId))
                        {
                            command.Parameters.AddWithValue("@ExcludeId", excludeId);
                        }
                        
                        var count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"检查分类名称是否存在失败: {ex.Message}");
                throw new Exception($"检查分类名称失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取分类统计信息
        /// </summary>
        public CategoryStatistics GetCategoryStatistics()
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    var statistics = new CategoryStatistics();
                    
                    // 总分类数
                    var totalQuery = "SELECT COUNT(*) FROM Categories WHERE IsActive = TRUE";
                    using (var command = new MySqlCommand(totalQuery, connection))
                    {
                        statistics.TotalCategories = Convert.ToInt32(command.ExecuteScalar());
                    }
                    
                    // 活跃分类数（总分类数）
                    statistics.ActiveCategories = statistics.TotalCategories;
                    
                    // 包含商品的分类数
                    var withProductsQuery = @"
                        SELECT COUNT(DISTINCT c.Id) 
                        FROM Categories c 
                        INNER JOIN Products p ON p.Category = c.Name 
                        WHERE c.IsActive = TRUE";
                    using (var command = new MySqlCommand(withProductsQuery, connection))
                    {
                        statistics.CategoriesWithProducts = Convert.ToInt32(command.ExecuteScalar());
                    }
                    
                    // 各分类的商品数量分布
                    var distributionQuery = @"
                        SELECT c.Name, COUNT(p.ProductCode) as ProductCount
                        FROM Categories c
                        LEFT JOIN Products p ON p.Category = c.Name
                        WHERE c.IsActive = TRUE
                        GROUP BY c.Id, c.Name
                        ORDER BY ProductCount DESC";
                    
                    using (var command = new MySqlCommand(distributionQuery, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var categoryName = reader.GetString("Name");
                            var productCount = reader.GetInt32("ProductCount");
                            statistics.ProductDistribution[categoryName] = productCount;
                        }
                    }
                    
                    return statistics;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取分类统计信息失败: {ex.Message}");
                throw new Exception($"获取分类统计信息失败: {ex.Message}", ex);
            }
        }
    }
}