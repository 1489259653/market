using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using market.Services;
using OfficeOpenXml;

namespace market.Forms
{
    public partial class SalesStatisticsForm : Form
    {
        private readonly SaleService _saleService;
        private readonly DatabaseService _databaseService;

        // 用于存储统计数据的临时表
        private DataTable salesTrendData = new DataTable();
        private DataTable topProductsData = new DataTable();
        private DataTable categoryDistributionData = new DataTable();
        private DataTable slowMovingProductsData = new DataTable();

        public SalesStatisticsForm(SaleService saleService, DatabaseService databaseService)
        {
            InitializeComponent();
            _saleService = saleService;
            _databaseService = databaseService;
            InitializeDataTables();
            LoadDefaultData();
        }

        private void InitializeDataTables()
        {
            // 初始化销售趋势数据表
            salesTrendData.Columns.Add("日期", typeof(string));
            salesTrendData.Columns.Add("销售额", typeof(decimal));
            
            // 初始化热门商品数据表
            topProductsData.Columns.Add("商品名称", typeof(string));
            topProductsData.Columns.Add("销量", typeof(int));
            
            // 初始化类别分布数据表
            categoryDistributionData.Columns.Add("类别名称", typeof(string));
            categoryDistributionData.Columns.Add("销售额", typeof(decimal));
            categoryDistributionData.Columns.Add("占比", typeof(string));
            
            // 初始化滞销商品数据表
            slowMovingProductsData.Columns.Add("商品名称", typeof(string));
            slowMovingProductsData.Columns.Add("库存数量", typeof(int));
        }

