using System;
using System.Drawing;
using System.Windows.Forms;
using market.Models;
using market.Services;

namespace market.Forms
{
    public partial class PurchaseOrderItemEditForm : Form
    {
        private readonly ProductService _productService;
        
        public PurchaseOrderItem Item { get; private set; }
        private bool _isEditMode;

        public PurchaseOrderItemEditForm(ProductService productService, PurchaseOrderItem item = null)
        {
            InitializeComponent();
            
            _productService = productService;
            Item = item;
            _isEditMode = item != null;
            
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = _isEditMode ? "编辑商品明细" : "添加商品明细";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 创建主布局
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // 商品选择
            var lblProduct = new Label { Text = "商品:", Location = new Point(10, 20), Width = 80 };
            var cmbProduct = new ComboBox { Location = new Point(100, 17), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };

            // 数量
            var lblQuantity = new Label { Text = "数量:", Location = new Point(10, 60), Width = 80 };
            var numQuantity = new NumericUpDown { Location = new Point(100, 57), Width = 120, Minimum = 1, Maximum = 10000 };

            // 进货单价
            var lblPurchasePrice = new Label { Text = "进货单价:", Location = new Point(10, 100), Width = 80 };
            var numPurchasePrice = new NumericUpDown { Location = new Point(100, 97), Width = 120, Minimum = 0, Maximum = 100000, DecimalPlaces = 2 };

            // 批次号
            var lblBatchNumber = new Label { Text = "批次号:", Location = new Point(10, 140), Width = 80 };
            var txtBatchNumber = new TextBox { Location = new Point(100, 137), Width = 200 };

            // 保质期
            var lblExpiryDate = new Label { Text = "保质期:", Location = new Point(10, 180), Width = 80 };
            var dtpExpiryDate = new DateTimePicker { Location = new Point(100, 177), Width = 120, Format = DateTimePickerFormat.Short };

            // 备注
            var lblNotes = new Label { Text = "备注:", Location = new Point(10, 220), Width = 80 };
            var txtNotes = new TextBox { Location = new Point(100, 217), Width = 250, Height = 40, Multiline = true, ScrollBars = ScrollBars.Vertical };

            // 按钮
            var btnOK = new Button { Text = "确定", Size = new Size(80, 30), Location = new Point(150, 280) };
            var btnCancel = new Button { Text = "取消", Size = new Size(80, 30), Location = new Point(250, 280) };

            mainPanel.Controls.AddRange(new Control[] {
                lblProduct, cmbProduct,
                lblQuantity, numQuantity,
                lblPurchasePrice, numPurchasePrice,
                lblBatchNumber, txtBatchNumber,
                lblExpiryDate, dtpExpiryDate,
                lblNotes, txtNotes,
                btnOK, btnCancel
            });

            this.Controls.Add(mainPanel);

            // 事件处理
            btnOK.Click += (s, e) => SaveItem();
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            // 商品选择事件
            cmbProduct.SelectedIndexChanged += (s, e) =>
            {
                if (cmbProduct.SelectedItem is Product product)
                {
                    // 自动填充商品信息
                    numPurchasePrice.Value = product.PurchasePrice > 0 ? product.PurchasePrice : product.Price * 0.8m;
                }
            };

            // 加载商品列表
            LoadProducts(cmbProduct);

            // 初始化数据
            InitializeData();
        }

