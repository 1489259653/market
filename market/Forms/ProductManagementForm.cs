using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using market.Models;
using market.Services;

namespace market.Forms
{
    public partial class ProductManagementForm : Form
    {
        private readonly ProductService _productService;
        private readonly AuthService _authService;
        
        // UI控件
        private DataGridView _dataGridView;
        private TextBox _txtSearch;
        private ComboBox _cmbCategory;
        private Button _btnSearch;
        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnRefresh;
        private Label _lblStatus;
        private Panel _pagingPanel;
        private Button _btnFirstPage;
        private Button _btnPrevPage;
        private Label _lblPageInfo;
        private Button _btnNextPage;
        private Button _btnLastPage;
        private ComboBox _cmbPageSize;
        
        // 分页相关
        private int _currentPage = 1;
        private int _pageSize = 20;
        private int _totalPages = 1;
        private int _totalCount = 0;
        
        public ProductManagementForm(ProductService productService, AuthService authService)
        {
            _productService = productService;
            _authService = authService;
            InitializeComponent();
            LoadCategories();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Text = "商品信息管理";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // 创建主面板
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // 创建搜索面板
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.WhiteSmoke
            };

            // 搜索文本框
            _txtSearch = new TextBox
            {
                Location = new Point(10, 15),
                Size = new Size(200, 25),
                PlaceholderText = "搜索商品名称或编码"
            };

            // 分类下拉框
            _cmbCategory = new ComboBox
            {
                Location = new Point(220, 15),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // 搜索按钮
            _btnSearch = new Button
            {
                Location = new Point(380, 15),
                Size = new Size(80, 25),
                Text = "搜索",
                BackColor = Color.SteelBlue,
                ForeColor = Color.White
            };

            // 添加按钮
            _btnAdd = new Button
            {
                Location = new Point(470, 15),
                Size = new Size(80, 25),
                Text = "添加",
                BackColor = Color.Green,
                ForeColor = Color.White
            };

            // 编辑按钮
            _btnEdit = new Button
            {
                Location = new Point(560, 15),
                Size = new Size(80, 25),
                Text = "编辑",
                BackColor = Color.Orange,
                ForeColor = Color.White
            };

            // 删除按钮
            _btnDelete = new Button
            {
                Location = new Point(650, 15),
                Size = new Size(80, 25),
                Text = "删除",
                BackColor = Color.Red,
                ForeColor = Color.White
            };

            // 刷新按钮
            _btnRefresh = new Button
            {
                Location = new Point(740, 15),
                Size = new Size(80, 25),
                Text = "刷新",
                BackColor = Color.SteelBlue,
                ForeColor = Color.White
            };

            // 状态标签
            _lblStatus = new Label
            {
                Location = new Point(830, 18),
                Size = new Size(300, 20),
                Text = "加载中..."
            };

            // 添加控件到搜索面板
            searchPanel.Controls.AddRange(new Control[] {
                _txtSearch, _cmbCategory, _btnSearch, _btnAdd, _btnEdit, _btnDelete, _btnRefresh, _lblStatus
            });

            // 创建数据表格
            _dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            // 配置数据表格列
            SetupDataGridViewColumns();

            // 创建分页面板
            _pagingPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.WhiteSmoke
            };

            // 分页控件
            _btnFirstPage = new Button { Text = "首页", Size = new Size(60, 25) };
            _btnPrevPage = new Button { Text = "上一页", Size = new Size(80, 25) };
            _lblPageInfo = new Label { Size = new Size(200, 25), TextAlign = ContentAlignment.MiddleCenter };
            _btnNextPage = new Button { Text = "下一页", Size = new Size(80, 25) };
            _btnLastPage = new Button { Text = "末页", Size = new Size(60, 25) };
            _cmbPageSize = new ComboBox { 
                Size = new Size(80, 25), 
                DropDownStyle = ComboBoxStyle.DropDownList,
                Items = { 10, 20, 50, 100 }
            };
            _cmbPageSize.SelectedItem = 20;

