using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using market.Services;
using market.Models;

namespace market.Forms
{
    public partial class MainForm : Form
    {
        private readonly AuthService _authService;
        private readonly DatabaseService _databaseService;
        private readonly ProductService _productService;
        
        private MenuStrip _mainMenu;
        private StatusStrip _statusStrip;
        private Panel _contentPanel;

        public MainForm(AuthService authService, DatabaseService databaseService)
        {
            _authService = authService;
            _databaseService = databaseService;
            _productService = new ProductService(databaseService);
            
            InitializeComponent();
            SetupMenu();
            SetupStatusBar();
            ShowWelcomeScreen();
        }

        private void InitializeComponent()
        {
            this.Text = "超市管理系统";
            this.Size = new Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // 主菜单
            _mainMenu = new MenuStrip();
            this.Controls.Add(_mainMenu);
            this.MainMenuStrip = _mainMenu;

            // 内容面板
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            this.Controls.Add(_contentPanel);

            // 状态栏
            _statusStrip = new StatusStrip();
            this.Controls.Add(_statusStrip);
        }

        private void SetupMenu()
        {
            // 文件菜单
            var fileMenu = new ToolStripMenuItem("文件(&F)");
            fileMenu.DropDownItems.Add("退出(&X)", null, (s, e) => this.Close());

            // 商品管理菜单
            var productMenu = new ToolStripMenuItem("商品管理(&P)");
            productMenu.DropDownItems.Add("商品信息管理", null, (s, e) => ShowProductManagement());
            productMenu.DropDownItems.Add("商品分类管理", null, (s, e) => ShowCategoryManagement());

            // 库存管理菜单
            var inventoryMenu = new ToolStripMenuItem("库存管理(&I)");
            inventoryMenu.DropDownItems.Add("进货管理", null, (s, e) => ShowPurchaseManagement());
            inventoryMenu.DropDownItems.Add("库存查询", null, (s, e) => ShowInventoryQuery());
            inventoryMenu.DropDownItems.Add("库存预警", null, (s, e) => ShowStockAlert());
            inventoryMenu.DropDownItems.Add("供应商管理", null, (s, e) => ShowSupplierManagement());

            // 销售管理菜单
            var salesMenu = new ToolStripMenuItem("销售管理(&S)");
            salesMenu.DropDownItems.Add("销售收银", null, (s, e) => ShowSalesCounter());
            salesMenu.DropDownItems.Add("退货处理", null, (s, e) => ShowReturnManagement());
            salesMenu.DropDownItems.Add("销售记录", null, (s, e) => ShowSalesRecords());

            // 报表菜单
            var reportMenu = new ToolStripMenuItem("报表(&R)");
            reportMenu.DropDownItems.Add("销售统计", null, (s, e) => ShowSalesReport());

            // 系统管理菜单
            var systemMenu = new ToolStripMenuItem("系统管理(&Y)");
            systemMenu.DropDownItems.Add("用户管理", null, (s, e) => ShowUserManagement());
            systemMenu.DropDownItems.Add("操作日志", null, (s, e) => ShowOperationLogs());
            systemMenu.DropDownItems.Add("数据备份", null, (s, e) => ShowBackup());

            // 添加到主菜单
            _mainMenu.Items.AddRange(new ToolStripItem[] { fileMenu, productMenu, inventoryMenu, salesMenu, reportMenu, systemMenu });

            // 根据用户权限设置菜单可见性
            UpdateMenuPermissions();
        }

        private void UpdateMenuPermissions()
        {
            var user = _authService.CurrentUser;
            if (user == null) return;

            // 根据用户角色设置菜单权限
            foreach (ToolStripMenuItem menuItem in _mainMenu.Items)
            {
                if (menuItem.Text == "系统管理(&Y)" && user.Role != Models.UserRole.Administrator)
                {
                    menuItem.Visible = false;
                }
                
                if (menuItem.Text == "商品管理(&P)" && user.Role == Models.UserRole.Cashier)
                {
                    menuItem.Visible = false;
                }

                if (menuItem.Text == "库存管理(&I)" && user.Role == Models.UserRole.Cashier)
                {
                    // 收银员只能查看库存查询
                    foreach (ToolStripItem subItem in menuItem.DropDownItems)
                    {
                        if (subItem.Text != "库存查询")
                        {
                            subItem.Visible = false;
                        }
                    }
                }
            }
        }