        private void LoadProducts(ComboBox comboBox)
        {
            try
            {
                var products = _productService.GetAllProducts();
                comboBox.DataSource = products;
                comboBox.DisplayMember = "Name";
                comboBox.ValueMember = "ProductCode";
                comboBox.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载商品列表失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeData()
        {
            if (_isEditMode)
            {
                var mainPanel = this.Controls[0] as Panel;
                var cmbProduct = mainPanel.Controls[1] as ComboBox;
                var numQuantity = mainPanel.Controls[3] as NumericUpDown;
                var numPurchasePrice = mainPanel.Controls[5] as NumericUpDown;
                var txtBatchNumber = mainPanel.Controls[7] as TextBox;
                var dtpExpiryDate = mainPanel.Controls[9] as DateTimePicker;
                var txtNotes = mainPanel.Controls[11] as TextBox;

                // 选择商品 - 使用更安全的方法
                for (int i = 0; i < cmbProduct.Items.Count; i++)
                {
                    var product = cmbProduct.Items[i] as Product;
                    if (product != null && product.ProductCode == Item.ProductCode)
                    {
                        cmbProduct.SelectedIndex = i;
                        break;
                    }
                }
                
                // 如果没找到，设置商品信息
                if (cmbProduct.SelectedIndex == -1 && !string.IsNullOrEmpty(Item.ProductCode))
                {
                    cmbProduct.Text = Item.ProductName;
                }

                numQuantity.Value = Item.Quantity;
                numPurchasePrice.Value = Item.PurchasePrice;
                txtBatchNumber.Text = Item.BatchNumber ?? "";
                
                if (Item.ExpiryDate.HasValue)
                {
                    dtpExpiryDate.Value = Item.ExpiryDate.Value;
                }
                
                txtNotes.Text = Item.Notes ?? "";
            }
        }

        private void SaveItem()
        {
            if (!ValidateForm()) return;

            try
            {
                var mainPanel = this.Controls[0] as Panel;
                var cmbProduct = mainPanel.Controls[1] as ComboBox;
                var numQuantity = mainPanel.Controls[3] as NumericUpDown;
                var numPurchasePrice = mainPanel.Controls[5] as NumericUpDown;
                var txtBatchNumber = mainPanel.Controls[7] as TextBox;
                var dtpExpiryDate = mainPanel.Controls[9] as DateTimePicker;
                var txtNotes = mainPanel.Controls[11] as TextBox;

                Product selectedProduct = null;
                
                // 检查是否是编辑模式且商品在列表中没有找到的情况
                if (cmbProduct.SelectedItem is Product product)
                {
                    selectedProduct = product;
                }
                else if (cmbProduct.SelectedIndex == -1 && !string.IsNullOrEmpty(cmbProduct.Text))
                {
                    // 如果商品在列表中没有找到，需要验证商品是否存在
                    if (_isEditMode)
                    {
                        // 在编辑模式下，使用原来的商品信息
                        selectedProduct = new Product { ProductCode = Item.ProductCode, Name = Item.ProductName };
                    }
                    else
                    {
                        // 在新建模式下，需要检查商品是否存在
                        var existingProduct = _productService.GetProductByCode(cmbProduct.Text.Trim());
                        if (existingProduct != null)
                        {
                            selectedProduct = existingProduct;
                        }
                        else
                        {
                            MessageBox.Show("商品不存在，请输入有效的商品编码或从列表中选择", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("请选择有效的商品", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_isEditMode)
                {
                    // 更新现有明细
                    Item.ProductCode = selectedProduct.ProductCode;
                    Item.ProductName = selectedProduct.Name;
                    Item.Quantity = (int)numQuantity.Value;
                    Item.PurchasePrice = numPurchasePrice.Value;
                    Item.BatchNumber = txtBatchNumber.Text.Trim();
                    Item.ExpiryDate = dtpExpiryDate.Checked ? dtpExpiryDate.Value : (DateTime?)null;
                    Item.Notes = txtNotes.Text.Trim();
                    Item.CalculateAmount();
                }
                else
                {
                    // 创建新明细
                    Item = new PurchaseOrderItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProductCode = selectedProduct.ProductCode,
                        ProductName = selectedProduct.Name,
                        Quantity = (int)numQuantity.Value,
                        PurchasePrice = numPurchasePrice.Value,
                        BatchNumber = txtBatchNumber.Text.Trim(),
                        ExpiryDate = dtpExpiryDate.Checked ? dtpExpiryDate.Value : (DateTime?)null,
                        Notes = txtNotes.Text.Trim()
                    };
                    Item.CalculateAmount();
                }

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            var mainPanel = this.Controls[0] as Panel;
            var cmbProduct = mainPanel.Controls[1] as ComboBox;

            if (cmbProduct.SelectedIndex == -1 && string.IsNullOrEmpty(cmbProduct.Text))
            {
                MessageBox.Show("请选择商品", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // PurchaseOrderItemEditForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "PurchaseOrderItemEditForm";
            this.ResumeLayout(false);
        }
        #endregion
    }
}