            // 布局分页控件
            int y = 7;
            int x = 10;
            _btnFirstPage.Location = new Point(x, y); x += 70;
            _btnPrevPage.Location = new Point(x, y); x += 90;
            _lblPageInfo.Location = new Point(x, y); x += 210;
            _btnNextPage.Location = new Point(x, y); x += 90;
            _btnLastPage.Location = new Point(x, y); x += 70;
            _pagingPanel.Controls.Add(new Label { 
                Text = "每页显示:", 
                Location = new Point(_pagingPanel.Width - 200, y), 
                Size = new Size(80, 25)
            });
            _cmbPageSize.Location = new Point(_pagingPanel.Width - 110, y);

            // 添加分页控件到分页面板
            _pagingPanel.Controls.AddRange(new Control[] {
                _btnFirstPage, _btnPrevPage, _lblPageInfo, _btnNextPage, _btnLastPage, _cmbPageSize
            });

            // 添加控件到主面板
            mainPanel.Controls.Add(_dataGridView);
            mainPanel.Controls.Add(_pagingPanel);
            mainPanel.Controls.Add(searchPanel);

            // 添加主面板到窗体
            this.Controls.Add(mainPanel);

            // 绑定事件
            BindEvents();
        }

        private void SetupDataGridViewColumns()
        {
            // 清除现有列
            _dataGridView.Columns.Clear();

            // 添加列
            _dataGridView.Columns.Add("ProductCode", "商品编码");
            _dataGridView.Columns.Add("Name", "商品名称");
            _dataGridView.Columns.Add("Price", "单价");
            _dataGridView.Columns.Add("Quantity", "库存数量");
            _dataGridView.Columns.Add("Unit", "单位");
            _dataGridView.Columns.Add("Category", "分类");
            _dataGridView.Columns.Add("ExpiryDate", "保质期");
            _dataGridView.Columns.Add("StockAlertThreshold", "库存预警");
            _dataGridView.Columns.Add("SupplierName", "供货方");
            _dataGridView.Columns.Add("StatusText", "状态");

            // 设置列属性
            _dataGridView.Columns["Price"].DefaultCellStyle.Format = "C2";
            _dataGridView.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dataGridView.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dataGridView.Columns["StockAlertThreshold"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            
            // 设置列宽
            _dataGridView.Columns["ProductCode"].Width = 120;
            _dataGridView.Columns["Price"].Width = 80;
            _dataGridView.Columns["Quantity"].Width = 80;
            _dataGridView.Columns["Unit"].Width = 60;
            _dataGridView.Columns["Category"].Width = 100;
            _dataGridView.Columns["ExpiryDate"].Width = 100;
            _dataGridView.Columns["StockAlertThreshold"].Width = 80;
            _dataGridView.Columns["StatusText"].Width = 80;
        }

        /// <summary>
        /// 绑定所有事件
        /// </summary>
        private void BindEvents()
        {
            // 按钮事件
            _btnSearch.Click += BtnSearch_Click;
            _btnAdd.Click += BtnAdd_Click;
            _btnEdit.Click += BtnEdit_Click;
            _btnDelete.Click += BtnDelete_Click;
            _btnRefresh.Click += BtnRefresh_Click;
            
            // 分页事件
            _btnFirstPage.Click += BtnFirstPage_Click;
            _btnPrevPage.Click += BtnPrevPage_Click;
            _btnNextPage.Click += BtnNextPage_Click;
            _btnLastPage.Click += BtnLastPage_Click;
            _cmbPageSize.SelectedIndexChanged += CmbPageSize_SelectedIndexChanged;
            
            // 数据表格事件
            _dataGridView.SelectionChanged += DataGridView_SelectionChanged;
            _dataGridView.CellFormatting += DataGridView_CellFormatting;
            
            // 窗体事件
            this.Resize += ProductManagementForm_Resize;
        }

        /// <summary>
        /// 加载商品分类
        /// </summary>
        private void LoadCategories()
        {
            try
            {
                // 添加默认分类选项
                _cmbCategory.Items.Clear();
                _cmbCategory.Items.Add("全部");
                
                // 添加预设分类
                string[] defaultCategories = { "食品", "日用品", "饮料", "文具", "粮油" };
                foreach (var category in defaultCategories)
                {
                    _cmbCategory.Items.Add(category);
                }
                
                // 设置默认选择
                _cmbCategory.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"加载分类失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 加载商品数据
        /// </summary>
        private void LoadProducts()
        {
            try
            {
                _lblStatus.Text = "加载中...";
                Application.DoEvents();
                
                // 获取分页参数
                string keyword = _txtSearch.Text.Trim();
                string category = _cmbCategory.SelectedIndex > 0 ? _cmbCategory.SelectedItem.ToString() : string.Empty;
                
                // 获取分页数据
                var result = _productService.GetProductsPaged(_currentPage, _pageSize, keyword, category);
                _totalCount = result.TotalCount;
                _totalPages = (int)Math.Ceiling((double)_totalCount / _pageSize);
                
                // 填充数据表格
                _dataGridView.Rows.Clear();
                foreach (var product in result.Products)
                {
                    _dataGridView.Rows.Add(
                        product.ProductCode,
                        product.Name,
                        product.Price,
                        product.Quantity,
                        product.Unit,
                        product.Category,
                        product.ExpiryDate?.ToString("yyyy-MM-dd"),
                        product.StockAlertThreshold,
                        product.SupplierName ?? "-",
                        product.StatusText
                    );
                }
                
                // 更新状态和分页信息
                _lblStatus.Text = $"共 {_totalCount} 条记录";
                UpdatePagingInfo();
                UpdatePagingButtons();
                
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"加载商品失败: {ex.Message}";
                MessageBox.Show("加载商品失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 更新分页信息
        /// </summary>
        private void UpdatePagingInfo()
        {
            _lblPageInfo.Text = $"第 {_currentPage} 页 / 共 {_totalPages} 页";
        }

        /// <summary>
        /// 更新分页按钮状态
        /// </summary>
        private void UpdatePagingButtons()
        {
            _btnFirstPage.Enabled = _currentPage > 1;
            _btnPrevPage.Enabled = _currentPage > 1;
            _btnNextPage.Enabled = _currentPage < _totalPages;
            _btnLastPage.Enabled = _currentPage < _totalPages;
        }

        /// <summary>
        /// 首页按钮点击
        /// </summary>
        private void BtnFirstPage_Click(object sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage = 1;
                LoadProducts();
            }
        }

        /// <summary>
        /// 上一页按钮点击
        /// </summary>
        private void BtnPrevPage_Click(object sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadProducts();
            }
        }

        /// <summary>
        /// 下一页按钮点击
        /// </summary>
        private void BtnNextPage_Click(object sender, EventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                LoadProducts();
            }
        }

        /// <summary>
        /// 末页按钮点击
        /// </summary>
        private void BtnLastPage_Click(object sender, EventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage = _totalPages;
                LoadProducts();
            }
        }

        /// <summary>
        /// 每页显示数量变更
        /// </summary>
        private void CmbPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            _pageSize = Convert.ToInt32(_cmbPageSize.SelectedItem);
            _currentPage = 1;
            LoadProducts();
        }

        /// <summary>
        /// 搜索按钮点击
        /// </summary>
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            _currentPage = 1;
            LoadProducts();
        }

