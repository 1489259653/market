using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using market.Models;
using market.Services;

namespace market.Forms
{
    public partial class ProductSearchForm : Form
    {
        private readonly ProductService _productService;
        private List<Product> _searchResults;
        public Product SelectedProduct { get; private set; }
        
        private TextBox txtSearch;
        private Button btnSearch;
        private DataGridView dgvProducts;
        private Button btnSelect;
        private Button btnCancel;

        public ProductSearchForm(ProductService productService)
        {
            _productService = productService;
            _searchResults = new List<Product>();
            
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // 窗体基本设置
            this.Text = "商品搜索";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // 创建主面板
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // 搜索面板
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.White
            };

            // 搜索文本框
            var lblSearch = new Label { Text = "搜索:", Location = new Point(10, 15), Size = new Size(40, 20) };
            txtSearch = new TextBox 
            { 
                Location = new Point(50, 12), 
                Size = new Size(300, 25),
                PlaceholderText = "请输入商品编码或商品名称"
            };
            txtSearch.KeyDown += TxtSearch_KeyDown;

            // 搜索按钮
            btnSearch = new Button 
            { 
                Text = "搜索", 
                Location = new Point(360, 12), 
                Size = new Size(80, 25),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White
            };
            btnSearch.Click += BtnSearch_Click;

            // 商品列表表格
            dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // 添加列
            dgvProducts.Columns.Add("ProductCode", "商品编码");
            dgvProducts.Columns.Add("Name", "商品名称");
            dgvProducts.Columns.Add("Price", "销售价格");
            dgvProducts.Columns.Add("Quantity", "库存数量");
            dgvProducts.Columns.Add("Unit", "单位");
            dgvProducts.Columns.Add("Category", "分类");

            // 格式化列
            dgvProducts.Columns["Price"].DefaultCellStyle.Format = "C2";
            dgvProducts.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvProducts.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // 双击选择
            dgvProducts.CellDoubleClick += DgvProducts_CellDoubleClick;

            // 按钮面板
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.White
            };

            // 选择按钮
            btnSelect = new Button
            {
                Text = "选择",
                Location = new Point(400, 10),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White
            };
            btnSelect.Click += BtnSelect_Click;

            // 取消按钮
            btnCancel = new Button
            {
                Text = "取消",
                Location = new Point(490, 10),
                Size = new Size(80, 30),
                BackColor = Color.Gray,
                ForeColor = Color.White
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            // 添加控件到面板
            searchPanel.Controls.AddRange(new Control[] { lblSearch, txtSearch, btnSearch });
            buttonPanel.Controls.AddRange(new Control[] { btnSelect, btnCancel });

            // 添加面板到窗体
            this.Controls.AddRange(new Control[] { dgvProducts, buttonPanel, searchPanel });

            // 默认加载一些商品
            LoadDefaultProducts();
        }

        private void LoadDefaultProducts()
        {
            try
            {
                // 加载所有商品或最近使用的商品
                _searchResults = _productService.SearchProducts("");
                DisplaySearchResults();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载商品失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchProducts();
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            SearchProducts();
        }

        private void SearchProducts()
        {
            try
            {
                var keyword = txtSearch.Text.Trim();
                if (string.IsNullOrEmpty(keyword))
                {
                    MessageBox.Show("请输入搜索关键词", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _searchResults = _productService.SearchProducts(keyword);
                DisplaySearchResults();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"搜索商品失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplaySearchResults()
        {
            dgvProducts.Rows.Clear();
            
            foreach (var product in _searchResults)
            {
                dgvProducts.Rows.Add(
                    product.ProductCode,
                    product.Name,
                    product.Price,
                    product.Quantity,
                    product.Unit,
                    product.Category
                );
            }

            if (_searchResults.Count == 0)
            {
                MessageBox.Show("未找到匹配的商品", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DgvProducts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < _searchResults.Count)
            {
                SelectedProduct = _searchResults[e.RowIndex];
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void BtnSelect_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择商品", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedIndex = dgvProducts.SelectedRows[0].Index;
            if (selectedIndex >= 0 && selectedIndex < _searchResults.Count)
            {
                SelectedProduct = _searchResults[selectedIndex];
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        /// <summary>
        /// 获取选中的商品
        /// </summary>
        public Product GetSelectedProduct()
        {
            return SelectedProduct;
        }

        /// <summary>
        /// 显示商品搜索窗体并返回选中的商品
        /// </summary>
        public static Product ShowProductSearch(ProductService productService)
        {
            using (var form = new ProductSearchForm(productService))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    return form.SelectedProduct;
                }
                return null;
            }
        }
    }
}