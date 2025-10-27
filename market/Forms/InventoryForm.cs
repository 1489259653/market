using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using market.Models;
using market.Services;

namespace market.Forms
{
    /// <summary>
    /// 库存管理表单
    /// </summary>
    public class InventoryForm : Form
    {
        private readonly ProductService _productService;
        private readonly InventoryService _inventoryService;
        private TabControl _tabControl;

        /// <summary>
        /// 构造函数
        /// </summary>
        public InventoryForm()
        {
            // 初始化表单
            this.Text = "库存管理";
            this.Size = new Size(800, 600);
            
            // 创建TabControl
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Name = "_tabControl"
            };
            
            // 添加4个选项卡
            for (int i = 0; i < 4; i++)
            {
                _tabControl.TabPages.Add(new TabPage());
            }
            
            // 添加TabControl到表单
            this.Controls.Add(_tabControl);
            
            // 初始化服务
            _productService = new ProductService(new DatabaseService());
            _inventoryService = new InventoryService(new DatabaseService());
            
            // 绑定事件
            _tabControl.SelectedIndexChanged += _tabControl_SelectedIndexChanged;
            this.Load += InventoryForm_Load;
        }

        /// <summary>
        /// 设置默认选中的选项卡
        /// </summary>
        /// <param name="tabIndex">选项卡索引：0-库存查询，1-库存预警，2-库存历史，3-库存统计</param>
        public void SetDefaultTab(int tabIndex)
        {
            if (_tabControl != null && tabIndex >= 0 && tabIndex < _tabControl.TabPages.Count)
            {
                _tabControl.SelectedIndex = tabIndex;
            }
        }

        /// <summary>
        /// 加载表单
        /// </summary>
        private void InventoryForm_Load(object sender, EventArgs e)
        {
            InitializeTabPages();
            LoadInventoryData();
            LoadStockAlerts();
            LoadInventoryHistory();
            LoadCategoryStockSummary();
        }

        /// <summary>
        /// 初始化选项卡页面
        /// </summary>
        private void InitializeTabPages()
        {
            // 库存查询选项卡
            _tabControl.TabPages[0].Text = "库存查询";
            SetupInventoryQueryTab();

            // 库存预警选项卡
            _tabControl.TabPages[1].Text = "库存预警";
            SetupStockAlertTab();

            // 库存历史选项卡
            _tabControl.TabPages[2].Text = "库存历史";
            SetupInventoryHistoryTab();

            // 库存统计选项卡
            _tabControl.TabPages[3].Text = "库存统计";
            SetupCategoryStockTab();
        }

        #region 库存查询功能

        private void SetupInventoryQueryTab()
        {
            // 搜索面板
            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 80, Margin = new Padding(10) };

            var label1 = new Label { Text = "搜索条件:", Location = new Point(10, 10), AutoSize = true };
            var txtSearch = new TextBox { Name = "_txtSearch", Location = new Point(80, 8), Width = 150, PlaceholderText = "商品名称或编码" };
            var cmbCategory = new ComboBox { Name = "_cmbCategory", Location = new Point(240, 8), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            var btnSearch = new Button { Text = "搜索", Location = new Point(370, 8), Width = 60 };
            var btnRefresh = new Button { Text = "刷新", Location = new Point(440, 8), Width = 60 };

            var chkShowOutOfStock = new CheckBox { Text = "显示缺货商品", Location = new Point(10, 40), AutoSize = true };
            var chkShowExpired = new CheckBox { Text = "显示过期商品", Location = new Point(150, 40), AutoSize = true };

            searchPanel.Controls.AddRange(new Control[] { label1, txtSearch, cmbCategory, btnSearch, btnRefresh, chkShowOutOfStock, chkShowExpired });

            // 数据表格
            var dgvInventory = new DataGridView
            {
                Name = "_dgvInventory",
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            dgvInventory.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { DataPropertyName = "ProductCode", HeaderText = "商品编码", Width = 100 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "商品名称", Width = 150 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Category", HeaderText = "分类", Width = 80 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "库存数量", Width = 80 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Unit", HeaderText = "单位", Width = 60 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Price", HeaderText = "单价", Width = 80, DefaultCellStyle = { Format = "F2" } },
                new DataGridViewTextBoxColumn { DataPropertyName = "StockAlertThreshold", HeaderText = "预警阈值", Width = 80 },
                new DataGridViewTextBoxColumn { DataPropertyName = "StockStatusText", HeaderText = "库存状态", Width = 100 },
                new DataGridViewTextBoxColumn { DataPropertyName = "ExpiryStatusText", HeaderText = "过期状态", Width = 120 }
            });

            // 添加到选项卡
            _tabControl.TabPages[0].Controls.Add(dgvInventory);
            _tabControl.TabPages[0].Controls.Add(searchPanel);

            // 添加事件
            btnSearch.Click += (s, e) => LoadInventoryData(txtSearch.Text, cmbCategory.Text, chkShowOutOfStock.Checked, chkShowExpired.Checked);
            btnRefresh.Click += (s, e) => LoadInventoryData();

            // 加载分类
            var categories = _productService.GetCategories();
            cmbCategory.Items.AddRange(categories.ToArray());
            cmbCategory.Items.Insert(0, "全部");
            cmbCategory.SelectedIndex = 0;
        }