        /// <summary>
        /// 添加按钮点击
        /// </summary>
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var productEditForm = new ProductEditForm(_productService);
                if (productEditForm.ShowDialog() == DialogResult.OK)
                {
                    // 重新加载数据
                    _currentPage = 1;
                    LoadProducts();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("打开添加商品窗口失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 编辑按钮点击
        /// </summary>
        private void BtnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (_dataGridView.SelectedRows.Count > 0)
                {
                    string productCode = _dataGridView.SelectedRows[0].Cells["ProductCode"].Value.ToString();
                    var product = _productService.GetProductByCode(productCode);
                    
                    if (product != null)
                    {
                        var productEditForm = new ProductEditForm(_productService, product);
                        if (productEditForm.ShowDialog() == DialogResult.OK)
                        {
                            LoadProducts();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("编辑商品失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 删除按钮点击
        /// </summary>
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (_dataGridView.SelectedRows.Count > 0)
                {
                    string productCode = _dataGridView.SelectedRows[0].Cells["ProductCode"].Value.ToString();
                    string productName = _dataGridView.SelectedRows[0].Cells["Name"].Value.ToString();
                    
                    // 确认删除
                    if (MessageBox.Show($"确定要删除商品 '{productName}' 吗？\n删除后将无法恢复！", 
                        "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        // 执行删除
                        bool success = _productService.DeleteProduct(productCode);
                        if (success)
                        {
                            MessageBox.Show("商品删除成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadProducts();
                        }
                        else
                        {
                            MessageBox.Show("商品删除失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("删除商品失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 刷新按钮点击
        /// </summary>
        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            _txtSearch.Clear();
            _cmbCategory.SelectedIndex = 0;
            _currentPage = 1;
            LoadProducts();
        }

        /// <summary>
        /// 数据表格选择变更
        /// </summary>
        private void DataGridView_SelectionChanged(object sender, EventArgs e)
        {
            // 编辑和删除按钮只有在选中行时才可用
            _btnEdit.Enabled = _dataGridView.SelectedRows.Count > 0;
            _btnDelete.Enabled = _dataGridView.SelectedRows.Count > 0;
        }

        /// <summary>
        /// 数据表格单元格格式化
        /// </summary>
        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && _dataGridView.Rows[e.RowIndex] != null)
            {
                var row = _dataGridView.Rows[e.RowIndex];
                
                // 检查是否需要库存预警显示
                if (e.ColumnIndex == _dataGridView.Columns["Quantity"].Index)
                {
                    if (row.Cells["StatusText"].Value != null && 
                        row.Cells["StatusText"].Value.ToString() == "库存预警")
                    {
                        e.CellStyle.BackColor = Color.LightSalmon;
                        e.CellStyle.ForeColor = Color.Red;
                    }
                }
                
                // 保质期颜色处理
                if (e.ColumnIndex == _dataGridView.Columns["ExpiryDate"].Index && 
                    e.Value != null && !string.IsNullOrEmpty(e.Value.ToString()))
                {
                    if (DateTime.TryParse(e.Value.ToString(), out DateTime expiryDate))
                    {
                        TimeSpan daysUntilExpiry = expiryDate - DateTime.Today;
                        
                        if (daysUntilExpiry.TotalDays <= 0)
                        {
                            // 已过期
                            e.CellStyle.BackColor = Color.Red;
                            e.CellStyle.ForeColor = Color.White;
                        }
                        else if (daysUntilExpiry.TotalDays <= 7)
                        {
                            // 即将过期（7天内）
                            e.CellStyle.BackColor = Color.Yellow;
                            e.CellStyle.ForeColor = Color.Black;
                        }
                    }
                }
                
                // 状态列颜色处理
                if (e.ColumnIndex == _dataGridView.Columns["StatusText"].Index)
                {
                    if (e.Value != null && e.Value.ToString() == "库存预警")
                    {
                        e.CellStyle.BackColor = Color.LightSalmon;
                        e.CellStyle.ForeColor = Color.Red;
                    }
                }
            }
        }

        /// <summary>
        /// 窗体大小变更时调整分页控件位置
        /// </summary>
        private void ProductManagementForm_Resize(object sender, EventArgs e)
        {
            // 重新调整分页控件的位置
            _pagingPanel.Controls["label1"]?.Dispose(); // 移除之前的标签
            
            int y = 7;
            _pagingPanel.Controls.Add(new Label { 
                Text = "每页显示:", 
                Location = new Point(_pagingPanel.Width - 200, y), 
                Size = new Size(80, 25)
            });
            _cmbPageSize.Location = new Point(_pagingPanel.Width - 110, y);
        }

        private void SetupDataGridView()
        {
            // 设置列宽
            _dataGridView.Columns["ProductCode"].Width = 120;
            _dataGridView.Columns["Name"].Width = 200;
            _dataGridView.Columns["Price"].Width = 80;
            _dataGridView.Columns["Quantity"].Width = 80;
            _dataGridView.Columns["Unit"].Width = 60;
            _dataGridView.Columns["Category"].Width = 100;
            _dataGridView.Columns["ExpiryDate"].Width = 100;
            _dataGridView.Columns["StockAlertThreshold"].Width = 80;
            _dataGridView.Columns["SupplierName"].Width = 150;
            _dataGridView.Columns["StatusText"].Width = 80;

            // 设置行样式
            _dataGridView.RowPrePaint += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < _dataGridView.Rows.Count)
                {
                    var row = _dataGridView.Rows[e.RowIndex];
                    var product = row.DataBoundItem as Product;
                    if (product != null && product.IsStockAlert)
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
        }

        private void SearchProducts()
        {
            try
            {
                _currentPage = 1;
                var keyword = _txtSearch.Text.Trim();
                var category = _cmbCategory.SelectedItem?.ToString();
                
                if (category == "全部")
                {
                    category = "";
                }

                var result = _productService.GetProductsPaged(_currentPage, _pageSize, keyword, category);
                
                _dataGridView.DataSource = result.Products;
                _totalPages = result.TotalPages;
                
                UpdatePagingControls();
                _lblStatus.Text = $"搜索到 {result.TotalCount} 条记录，第 {_currentPage}/{_totalPages} 页";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"搜索商品失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdatePagingControls()
        {
            _pagingPanel.Controls.Clear();

            // 上一页按钮
            var btnPrev = new Button
            {
                Text = "上一页",
                Size = new Size(80, 25),
                Location = new Point(10, 7),
                Enabled = _currentPage > 1
            };
            btnPrev.Click += (s, e) =>
            {
                if (_currentPage > 1)
                {
                    _currentPage--;
                    LoadProducts();
                }
            };

            // 下一页按钮
            var btnNext = new Button
            {
                Text = "下一页",
                Size = new Size(80, 25),
                Location = new Point(100, 7),
                Enabled = _currentPage < _totalPages
            };
            btnNext.Click += (s, e) =>
            {
                if (_currentPage < _totalPages)
                {
                    _currentPage++;
                    LoadProducts();
                }
            };

            // 页码标签
            var lblPage = new Label
            {
                Text = $"第 {_currentPage} 页，共 {_totalPages} 页",
                Location = new Point(190, 12),
                Size = new Size(150, 20)
            };

            _pagingPanel.Controls.AddRange(new Control[] { btnPrev, btnNext, lblPage });
        }

        private void AddProduct()
        {
            try
            {
                using (var form = new ProductEditForm(_productService, null))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadProducts();
                        MessageBox.Show("商品添加成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加商品失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditProduct()
        {
            try
            {
                if (_dataGridView.SelectedRows.Count == 0)
                {
                    MessageBox.Show("请选择要编辑的商品！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var selectedProduct = _dataGridView.SelectedRows[0].DataBoundItem as Product;
                if (selectedProduct == null) return;

                using (var form = new ProductEditForm(_productService, selectedProduct))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadProducts();
                        MessageBox.Show("商品修改成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"编辑商品失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteProduct()
        {
            try
            {
                if (_dataGridView.SelectedRows.Count == 0)
                {
                    MessageBox.Show("请选择要删除的商品！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var selectedProduct = _dataGridView.SelectedRows[0].DataBoundItem as Product;
                if (selectedProduct == null) return;

                var result = MessageBox.Show(
                    $"确定要删除商品 '{selectedProduct.Name}' 吗？此操作不可恢复！",
                    "确认删除",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    if (_productService.DeleteProduct(selectedProduct.ProductCode))
                    {
                        LoadProducts();
                        MessageBox.Show("商品删除成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("商品删除失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除商品失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}