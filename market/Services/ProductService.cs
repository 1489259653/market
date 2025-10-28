using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using market.Models;

namespace market.Services
{
    /// <summary>
    /// 商品管理服务类
    /// </summary>
    public class ProductService
    {
        private readonly DatabaseService _databaseService;

        public ProductService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// 添加商品
        /// </summary>
        public bool AddProduct(Product product)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = @"
                        INSERT INTO Products (ProductCode, Name, Price, Quantity, Unit, Category, ExpiryDate, StockAlertThreshold, SupplierId)
                        VALUES (@ProductCode, @Name, @Price, @Quantity, @Unit, @Category, @ExpiryDate, @StockAlertThreshold, @SupplierId)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductCode", product.ProductCode);
                        command.Parameters.AddWithValue("@Name", product.Name);
                        command.Parameters.AddWithValue("@Price", product.Price);
                        command.Parameters.AddWithValue("@Quantity", product.Quantity);
                        command.Parameters.AddWithValue("@Unit", product.Unit);
                        command.Parameters.AddWithValue("@Category", product.Category);
                        command.Parameters.AddWithValue("@ExpiryDate", product.ExpiryDate.HasValue ? product.ExpiryDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
                        command.Parameters.AddWithValue("@StockAlertThreshold", product.StockAlertThreshold);
                        command.Parameters.AddWithValue("@SupplierId", string.IsNullOrEmpty(product.SupplierId) ? DBNull.Value : (object)product.SupplierId);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"添加商品失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 更新商品信息
        /// </summary>
        public bool UpdateProduct(Product product)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = @"
                        UPDATE Products 
                        SET Name = @Name, Price = @Price, Quantity = @Quantity, Unit = @Unit, 
                            Category = @Category, ExpiryDate = @ExpiryDate, StockAlertThreshold = @StockAlertThreshold, SupplierId = @SupplierId
                        WHERE ProductCode = @ProductCode";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductCode", product.ProductCode);
                        command.Parameters.AddWithValue("@Name", product.Name);
                        command.Parameters.AddWithValue("@Price", product.Price);
                        command.Parameters.AddWithValue("@Quantity", product.Quantity);
                        command.Parameters.AddWithValue("@Unit", product.Unit);
                        command.Parameters.AddWithValue("@Category", product.Category);
                        command.Parameters.AddWithValue("@ExpiryDate", product.ExpiryDate.HasValue ? product.ExpiryDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
                        command.Parameters.AddWithValue("@StockAlertThreshold", product.StockAlertThreshold);
                        command.Parameters.AddWithValue("@SupplierId", string.IsNullOrEmpty(product.SupplierId) ? DBNull.Value : (object)product.SupplierId);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"更新商品失败: {ex.Message}", ex);
            }
        }



        /// <summary>
        /// 根据商品编码获取商品
        /// </summary>
        public Product GetProductByCode(string productCode)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = @"
                        SELECT p.*, s.Name as SupplierName 
                        FROM Products p 
                        LEFT JOIN Suppliers s ON p.SupplierId = s.Id 
                        WHERE p.ProductCode = @ProductCode";

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
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                    Unit = reader.GetString(reader.GetOrdinal("Unit")),
                                    Category = reader.GetString(reader.GetOrdinal("Category")),
                                    ExpiryDate = reader.IsDBNull(reader.GetOrdinal("ExpiryDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("ExpiryDate")),
                                    StockAlertThreshold = reader.GetInt32(reader.GetOrdinal("StockAlertThreshold")),
                                    SupplierId = reader.IsDBNull(reader.GetOrdinal("SupplierId")) ? null : reader.GetString(reader.GetOrdinal("SupplierId")),
                                    SupplierName = reader.IsDBNull(reader.GetOrdinal("SupplierName")) ? null : reader.GetString(reader.GetOrdinal("SupplierName"))
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
        /// 获取所有商品（含供货方信息）
        /// </summary>
        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();

            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = @"
                        SELECT p.*, s.Name as SupplierName 
                        FROM Products p 
                        LEFT JOIN Suppliers s ON p.SupplierId = s.Id 
                        ORDER BY p.Category, p.Name";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                products.Add(new Product
                                {
                                    ProductCode = reader.GetString(reader.GetOrdinal("ProductCode")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                    Unit = reader.GetString(reader.GetOrdinal("Unit")),
                                    Category = reader.GetString(reader.GetOrdinal("Category")),
                                    ExpiryDate = reader.IsDBNull(reader.GetOrdinal("ExpiryDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("ExpiryDate")),
                                    StockAlertThreshold = reader.GetInt32(reader.GetOrdinal("StockAlertThreshold")),
                                    SupplierId = reader.IsDBNull(reader.GetOrdinal("SupplierId")) ? null : reader.GetString(reader.GetOrdinal("SupplierId")),
                                    SupplierName = reader.IsDBNull(reader.GetOrdinal("SupplierName")) ? null : reader.GetString(reader.GetOrdinal("SupplierName"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取商品列表失败: {ex.Message}", ex);
            }

            return products;
        }

        /// <summary>
        /// 分页获取商品列表
        /// </summary>
        public ProductListResult GetProductsPaged(int pageIndex, int pageSize, string keyword = "", string category = "")
        {
            var result = new ProductListResult
            {
                Products = new List<Product>(),
                CurrentPage = pageIndex,
                PageSize = pageSize
            };

            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();

                    // 构建查询条件，使用参数化查询防止SQL注入
                    List<MySqlParameter> parameters = new List<MySqlParameter>();
                    string whereClause = "WHERE 1=1";
                    
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        whereClause += " AND (p.Name LIKE @Keyword OR p.ProductCode LIKE @Keyword)";
                        parameters.Add(new MySqlParameter("@Keyword", $"%{keyword}%"));
                    }
                    if (!string.IsNullOrEmpty(category))
                    {
                        whereClause += " AND p.Category = @Category";
                        parameters.Add(new MySqlParameter("@Category", category));
                    }

                    // 计算总数
                    string countQuery = $"SELECT COUNT(*) FROM Products p {whereClause}";
                    using (var countCommand = new MySqlCommand(countQuery, connection))
                    {
                        foreach (var param in parameters)
                        {
                            countCommand.Parameters.Add(param);
                        }
                        result.TotalCount = Convert.ToInt32(countCommand.ExecuteScalar());
                    }

                    // 计算总页数
                    result.TotalPages = (result.TotalCount + pageSize - 1) / pageSize;

                    // 计算分页
                    int offset = (pageIndex - 1) * pageSize;
                    string query = $@"
                        SELECT p.*, s.Name as SupplierName 
                        FROM Products p 
                        LEFT JOIN Suppliers s ON p.SupplierId = s.Id 
                        {whereClause}
                        ORDER BY p.Category, p.Name
                        LIMIT @Offset, @PageSize";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        // 添加之前的参数
                        foreach (var param in parameters)
                        {
                            command.Parameters.Add(param.Clone() as MySqlParameter);
                        }
                        // 添加分页参数
                        command.Parameters.AddWithValue("@Offset", offset);
                        command.Parameters.AddWithValue("@PageSize", pageSize);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var product = new Product
                                {
                                    ProductCode = reader.GetString(reader.GetOrdinal("ProductCode")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                    Unit = reader.GetString(reader.GetOrdinal("Unit")),
                                    Category = reader.GetString(reader.GetOrdinal("Category")),
                                    ExpiryDate = reader.IsDBNull(reader.GetOrdinal("ExpiryDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("ExpiryDate")),
                                    StockAlertThreshold = reader.GetInt32(reader.GetOrdinal("StockAlertThreshold")),
                                    SupplierId = reader.IsDBNull(reader.GetOrdinal("SupplierId")) ? null : reader.GetString(reader.GetOrdinal("SupplierId")),
                                    SupplierName = reader.IsDBNull(reader.GetOrdinal("SupplierName")) ? null : reader.GetString(reader.GetOrdinal("SupplierName"))
                                };
                                
                                // 设置其他可选字段（如果表中有这些字段）
                                if (!reader.IsDBNull(reader.GetOrdinal("Description")))
                                    product.Description = reader.GetString("Description");
                                if (!reader.IsDBNull(reader.GetOrdinal("PurchasePrice")))
                                    product.PurchasePrice = reader.GetDecimal("PurchasePrice");
                                if (!reader.IsDBNull(reader.GetOrdinal("MinimumOrderQuantity")))
                                    product.MinimumOrderQuantity = reader.GetInt32("MinimumOrderQuantity");
                                if (!reader.IsDBNull(reader.GetOrdinal("LastUpdated")))
                                    product.LastUpdated = reader.GetDateTime("LastUpdated");
                                
                                result.Products.Add(product);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"分页获取商品失败: {ex.Message}", ex);
            }

            return result;
        }

        /// <summary>
        /// 更新商品库存
        /// </summary>
        public bool UpdateProductStock(string productCode, int quantityChange, string operationType, string operatorId, string orderNumber = null)
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
                            // 更新商品库存
                            string updateQuery = "UPDATE Products SET Quantity = Quantity + @QuantityChange WHERE ProductCode = @ProductCode";
                            using (var updateCommand = new MySqlCommand(updateQuery, connection, transaction))
                            {
                                updateCommand.Parameters.AddWithValue("@ProductCode", productCode);
                                updateCommand.Parameters.AddWithValue("@QuantityChange", quantityChange);
                                updateCommand.ExecuteNonQuery();
                            }

                            // 记录库存变动历史
                            string historyQuery = @"
                                INSERT INTO InventoryHistory (Id, ProductCode, QuantityChange, OperationType, OperationDate, OperatorId, OrderNumber)
                                VALUES (@Id, @ProductCode, @QuantityChange, @OperationType, @OperationDate, @OperatorId, @OrderNumber)";
                            using (var historyCommand = new MySqlCommand(historyQuery, connection, transaction))
                            {
                                historyCommand.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                                historyCommand.Parameters.AddWithValue("@ProductCode", productCode);
                                historyCommand.Parameters.AddWithValue("@QuantityChange", quantityChange);
                                historyCommand.Parameters.AddWithValue("@OperationType", operationType);
                                historyCommand.Parameters.AddWithValue("@OperationDate", DateTime.Now);
                                historyCommand.Parameters.AddWithValue("@OperatorId", operatorId);
                                historyCommand.Parameters.AddWithValue("@OrderNumber", string.IsNullOrEmpty(orderNumber) ? DBNull.Value : (object)orderNumber);
                                historyCommand.ExecuteNonQuery();
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
                throw new Exception($"更新库存失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 检查商品编码是否已存在
        /// </summary>
        public bool ProductCodeExists(string productCode)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Products WHERE ProductCode = @ProductCode";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductCode", productCode);
                        return Convert.ToInt32(command.ExecuteScalar()) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"检查商品编码失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 生成商品编码
        /// </summary>
        public string GenerateProductCode()
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT MAX(ProductCode) FROM Products WHERE ProductCode LIKE 'S%'";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        var result = command.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            string lastCode = result.ToString();
                            if (lastCode.Length >= 9 && lastCode.StartsWith("S"))
                            {
                                if (int.TryParse(lastCode.Substring(1), out int lastNumber))
                                {
                                    return "S" + (lastNumber + 1).ToString("D8");
                                }
                            }
                        }
                        // 默认生成第一个编码
                        return "S00000001";
                    }
                }
            }
            catch (Exception)
            {
                // 如果出错，返回默认编码
                return "S00000001";
            }
        }

        /// <summary>
        /// 获取所有商品分类
        /// </summary>
        public List<string> GetCategories()
        {
            var categories = new List<string> { "食品", "饮料", "日用品", "洗护用品", "生鲜", "家电", "服装", "其他" };
            
            try
            {
                // 从数据库中获取已存在的分类
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT DISTINCT Category FROM Products WHERE Category IS NOT NULL AND Category != '' ORDER BY Category";
                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string category = reader.GetString("Category");
                            if (!categories.Contains(category))
                            {
                                categories.Add(category);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 如果数据库查询失败，使用默认分类列表
            }
            
            return categories;
        }

        /// <summary>
        /// 获取所有供应商
        /// </summary>
        public List<Supplier> GetAllSuppliers()
        {
            var suppliers = new List<Supplier>();
            
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT * FROM Suppliers ORDER BY Name";
                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            suppliers.Add(new Supplier
                            {
                                Id = reader.GetString(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                ProductionLocation = reader.IsDBNull(reader.GetOrdinal("ProductionLocation")) ? "" : reader.GetString(reader.GetOrdinal("ProductionLocation")),
                                ContactInfo = $"{(reader.IsDBNull(reader.GetOrdinal("Contact")) ? "" : reader.GetString(reader.GetOrdinal("Contact")))} {(reader.IsDBNull(reader.GetOrdinal("Phone")) ? "" : reader.GetString(reader.GetOrdinal("Phone")))} {(reader.IsDBNull(reader.GetOrdinal("Email")) ? "" : reader.GetString(reader.GetOrdinal("Email")))}".Trim()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取供应商列表失败: {ex.Message}", ex);
            }
            
            return suppliers;
        }

        /// <summary>
        /// 根据ID获取供应商
        /// </summary>
        public Supplier GetSupplierById(string id)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT * FROM Suppliers WHERE Id = @Id";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Supplier
                                {
                                    Id = reader.GetString(reader.GetOrdinal("Id")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    ProductionLocation = reader.IsDBNull(reader.GetOrdinal("ProductionLocation")) ? "" : reader.GetString(reader.GetOrdinal("ProductionLocation")),
                                    ContactInfo = reader.IsDBNull(reader.GetOrdinal("Contact")) ? "" : reader.GetString(reader.GetOrdinal("Contact")),
                                    BusinessLicense = reader.IsDBNull(reader.GetOrdinal("BusinessLicense")) ? "" : reader.GetString(reader.GetOrdinal("BusinessLicense"))
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取供应商信息失败: {ex.Message}", ex);
            }
            
            return null;
        }

        /// <summary>
        /// 创建供应商
        /// </summary>
        public bool CreateSupplier(Supplier supplier)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    string query = @"INSERT INTO Suppliers (Id, Name, ProductionLocation, Contact, Phone, Email, BusinessLicense) 
                                    VALUES (@Id, @Name, @ProductionLocation, @Contact, @Phone, @Email, @BusinessLicense)";
                    
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", supplier.Id ?? Guid.NewGuid().ToString());
                        command.Parameters.AddWithValue("@Name", supplier.Name);
                        command.Parameters.AddWithValue("@ProductionLocation", string.IsNullOrEmpty(supplier.ProductionLocation) ? DBNull.Value : (object)supplier.ProductionLocation);
                        
                        // 解析联系方式
                        var contactInfo = ParseContactInfo(supplier.ContactInfo);
                        command.Parameters.AddWithValue("@Contact", contactInfo.Contact);
                        command.Parameters.AddWithValue("@Phone", contactInfo.Phone);
                        command.Parameters.AddWithValue("@Email", contactInfo.Email);
                        
                        command.Parameters.AddWithValue("@BusinessLicense", string.IsNullOrEmpty(supplier.BusinessLicense) ? DBNull.Value : (object)supplier.BusinessLicense);
                        
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"创建供应商失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 更新供应商
        /// </summary>
        public bool UpdateSupplier(Supplier supplier)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    string query = @"UPDATE Suppliers SET 
                                    Name = @Name, 
                                    ProductionLocation = @ProductionLocation, 
                                    Contact = @Contact, 
                                    Phone = @Phone, 
                                    Email = @Email, 
                                    BusinessLicense = @BusinessLicense 
                                    WHERE Id = @Id";
                    
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", supplier.Id);
                        command.Parameters.AddWithValue("@Name", supplier.Name);
                        command.Parameters.AddWithValue("@ProductionLocation", string.IsNullOrEmpty(supplier.ProductionLocation) ? DBNull.Value : (object)supplier.ProductionLocation);
                        
                        // 解析联系方式
                        var contactInfo = ParseContactInfo(supplier.ContactInfo);
                        command.Parameters.AddWithValue("@Contact", contactInfo.Contact);
                        command.Parameters.AddWithValue("@Phone", contactInfo.Phone);
                        command.Parameters.AddWithValue("@Email", contactInfo.Email);
                        
                        command.Parameters.AddWithValue("@BusinessLicense", string.IsNullOrEmpty(supplier.BusinessLicense) ? DBNull.Value : (object)supplier.BusinessLicense);
                        
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"更新供应商失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 删除供应商
        /// </summary>
        public bool DeleteSupplier(string id)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    // 检查是否有商品使用此供应商
                    string checkQuery = "SELECT COUNT(*) FROM Products WHERE SupplierId = @SupplierId";
                    using (var checkCommand = new MySqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@SupplierId", id);
                        var productCount = Convert.ToInt32(checkCommand.ExecuteScalar());
                        
                        if (productCount > 0)
                        {
                            throw new Exception("该供应商已被商品使用，无法删除");
                        }
                    }
                    
                    string deleteQuery = "DELETE FROM Suppliers WHERE Id = @Id";
                    using (var command = new MySqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"删除供应商失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 解析联系方式
        /// </summary>
        private (string Contact, string Phone, string Email) ParseContactInfo(string contactInfo)
        {
            if (string.IsNullOrEmpty(contactInfo))
                return ("", "", "");

            // 简单的解析逻辑，可以根据实际需求调整
            var parts = contactInfo.Split(' ');
            var contact = "";
            var phone = "";
            var email = "";

            foreach (var part in parts)
            {
                if (part.Contains("@"))
                    email = part;
                else if (part.Any(char.IsDigit) && part.Length >= 7)
                    phone = part;
                else
                    contact = part;
            }

            return (contact, phone, email);
        }

        // 注意：ProductListResult类已在Product模型中定义，这里不再重复定义
        
        public ProductListResult GetProductsPaged(string keyword, string category, int page, int pageSize)
        {
            var result = new ProductListResult();
            try
            {
                result.Products = new List<Product>();
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    // 构建查询条件
                    var whereClause = "WHERE 1=1";
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        whereClause += " AND (p.Name LIKE @Keyword OR p.ProductCode LIKE @Keyword)";
                    }
                    if (!string.IsNullOrEmpty(category))
                    {
                        whereClause += " AND p.Category = @Category";
                    }

                    // 获取总数
                    var countQuery = $"SELECT COUNT(*) FROM Products p {whereClause}";
                    using (var countCommand = new MySqlCommand(countQuery, connection))
                    {
                        if (!string.IsNullOrEmpty(keyword))
                        {
                            countCommand.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                        }
                        if (!string.IsNullOrEmpty(category))
                        {
                            countCommand.Parameters.AddWithValue("@Category", category);
                        }
                        
                        result.TotalCount = Convert.ToInt32(countCommand.ExecuteScalar());
                    }

                    // 获取分页数据
                    var query = $"SELECT p.*, s.Name as SupplierName FROM Products p LEFT JOIN Suppliers s ON p.SupplierId = s.Id {whereClause} ORDER BY p.Category, p.Name LIMIT @PageSize OFFSET @Offset";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PageSize", pageSize);
                        command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                        
                        if (!string.IsNullOrEmpty(keyword))
                        {
                            command.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                        }
                        if (!string.IsNullOrEmpty(category))
                        {
                            command.Parameters.AddWithValue("@Category", category);
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Products.Add(new Product
                                {
                                    ProductCode = reader.GetString(reader.GetOrdinal("ProductCode")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                    Unit = reader.GetString(reader.GetOrdinal("Unit")),
                                    Category = reader.GetString(reader.GetOrdinal("Category")),
                                    ExpiryDate = reader.IsDBNull(reader.GetOrdinal("ExpiryDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("ExpiryDate")),
                                    StockAlertThreshold = reader.GetInt32(reader.GetOrdinal("StockAlertThreshold")),
                                    SupplierId = reader.IsDBNull(reader.GetOrdinal("SupplierId")) ? null : reader.GetString(reader.GetOrdinal("SupplierId")),
                                    SupplierName = reader.IsDBNull(reader.GetOrdinal("SupplierName")) ? null : reader.GetString(reader.GetOrdinal("SupplierName"))
                                });
                            }
                        }
                    }
                }

                result.CurrentPage = page;
                result.PageSize = pageSize;
                result.TotalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"获取商品列表失败: {ex.Message}", ex);
            }

            return result;
        }

        /// <summary>
        /// 删除商品
        /// </summary>
        public bool DeleteProduct(string productCode)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = "DELETE FROM Products WHERE ProductCode = @ProductCode";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductCode", productCode);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"删除商品失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 搜索商品
        /// </summary>
        public List<Product> SearchProducts(string keyword, string category = null)
        {
            var products = new List<Product>();

            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    
                    var query = @"
                        SELECT p.*, s.Name as SupplierName 
                        FROM Products p 
                        LEFT JOIN Suppliers s ON p.SupplierId = s.Id 
                        WHERE p.Name LIKE @Keyword";
                    if (!string.IsNullOrEmpty(category))
                    {
                        query += " AND p.Category = @Category";
                    }
                    query += " ORDER BY p.Name";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                        if (!string.IsNullOrEmpty(category))
                        {
                            command.Parameters.AddWithValue("@Category", category);
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                products.Add(new Product
                                {
                                    ProductCode = reader.GetString(reader.GetOrdinal("ProductCode")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                    Unit = reader.GetString(reader.GetOrdinal("Unit")),
                                    Category = reader.GetString(reader.GetOrdinal("Category")),
                                    ExpiryDate = reader.IsDBNull(reader.GetOrdinal("ExpiryDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("ExpiryDate")),
                                    StockAlertThreshold = reader.GetInt32(reader.GetOrdinal("StockAlertThreshold")),
                                    SupplierId = reader.IsDBNull(reader.GetOrdinal("SupplierId")) ? null : reader.GetString(reader.GetOrdinal("SupplierId")),
                                    SupplierName = reader.IsDBNull(reader.GetOrdinal("SupplierName")) ? null : reader.GetString(reader.GetOrdinal("SupplierName"))
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
        /// 获取库存预警商品
        /// </summary>
        public List<Product> GetStockAlertProducts()
        {
            var products = new List<Product>();

            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = @"
                        SELECT p.*, s.Name as SupplierName 
                        FROM Products p 
                        LEFT JOIN Suppliers s ON p.SupplierId = s.Id 
                        WHERE p.Quantity <= p.StockAlertThreshold ORDER BY p.Quantity";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                products.Add(new Product
                                {
                                    ProductCode = reader.GetString(reader.GetOrdinal("ProductCode")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                    Unit = reader.GetString(reader.GetOrdinal("Unit")),
                                    Category = reader.GetString(reader.GetOrdinal("Category")),
                                    ExpiryDate = reader.IsDBNull(reader.GetOrdinal("ExpiryDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("ExpiryDate")),
                                    StockAlertThreshold = reader.GetInt32(reader.GetOrdinal("StockAlertThreshold")),
                                    SupplierId = reader.IsDBNull(reader.GetOrdinal("SupplierId")) ? null : reader.GetString(reader.GetOrdinal("SupplierId")),
                                    SupplierName = reader.IsDBNull(reader.GetOrdinal("SupplierName")) ? null : reader.GetString(reader.GetOrdinal("SupplierName"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取库存预警商品失败: {ex.Message}", ex);
            }

            return products;
        }




    }
}