        private void SetupStatusBar()
        {
            var userInfoLabel = new ToolStripStatusLabel
            {
                Text = $"当前用户: {_authService.CurrentUser?.Username} - {GetRoleName(_authService.CurrentUser?.Role)}"
            };

            var timeLabel = new ToolStripStatusLabel
            {
                Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            _statusStrip.Items.AddRange(new ToolStripItem[] { userInfoLabel, timeLabel });

            // 更新时间显示
            var timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += (s, e) => timeLabel.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            timer.Start();
        }

        private string GetRoleName(Models.UserRole? role)
        {
            return role switch
            {
                Models.UserRole.Administrator => "管理员",
                Models.UserRole.Cashier => "收银员",
                Models.UserRole.WarehouseManager => "仓库管理员",
                _ => "未知角色"
            };
        }

        private void ShowWelcomeScreen()
        {
            _contentPanel.Controls.Clear();

            var welcomeLabel = new Label
            {
                Text = $"欢迎使用超市管理系统\n当前用户: {_authService.CurrentUser?.Username}",
                Font = new Font("微软雅黑", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            _contentPanel.Controls.Add(welcomeLabel);
        }

        // 商品管理功能
        private void ShowProductManagement()
        {
            try
            {
                // 检查用户权限
                if (!_authService.HasPermission("商品管理"))
                {
                    MessageBox.Show("您没有权限访问商品管理功能！", "权限不足", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var productForm = new ProductManagementForm(_productService, _authService);
                productForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开商品管理失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 库存查询功能
        private void ShowInventoryQuery()
        {
            try
            {
                // 打开库存管理表单，默认显示库存查询选项卡
                var inventoryForm = new InventoryForm();
                inventoryForm.StartPosition = FormStartPosition.CenterParent;
                inventoryForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开库存管理失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 供应商管理功能
        private void ShowSupplierManagement()
        {
            try
            {
                if (!_authService.HasPermission("供应商管理"))
                {
                    MessageBox.Show("您没有权限访问供应商管理功能！", "权限不足", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 打开库存管理表单，显示供应商管理选项卡
                var inventoryForm = new InventoryForm();
                inventoryForm.StartPosition = FormStartPosition.CenterParent;
                // 设置默认选中供应商管理选项卡（索引4）
                inventoryForm.SetDefaultTab(4);
                inventoryForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开供应商管理失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 库存预警功能
        private void ShowStockAlert()
        {
            try
            {
                // 打开库存管理表单，显示库存预警选项卡
                var inventoryForm = new InventoryForm();
                inventoryForm.StartPosition = FormStartPosition.CenterParent;
                // 设置默认选中库存预警选项卡（索引1）
                inventoryForm.SetDefaultTab(1);
                inventoryForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开库存预警失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 在网格中显示商品列表
        private void ShowProductsInGrid(System.Collections.Generic.List<Models.Product> products, string title)
        {
            _contentPanel.Controls.Clear();

            // 创建标题
            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("微软雅黑", 14, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(300, 30)
            };

            // 创建数据网格
            var dataGridView = new DataGridView
            {
                Location = new Point(10, 50),
                Size = new Size(_contentPanel.Width - 30, _contentPanel.Height - 100),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            // 配置列
            dataGridView.Columns.Add("ProductCode", "商品编码");
            dataGridView.Columns.Add("Name", "商品名称");
            dataGridView.Columns.Add("Price", "单价");
            dataGridView.Columns.Add("Quantity", "库存数量");
            dataGridView.Columns.Add("Unit", "单位");
            dataGridView.Columns.Add("Category", "分类");
            dataGridView.Columns.Add("SupplierName", "供货方");
            dataGridView.Columns.Add("StatusText", "状态");

            // 设置列格式
            dataGridView.Columns["Price"].DefaultCellStyle.Format = "C2";
            dataGridView.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // 填充数据
            foreach (var product in products)
            {
                dataGridView.Rows.Add(
                    product.ProductCode,
                    product.Name,
                    product.Price,
                    product.Quantity,
                    product.Unit,
                    product.Category,
                    product.SupplierName ?? "（无）",
                    product.StatusText
                );
            }

            // 设置行样式
            dataGridView.RowPrePaint += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < products.Count)
                {
                    var product = products[e.RowIndex];
                    var row = dataGridView.Rows[e.RowIndex];
                    
                    if (product.IsStockAlert)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightCoral;
                        row.DefaultCellStyle.ForeColor = Color.DarkRed;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.White;
                        row.DefaultCellStyle.ForeColor = Color.Black;
                    }
                }
            };

            // 添加统计信息
            var totalLabel = new Label
            {
                Text = $"共 {products.Count} 个商品",
                Location = new Point(10, _contentPanel.Height - 40),
                Size = new Size(200, 20)
            };

            _contentPanel.Controls.Add(titleLabel);
            _contentPanel.Controls.Add(dataGridView);
            _contentPanel.Controls.Add(totalLabel);
        }

        // 商品分类管理功能
        private void ShowCategoryManagement()
        {
            try
            {
                // 检查用户权限
                if (!_authService.HasPermission("商品分类管理"))
                {
                    MessageBox.Show("您没有权限访问商品分类管理功能！", "权限不足", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var categoryService = new CategoryService(_databaseService);
                var categoryForm = new CategoryManagementForm(categoryService);
                categoryForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开商品分类管理失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 进货管理功能
        private void ShowPurchaseManagement()
        {
            try
            {
                // 检查用户权限
                if (!_authService.HasPermission("进货管理"))
                {
                    MessageBox.Show("您没有权限访问进货管理功能！", "权限不足", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var purchaseForm = new market.Forms.PurchaseManagementForm(_databaseService, _authService);
                purchaseForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开进货管理失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowSalesCounter()
        {
            try
            {
                // 检查用户权限
                if (!_authService.HasPermission("销售收银"))
                {
                    MessageBox.Show("您没有权限访问销售收银功能！", "权限不足", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var saleService = new SaleService(_databaseService, _authService);
                var saleForm = new SaleCounterForm(saleService, _authService);
                saleForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开销售收银失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowReturnManagement()
        {
            try
            {
                var returnService = new ReturnService(_databaseService, _authService);
                var productService = new ProductService(_databaseService);
                var returnForm = new ReturnManagementForm(returnService, _authService, productService);
                
                // 清空内容面板并显示退货管理界面
                _contentPanel.Controls.Clear();
                returnForm.TopLevel = false;
                returnForm.FormBorderStyle = FormBorderStyle.None;
                returnForm.Dock = DockStyle.Fill;
                _contentPanel.Controls.Add(returnForm);
                returnForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开退货管理失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowSalesRecords()
        {
            try
            {
                var saleService = new SaleService(_databaseService, _authService);
                var salesRecordsForm = new SalesRecordsForm(saleService, _authService);
                
                // 清空内容面板并显示新窗体
                _contentPanel.Controls.Clear();
                salesRecordsForm.TopLevel = false;
                salesRecordsForm.FormBorderStyle = FormBorderStyle.None;
                salesRecordsForm.Dock = DockStyle.Fill;
                _contentPanel.Controls.Add(salesRecordsForm);
                salesRecordsForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开销售记录失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowSalesReport()
        {
            try
            {
                var saleService = new SaleService(_databaseService, _authService);
                var statisticsForm = new SalesStatisticsForm(saleService, _databaseService);
                
                // 清空内容面板并显示新窗体
                _contentPanel.Controls.Clear();
                statisticsForm.TopLevel = false;
                statisticsForm.FormBorderStyle = FormBorderStyle.None;
                statisticsForm.Dock = DockStyle.Fill;
                _contentPanel.Controls.Add(statisticsForm);
                statisticsForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开销售统计报表失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowUserManagement()
        {
            ShowFeatureNotImplemented("用户管理");
        }

        private void ShowOperationLogs()
        {
            _contentPanel.Controls.Clear();
            
            var logService = new LogService(_databaseService);
            var operationLogForm = new OperationLogForm(logService);
            operationLogForm.TopLevel = false;
            operationLogForm.Dock = DockStyle.Fill;
            operationLogForm.FormBorderStyle = FormBorderStyle.None;
            
            _contentPanel.Controls.Add(operationLogForm);
            operationLogForm.Show();
        }

        private void ShowBackup()
        {
            try
            {
                // 检查用户权限
                if (!_authService.HasPermission("数据备份"))
                {
                    MessageBox.Show("您没有权限访问数据备份功能！", "权限不足", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 使用完全限定名创建实例
                var backupService = new market.Services.BackupService(_databaseService);
                var backupForm = new market.Forms.BackupForm(backupService, _authService);
                
                // 清空内容面板并显示新窗体
                _contentPanel.Controls.Clear();
                backupForm.TopLevel = false;
                backupForm.FormBorderStyle = FormBorderStyle.None;
                backupForm.Dock = DockStyle.Fill;
                _contentPanel.Controls.Add(backupForm);
                backupForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开数据备份失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowFeatureNotImplemented(string featureName)
        {
            _contentPanel.Controls.Clear();

            var label = new Label
            {
                Text = $"{featureName}功能正在开发中...",
                Font = new Font("微软雅黑", 12, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            _contentPanel.Controls.Add(label);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            var result = MessageBox.Show("确定要退出系统吗？", "确认退出", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                _authService.Logout();
            }
        }
    }
}