        private void LoadInventoryData(string searchKeyword = "", string category = "全部", bool showOutOfStock = true, bool showExpired = true)
        {
            try
            {
                var dgvInventory = (DataGridView)_tabControl.TabPages[0].Controls["_dgvInventory"];
                var products = _productService.GetAllProducts();

                // 应用筛选条件
                var filteredProducts = products;

                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    filteredProducts = filteredProducts.Where(p => 
                        p.Name.Contains(searchKeyword) || p.ProductCode.Contains(searchKeyword)).ToList();
                }

                if (category != "全部")
                {
                    filteredProducts = filteredProducts.Where(p => p.Category == category).ToList();
                }

                if (!showOutOfStock)
                {
                    filteredProducts = filteredProducts.Where(p => p.Quantity > 0).ToList();
                }

                if (!showExpired && filteredProducts.Count > 0)
                {
                    filteredProducts = filteredProducts.Where(p => !p.IsExpired).ToList();
                }

                dgvInventory.DataSource = filteredProducts;

                // 设置单元格样式
                foreach (DataGridViewRow row in dgvInventory.Rows)
                {
                    var product = row.DataBoundItem as Product;
                    if (product != null)
                    {
                        // 库存状态样式
                        if (product.AlertLevel == StockAlertLevel.OutOfStock)
                            row.DefaultCellStyle.BackColor = Color.LightSalmon;
                        else if (product.AlertLevel == StockAlertLevel.Critical)
                            row.DefaultCellStyle.BackColor = Color.LemonChiffon;

                        // 过期样式
                        if (product.IsExpired)
                        {
                            row.Cells["ExpiryStatusText"].Style.ForeColor = Color.Red;
                            row.Cells["ExpiryStatusText"].Style.Font = new Font(dgvInventory.Font, FontStyle.Bold);
                        }
                        else if (product.IsExpiringSoon)
                        {
                            row.Cells["ExpiryStatusText"].Style.ForeColor = Color.Orange;
                        }
                    }
                }

                // 更新统计信息
                var totalProducts = filteredProducts.Count;
                var totalValue = filteredProducts.Sum(p => p.Price * p.Quantity);
                _tabControl.TabPages[0].Text = $"库存查询 ({totalProducts} 种商品，总价值: ¥{totalValue:F2})";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载库存数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region 库存预警功能

        private void SetupStockAlertTab()
        {
            var dgvAlerts = new DataGridView
            {
                Name = "_dgvAlerts",
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            dgvAlerts.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { DataPropertyName = "ProductCode", HeaderText = "商品编码", Width = 100 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "商品名称", Width = 150 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Category", HeaderText = "分类", Width = 80 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "库存数量", Width = 80 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Unit", HeaderText = "单位", Width = 60 },
                new DataGridViewTextBoxColumn { DataPropertyName = "StockAlertThreshold", HeaderText = "预警阈值", Width = 80 },
                new DataGridViewTextBoxColumn { DataPropertyName = "SupplierName", HeaderText = "供应商", Width = 120 },
                new DataGridViewTextBoxColumn { DataPropertyName = "StockStatusText", HeaderText = "预警状态", Width = 100 }
            });

            _tabControl.TabPages[1].Controls.Add(dgvAlerts);
        }