        private void LoadDefaultData()
        {
            // 默认加载最近30天的数据
            dtpStartDate.Value = DateTime.Now.AddDays(-30);
            dtpEndDate.Value = DateTime.Now;
            LoadStatistics();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            try
            {
                LoadSalesTrend();
                LoadTopProducts();
                LoadCategoryDistribution();
                LoadSlowMovingProducts();
                UpdateSummaryInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载统计数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSalesTrend()
        {
            salesTrendData.Clear();
            
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT 
                        DATE(so.OrderDate) as sale_date, 
                        SUM(oi.Amount) as daily_sales 
                    FROM 
                        saleorderitems oi
                    JOIN
                        saleOrders so ON oi.OrderNumber = so.OrderNumber
                    WHERE 
                        so.OrderDate BETWEEN @startDate AND @endDate 
                    GROUP BY 
                        DATE(so.OrderDate) 
                    ORDER BY 
                        sale_date;
                ";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@startDate", dtpStartDate.Value.Date);
                    command.Parameters.AddWithValue("@endDate", dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1));
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime date = reader.GetDateTime("sale_date");
                            decimal sales = reader.GetDecimal("daily_sales");
                            salesTrendData.Rows.Add(date.ToString("yyyy-MM-dd"), sales);
                        }
                    }
                }
            }
            
            // 绑定到DataGridView
            chartSalesTrend.DataSource = salesTrendData;
            chartSalesTrend.Columns["日期"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            chartSalesTrend.Columns["销售额"].DefaultCellStyle.Format = "C2";
        }

        private void LoadTopProducts()
        {
            topProductsData.Clear();
            
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT 
                        p.Name as product_name, 
                        SUM(oi.Quantity) as total_quantity 
                    FROM 
                        saleorderitems oi
                    JOIN 
                        Products p ON oi.ProductCode = p.ProductCode
                    JOIN 
                        saleOrders so ON oi.OrderNumber = so.OrderNumber
                    WHERE 
                        so.OrderDate BETWEEN @startDate AND @endDate
                    GROUP BY 
                        p.ProductCode, p.Name
                    ORDER BY 
                        total_quantity DESC
                    LIMIT 10;
                ";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@startDate", dtpStartDate.Value.Date);
                    command.Parameters.AddWithValue("@endDate", dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1));
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string productName = reader.GetString("product_name");
                            int quantity = reader.GetInt32("total_quantity");
                            topProductsData.Rows.Add(productName, quantity);
                        }
                    }
                }
            }
            
            // 绑定到DataGridView
            chartTopProducts.DataSource = topProductsData;
            chartTopProducts.Columns["商品名称"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            chartTopProducts.Columns["销量"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }

        private void LoadCategoryDistribution()
        {
            categoryDistributionData.Clear();
            decimal totalSales = 0;
            List<object[]> categoryData = new List<object[]>();
            
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT 
                        p.Category as category_name, 
                        SUM(oi.Amount) as category_sales
                    FROM 
                        saleorderitems oi
                    JOIN 
                        Products p ON oi.ProductCode = p.ProductCode
                    JOIN 
                        saleOrders so ON oi.OrderNumber = so.OrderNumber
                    WHERE 
                        so.OrderDate BETWEEN @startDate AND @endDate
                    GROUP BY 
                        p.Category
                    ORDER BY 
                        category_sales DESC;
                ";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@startDate", dtpStartDate.Value.Date);
                    command.Parameters.AddWithValue("@endDate", dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1));
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string categoryName = reader.GetString("category_name");
                            decimal sales = reader.GetDecimal("category_sales");
                            totalSales += sales;
                            categoryData.Add(new object[] { categoryName, sales });
                        }
                    }
                }
            }
            
            // 计算占比并添加到DataTable
            foreach (var data in categoryData)
            {
                string categoryName = (string)data[0];
                decimal sales = (decimal)data[1];
                string percentage = totalSales > 0 ? $"{((sales / totalSales) * 100):N2}%" : "0%";
                categoryDistributionData.Rows.Add(categoryName, sales, percentage);
            }
            
            // 绑定到DataGridView
            chartCategoryDistribution.DataSource = categoryDistributionData;
            chartCategoryDistribution.Columns["类别名称"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            chartCategoryDistribution.Columns["销售额"].DefaultCellStyle.Format = "C2";
        }

        private void LoadSlowMovingProducts()
        {
            slowMovingProductsData.Clear();
            
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT 
                        p.Name as product_name,
                        p.Quantity as stock_quantity
                    FROM 
                        Products p
                    LEFT JOIN 
                        saleorderitems oi ON p.ProductCode = oi.ProductCode
                    LEFT JOIN 
                        saleOrders so ON oi.OrderNumber = so.OrderNumber
                        AND so.OrderDate BETWEEN @startDate AND @endDate
                    WHERE 
                        p.Quantity > 0
                    GROUP BY 
                        p.ProductCode, p.Name, p.Quantity
                    HAVING 
                        COUNT(oi.Id) = 0
                    ORDER BY 
                        p.Quantity DESC
                    LIMIT 10;
                ";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@startDate", dtpStartDate.Value.Date);
                    command.Parameters.AddWithValue("@endDate", dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1));
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string productName = reader.GetString("product_name");
                            int stock = reader.GetInt32("stock_quantity");
                            slowMovingProductsData.Rows.Add(productName, stock);
                        }
                    }
                }
            }
            
            // 绑定到DataGridView
            chartSlowMoving.DataSource = slowMovingProductsData;
            chartSlowMoving.Columns["商品名称"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            chartSlowMoving.Columns["库存数量"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }

        private void UpdateSummaryInfo()
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                
                // 统计总销售额
                string salesQuery = @"
                    SELECT SUM(oi.Amount) FROM saleorderitems oi
                    JOIN saleOrders so ON oi.OrderNumber = so.OrderNumber
                    WHERE so.OrderDate BETWEEN @startDate AND @endDate;
                ";
                using (var command = new MySqlCommand(salesQuery, connection))
                {
                    command.Parameters.AddWithValue("@startDate", dtpStartDate.Value.Date);
                    command.Parameters.AddWithValue("@endDate", dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1));
                    
                    object result = command.ExecuteScalar();
                    decimal totalSales = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                    lblTotalSales.Text = $"总销售额: ¥{totalSales:N2}";
                }
                
                // 统计订单数量
                string orderQuery = @"
                    SELECT COUNT(*) FROM saleOrders 
                    WHERE OrderDate BETWEEN @startDate AND @endDate;
                ";
                using (var command = new MySqlCommand(orderQuery, connection))
                {
                    command.Parameters.AddWithValue("@startDate", dtpStartDate.Value.Date);
                    command.Parameters.AddWithValue("@endDate", dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1));
                    
                    int orderCount = Convert.ToInt32(command.ExecuteScalar());
                    lblOrderCount.Text = $"订单数量: {orderCount}";
                }
                
                // 统计销售商品种类
                string productQuery = @"
                    SELECT COUNT(DISTINCT oi.ProductCode) FROM saleorderitems oi
                    JOIN saleOrders so ON oi.OrderNumber = so.OrderNumber
                    WHERE so.OrderDate BETWEEN @startDate AND @endDate;
                ";
                using (var command = new MySqlCommand(productQuery, connection))
                {
                    command.Parameters.AddWithValue("@startDate", dtpStartDate.Value.Date);
                    command.Parameters.AddWithValue("@endDate", dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1));
                    
                    int productCount = Convert.ToInt32(command.ExecuteScalar());
                    lblProductCount.Text = $"销售商品种类: {productCount}";
                }
            }
        }

        private void btnExportPDF_Click(object sender, EventArgs e)
        {
            MessageBox.Show("PDF导出功能即将实现", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Excel文件 (*.xlsx)|*.xlsx";
            saveDialog.Title = "导出销售统计数据";
            saveDialog.FileName = $"销售统计_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (ExcelPackage package = new ExcelPackage())
                    {
                        // 创建销售趋势工作表
                        CreateSalesTrendSheet(package);
                        
                        // 创建商品销售排行工作表
                        CreateTopProductsSheet(package);
                        
                        // 创建类别分布工作表
                        CreateCategoryDistributionSheet(package);
                        
                        // 创建滞销商品工作表
                        CreateSlowMovingSheet(package);
                        
                        // 保存Excel文件
                        System.IO.File.WriteAllBytes(saveDialog.FileName, package.GetAsByteArray());
                        
                        if (MessageBox.Show("导出成功，是否打开文件？", "成功", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(saveDialog.FileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CreateSalesTrendSheet(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets.Add("销售趋势");
            worksheet.Cells["A1"].Value = "日期";
            worksheet.Cells["B1"].Value = "销售额";
            
            int row = 2;
            foreach (DataRow dr in salesTrendData.Rows)
            {
                worksheet.Cells[$"A{row}"].Value = dr["日期"];
                worksheet.Cells[$"B{row}"].Value = dr["销售额"];
                row++;
            }
            
            // 设置格式
            worksheet.Column(1).AutoFit();
            worksheet.Column(2).Style.Numberformat.Format = "¥#,##0.00";
        }

        private void CreateTopProductsSheet(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets.Add("商品销售排行");
            worksheet.Cells["A1"].Value = "商品名称";
            worksheet.Cells["B1"].Value = "销量";
            
            int row = 2;
            foreach (DataRow dr in topProductsData.Rows)
            {
                worksheet.Cells[$"A{row}"].Value = dr["商品名称"];
                worksheet.Cells[$"B{row}"].Value = dr["销量"];
                row++;
            }
            
            worksheet.Column(1).AutoFit();
        }

        private void CreateCategoryDistributionSheet(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets.Add("类别分布");
            
            // 直接准备饼图所需的数据，不显示表格
            int row = 1;
            foreach (DataRow dr in categoryDistributionData.Rows)
            {
                worksheet.Cells[$"A{row}"].Value = dr["类别名称"];
                worksheet.Cells[$"B{row}"].Value = dr["销售额"];
                row++;
            }
            
            // 隐藏数据列，只显示饼图
            worksheet.Column(1).Hidden = true;
            worksheet.Column(2).Hidden = true;
            
            // 创建饼图（使用EPPlus 7.x API）
            var pieChart = worksheet.Drawings.AddChart("类别分布饼图", OfficeOpenXml.Drawing.Chart.eChartType.Pie);
            
            // 设置图表标题
            pieChart.Title.Text = "销售类别分布";
            
            // 设置图表位置和大小
            pieChart.SetPosition(0, 0, 0, 0);
            pieChart.SetSize(600, 400);
            
            // 添加数据系列
            var dataRange = worksheet.Cells[$"B1:B{row-1}"];
            var categoriesRange = worksheet.Cells[$"A1:A{row-1}"];
            var series = pieChart.Series.Add(dataRange, categoriesRange);
            
            // 设置图例位置
            pieChart.Legend.Position = OfficeOpenXml.Drawing.Chart.eLegendPosition.Bottom;
        }

        private void CreateSlowMovingSheet(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets.Add("滞销商品");
            worksheet.Cells["A1"].Value = "商品名称";
            worksheet.Cells["B1"].Value = "库存数量";
            
            int row = 2;
            foreach (DataRow dr in slowMovingProductsData.Rows)
            {
                worksheet.Cells[$"A{row}"].Value = dr["商品名称"];
                worksheet.Cells[$"B{row}"].Value = dr["库存数量"];
                row++;
            }
            
            worksheet.Column(1).AutoFit();
        }
    }
}