        private void LoadStockAlerts()
        {
            try
            {
                var dgvAlerts = (DataGridView)_tabControl.TabPages[1].Controls["_dgvAlerts"];
                var alertProducts = _productService.GetStockAlertProducts();
                dgvAlerts.DataSource = alertProducts;

                // 设置预警样式
                foreach (DataGridViewRow row in dgvAlerts.Rows)
                {
                    var product = row.DataBoundItem as Product;
                    if (product != null)
                    {
                        switch (product.AlertLevel)
                        {
                            case StockAlertLevel.OutOfStock:
                                row.DefaultCellStyle.BackColor = Color.LightCoral;
                                break;
                            case StockAlertLevel.Critical:
                                row.DefaultCellStyle.BackColor = Color.LightYellow;
                                break;
                            case StockAlertLevel.Low:
                                row.DefaultCellStyle.BackColor = Color.LightBlue;
                                break;
                        }
                    }
                }

                _tabControl.TabPages[1].Text = $"库存预警 ({alertProducts.Count} 种商品)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载库存预警失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region 库存历史功能

        private void SetupInventoryHistoryTab()
        {
            // 筛选面板
            var filterPanel = new Panel { Dock = DockStyle.Top, Height = 100, Margin = new Padding(10) };

            var label1 = new Label { Text = "商品编码:", Location = new Point(10, 10), AutoSize = true };
            var txtProductCode = new TextBox { Name = "_txtHistoryProductCode", Location = new Point(80, 8), Width = 120 };
            
            var label2 = new Label { Text = "开始日期:", Location = new Point(220, 10), AutoSize = true };
            var dtpStartDate = new DateTimePicker { Name = "_dtpStartDate", Location = new Point(280, 8), Width = 150, Format = DateTimePickerFormat.Short };
            dtpStartDate.Value = DateTime.Now.AddDays(-30);

            var label3 = new Label { Text = "结束日期:", Location = new Point(450, 10), AutoSize = true };
            var dtpEndDate = new DateTimePicker { Name = "_dtpEndDate", Location = new Point(510, 8), Width = 150, Format = DateTimePickerFormat.Short };
            dtpEndDate.Value = DateTime.Now;

            var label4 = new Label { Text = "操作类型:", Location = new Point(10, 40), AutoSize = true };
            var cmbOperationType = new ComboBox { Name = "_cmbOperationType", Location = new Point(80, 40), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbOperationType.Items.AddRange(new string[] { "全部", "进货", "销售", "退货" });
            cmbOperationType.SelectedIndex = 0;

            var btnQueryHistory = new Button { Text = "查询", Location = new Point(220, 40), Width = 60 };
            var btnRefreshHistory = new Button { Text = "刷新", Location = new Point(290, 40), Width = 60 };

            filterPanel.Controls.AddRange(new Control[] 
            { 
                label1, txtProductCode, label2, dtpStartDate, label3, dtpEndDate,
                label4, cmbOperationType, btnQueryHistory, btnRefreshHistory 
            });

            // 历史记录表格
            var dgvHistory = new DataGridView
            {
                Name = "_dgvHistory",
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            dgvHistory.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { DataPropertyName = "OperationDate", HeaderText = "操作时间", Width = 130 },
                new DataGridViewTextBoxColumn { DataPropertyName = "ProductCode", HeaderText = "商品编码", Width = 100 },
                new DataGridViewTextBoxColumn { DataPropertyName = "QuantityChange", HeaderText = "数量变动", Width = 90 },
                new DataGridViewTextBoxColumn { DataPropertyName = "OperationType", HeaderText = "操作类型", Width = 80 },
                new DataGridViewTextBoxColumn { DataPropertyName = "OperatorId", HeaderText = "操作人", Width = 80 },
                new DataGridViewTextBoxColumn { DataPropertyName = "OrderNumber", HeaderText = "订单号", Width = 120 },
                new DataGridViewTextBoxColumn { DataPropertyName = "PurchasePrice", HeaderText = "进货价", Width = 80, DefaultCellStyle = { Format = "F2" } }
            });

            _tabControl.TabPages[2].Controls.Add(dgvHistory);
            _tabControl.TabPages[2].Controls.Add(filterPanel);

            // 添加事件
            btnQueryHistory.Click += (s, e) => LoadInventoryHistory(
                txtProductCode.Text,
                dtpStartDate.Value,
                dtpEndDate.Value,
                cmbOperationType.SelectedIndex > 0 ? cmbOperationType.Text : null
            );
            btnRefreshHistory.Click += (s, e) => LoadInventoryHistory();
        }

        private void LoadInventoryHistory(string productCode = null, DateTime? startDate = null, DateTime? endDate = null, string operationType = null)
        {
            try
            {
                var dgvHistory = (DataGridView)_tabControl.TabPages[2].Controls["_dgvHistory"];
                var historyList = _inventoryService.GetInventoryHistory(productCode, startDate, endDate, operationType);
                dgvHistory.DataSource = historyList;

                // 设置数量变动样式
                foreach (DataGridViewRow row in dgvHistory.Rows)
                {
                    var quantityChange = Convert.ToInt32(row.Cells["QuantityChange"].Value);
                    if (quantityChange > 0)
                        row.Cells["QuantityChange"].Style.ForeColor = Color.Green;
                    else if (quantityChange < 0)
                        row.Cells["QuantityChange"].Style.ForeColor = Color.Red;
                }

                _tabControl.TabPages[2].Text = $"库存历史 ({historyList.Count} 条记录)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载库存历史失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region 库存统计功能

        private void SetupCategoryStockTab()
        {
            var dgvSummary = new DataGridView
            {
                Name = "_dgvSummary",
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            dgvSummary.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { DataPropertyName = "Category", HeaderText = "商品分类", Width = 100 },
                new DataGridViewTextBoxColumn { DataPropertyName = "ProductCount", HeaderText = "商品种类数", Width = 100 },
                new DataGridViewTextBoxColumn { DataPropertyName = "TotalQuantity", HeaderText = "总数量", Width = 100 },
                new DataGridViewTextBoxColumn { DataPropertyName = "TotalValue", HeaderText = "总价值", Width = 120, DefaultCellStyle = { Format = "F2" } }
            });

            _tabControl.TabPages[3].Controls.Add(dgvSummary);
        }

        private void LoadCategoryStockSummary()
        {
            try
            {
                var dgvSummary = (DataGridView)_tabControl.TabPages[3].Controls["_dgvSummary"];
                var summaryList = _inventoryService.GetCategoryStockSummary();
                dgvSummary.DataSource = summaryList;

                // 计算总计
                var totalValue = summaryList.Sum(s => s.TotalValue);
                var totalQuantity = summaryList.Sum(s => s.TotalQuantity);
                var totalProducts = summaryList.Sum(s => s.ProductCount);

                _tabControl.TabPages[3].Text = $"库存统计 (总计: ¥{totalValue:F2}, {totalProducts} 种商品)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载库存统计失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        /// <summary>
        /// 选项卡切换时重新加载数据
        /// </summary>
        private void _tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (_tabControl.SelectedIndex)
            {
                case 0: // 库存查询
                    LoadInventoryData();
                    break;
                case 1: // 库存预警
                    LoadStockAlerts();
                    break;
                case 2: // 库存历史
                    // 保持当前筛选条件
                    break;
                case 3: // 库存统计
                    LoadCategoryStockSummary();
                    break;
            }
        }
    }
}