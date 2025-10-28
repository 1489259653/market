using System;
using System.Drawing;
using System.Windows.Forms;
using market.Models;
using market.Services;

namespace market.Forms
{
    public partial class ProductEditForm : Form
    {
        private ProductService _productService;
        private CategoryService _categoryService;
        private Product _product;
        private bool _isEditMode;
        
        // UI控件
        private TextBox _txtProductCode;
        private TextBox _txtName;
        private TextBox _txtPrice;
        private TextBox _txtQuantity;
        private TextBox _txtUnit;
        private ComboBox _cmbCategory;
        private DateTimePicker _dtpExpiryDate;
        private TextBox _txtStockAlertThreshold;
        private ComboBox _cmbSupplier;
        private Button _btnScanBarcode;
        private Button _btnSave;
        private Button _btnCancel;
        private CheckBox _chkHasExpiryDate;

        public ProductEditForm(ProductService productService, Product product = null)
        {
            _productService = productService;
            _product = product;
            _isEditMode = product != null;
            
            InitializeComponent();
            LoadData();
            SetupForm();
        }

        private void InitializeComponent()
        {
            this.Text = _isEditMode ? "编辑商品" : "添加商品";
            this.Size = new Size(500, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 创建主面板
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // 创建表单控件
            var y = 10;
            var labelWidth = 100;
            var controlWidth = 300;
            var controlHeight = 25;
            var verticalSpacing = 35;

            // 商品编码
            var lblProductCode = new Label
            {
                Text = "商品编码:",
                Location = new Point(10, y),
                Size = new Size(labelWidth, controlHeight)
            };
            _txtProductCode = new TextBox
            {
                Location = new Point(120, y),
                Size = new Size(controlWidth - 40, controlHeight),
                ReadOnly = _isEditMode
            };
            
            // 扫描按钮
            _btnScanBarcode = new Button
            {
                Text = "扫描",
                Location = new Point(120 + controlWidth - 60, y), // 修正位置，与商品编码框在同一行
                Size = new Size(60, controlHeight),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 9)
            };
            
            // 添加商品编码相关控件到面板
            mainPanel.Controls.Add(lblProductCode);
            mainPanel.Controls.Add(_txtProductCode);
            mainPanel.Controls.Add(_btnScanBarcode);
            
            // 增加Y坐标，为下一个控件腾出空间
            y += verticalSpacing;

            // 商品名称
            var lblName = new Label
            {
                Text = "商品名称:",
                Location = new Point(10, y),
                Size = new Size(labelWidth, controlHeight)
            };
            _txtName = new TextBox
            {
                Location = new Point(120, y),
                Size = new Size(controlWidth, controlHeight)
            };
            
            // 添加商品名称相关控件到面板
            mainPanel.Controls.Add(lblName);
            mainPanel.Controls.Add(_txtName);
            
            // 增加Y坐标，为下一个控件腾出空间
            y += verticalSpacing;

            // 单价
            var lblPrice = new Label
            {
                Text = "单价:",
                Location = new Point(10, y),
                Size = new Size(labelWidth, controlHeight)
            };
            _txtPrice = new TextBox
            {
                Location = new Point(120, y),
                Size = new Size(controlWidth, controlHeight)
            };
            y += verticalSpacing;

            // 数量
            var lblQuantity = new Label
            {
                Text = "库存数量:",
                Location = new Point(10, y),
                Size = new Size(labelWidth, controlHeight)
            };
            _txtQuantity = new TextBox
            {
                Location = new Point(120, y),
                Size = new Size(controlWidth, controlHeight)
            };
            y += verticalSpacing;

            // 单位
            var lblUnit = new Label
            {
                Text = "单位:",
                Location = new Point(10, y),
                Size = new Size(labelWidth, controlHeight)
            };
            _txtUnit = new TextBox
            {
                Location = new Point(120, y),
                Size = new Size(controlWidth, controlHeight)
            };
            y += verticalSpacing;

            // 分类
            var lblCategory = new Label
            {
                Text = "分类:",
                Location = new Point(10, y),
                Size = new Size(labelWidth, controlHeight)
            };
            _cmbCategory = new ComboBox
            {
                Location = new Point(120, y),
                Size = new Size(controlWidth, controlHeight),
                DropDownStyle = ComboBoxStyle.DropDown
            };
            y += verticalSpacing;

            // 保质期
            var lblExpiryDate = new Label
            {
                Text = "保质期:",
                Location = new Point(10, y),
                Size = new Size(labelWidth, controlHeight)
            };
            _chkHasExpiryDate = new CheckBox
            {
                Text = "设置保质期",
                Location = new Point(120, y),
                Size = new Size(100, controlHeight),
                Checked = false
            };
            _dtpExpiryDate = new DateTimePicker
            {
                Location = new Point(230, y),
                Size = new Size(190, controlHeight),
                Format = DateTimePickerFormat.Short,
                Enabled = false
            };
            y += verticalSpacing;

            // 库存预警
            var lblStockAlertThreshold = new Label
            {
                Text = "库存预警:",
                Location = new Point(10, y),
                Size = new Size(labelWidth, controlHeight)
            };
            _txtStockAlertThreshold = new TextBox
            {
                Location = new Point(120, y),
                Size = new Size(controlWidth, controlHeight),
                Text = "10"
            };
            y += verticalSpacing;

            // 供货方
            var lblSupplier = new Label
            {
                Text = "供货方:",
                Location = new Point(10, y),
                Size = new Size(labelWidth, controlHeight)
            };
            _cmbSupplier = new ComboBox
            {
                Location = new Point(120, y),
                Size = new Size(controlWidth, controlHeight),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            y += verticalSpacing;

            // 按钮面板
            var buttonPanel = new Panel
            {
                Location = new Point(10, y + 20),
                Size = new Size(460, 50)
            };

            _btnSave = new Button
            {
                Text = "保存",
                Size = new Size(100, 35),
                Location = new Point(150, 10),
                BackColor = Color.Green,
                ForeColor = Color.White
            };

            _btnCancel = new Button
            {
                Text = "取消",
                Size = new Size(100, 35),
                Location = new Point(270, 10),
                BackColor = Color.Gray,
                ForeColor = Color.White
            };

            buttonPanel.Controls.Add(_btnSave);
            buttonPanel.Controls.Add(_btnCancel);

            // 添加控件到主面板
            mainPanel.Controls.AddRange(new Control[] {lblProductCode, _txtProductCode,
                lblName, _txtName,
                lblPrice, _txtPrice,
                lblQuantity, _txtQuantity,
                lblUnit, _txtUnit,
                lblCategory, _cmbCategory,
                lblExpiryDate, _chkHasExpiryDate, _dtpExpiryDate,
                lblStockAlertThreshold, _txtStockAlertThreshold,
                lblSupplier, _cmbSupplier,
                buttonPanel
            });

            // 添加主面板到窗体
            this.Controls.Add(mainPanel);

            // 绑定事件
            BindEvents();
        }

        private void BtnScanBarcode_Click(object sender, EventArgs e)
        {
            try
            {
                // 由于BarcodeScannerForm类在同一个命名空间中，我们可以直接使用
                using (var scannerForm = new BarcodeScannerForm())
                {
                    if (scannerForm.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(scannerForm.ScannedBarcode))
                    {
                        _txtProductCode.Text = scannerForm.ScannedBarcode;
                        
                        // 尝试根据条形码获取商品信息（如果系统中已有）
                        var existingProduct = _productService.GetProductByCode(scannerForm.ScannedBarcode);
                        if (existingProduct != null && !_isEditMode)
                        {
                            _txtName.Text = existingProduct.Name;
                            _txtPrice.Text = existingProduct.Price.ToString("F2");
                            _txtUnit.Text = existingProduct.Unit;
                            _cmbCategory.Text = existingProduct.Category;
                            _txtStockAlertThreshold.Text = existingProduct.StockAlertThreshold.ToString();
                            
                            if (existingProduct.ExpiryDate.HasValue)
                            {
                                _chkHasExpiryDate.Checked = true;
                                _dtpExpiryDate.Enabled = true;
                            }
                            
                            // 自动选择供应商
                            if (!string.IsNullOrEmpty(existingProduct.SupplierId))
                            {
                                for (int i = 0; i < _cmbSupplier.Items.Count; i++)
                                {
                                    var itemParts = _cmbSupplier.Items[i].ToString().Split('-');
                                    if (itemParts.Length > 1 && itemParts[0].Trim() == existingProduct.SupplierId)
                                    {
                                        _cmbSupplier.SelectedIndex = i;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"条形码扫描失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupForm()
        {
            // 设置输入验证
            _txtPrice.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                {
                    e.Handled = true;
                }
                // 确保只能输入一个小数点
                if (e.KeyChar == '.' && ((TextBox)s).Text.Contains("."))
                {
                    e.Handled = true;
                }
            };

            _txtQuantity.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                }
            };

            _txtStockAlertThreshold.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                }
            };
        }

        private void BindEvents()
        {
            _btnSave.Click += (s, e) => SaveProduct();
            _btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            _chkHasExpiryDate.CheckedChanged += (s, e) =>
            {
                _dtpExpiryDate.Enabled = _chkHasExpiryDate.Checked;
            };

            // 扫描按钮事件
            _btnScanBarcode.Click += BtnScanBarcode_Click;

            // 回车键保存
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    SaveProduct();
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    this.DialogResult = DialogResult.Cancel;
                }
            };
        }

        private void LoadData()
        {
            try
            {
                // 加载分类
                var categories = _productService.GetCategories();
                _cmbCategory.Items.Clear();
                _cmbCategory.Items.AddRange(categories.ToArray());

                // 加载供货方
                var suppliers = _productService.GetAllSuppliers();
                _cmbSupplier.Items.Clear();
                _cmbSupplier.Items.Add("（无）");
                foreach (var supplier in suppliers)
                {
                    _cmbSupplier.Items.Add(new SupplierComboBoxItem
                    {
                        Supplier = supplier,
                        DisplayText = supplier.Name
                    });
                }
                _cmbSupplier.SelectedIndex = 0;

                // 如果是编辑模式，加载商品数据
                if (_isEditMode && _product != null)
                {
                    _txtProductCode.Text = _product.ProductCode;
                    _txtName.Text = _product.Name;
                    _txtPrice.Text = _product.Price.ToString("F2");
                    _txtQuantity.Text = _product.Quantity.ToString();
                    _txtUnit.Text = _product.Unit;
                    _txtStockAlertThreshold.Text = _product.StockAlertThreshold.ToString();

                    // 设置分类
                    if (!string.IsNullOrEmpty(_product.Category))
                    {
                        var categoryIndex = categories.IndexOf(_product.Category);
                        if (categoryIndex >= 0)
                        {
                            _cmbCategory.SelectedIndex = categoryIndex;
                        }
                        else
                        {
                            _cmbCategory.Text = _product.Category;
                        }
                    }

                    // 设置保质期
                    if (_product.ExpiryDate.HasValue)
                    {
                        _chkHasExpiryDate.Checked = true;
                        _dtpExpiryDate.Value = _product.ExpiryDate.Value;
                    }

                    // 设置供货方
                    if (!string.IsNullOrEmpty(_product.SupplierId))
                    {
                        for (int i = 0; i < _cmbSupplier.Items.Count; i++)
                        {
                            var item = _cmbSupplier.Items[i] as SupplierComboBoxItem;
                            if (item != null && item.Supplier.Id == _product.SupplierId)
                            {
                                _cmbSupplier.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // 添加模式，设置默认值
                    _txtStockAlertThreshold.Text = "10";
                    _dtpExpiryDate.Value = DateTime.Now.AddYears(1);
                    
                    // 自动生成商品编码
                    _txtProductCode.Text = _productService.GenerateProductCode();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void SaveProduct()
        {
            try
            {
                // 验证输入
                if (!ValidateInput())
                {
                    return;
                }

                var product = new Product
                {
                    ProductCode = _txtProductCode.Text.Trim(),
                    Name = _txtName.Text.Trim(),
                    Price = decimal.Parse(_txtPrice.Text),
                    Quantity = int.Parse(_txtQuantity.Text),
                    Unit = _txtUnit.Text.Trim(),
                    Category = _cmbCategory.Text.Trim(),
                    StockAlertThreshold = int.Parse(_txtStockAlertThreshold.Text),
                    ExpiryDate = _chkHasExpiryDate.Checked ? _dtpExpiryDate.Value : (DateTime?)null,
                    LastUpdated = DateTime.Now
                };

                // 设置供货方
                var selectedSupplier = _cmbSupplier.SelectedItem as SupplierComboBoxItem;
                if (selectedSupplier != null && selectedSupplier.Supplier != null)
                {
                    product.SupplierId = selectedSupplier.Supplier.Id;
                }

                bool success;
                if (_isEditMode)
                {
                    success = _productService.UpdateProduct(product);
                }
                else
                {
                    // 检查商品编码是否已存在
                    if (_productService.ProductCodeExists(product.ProductCode))
                    {
                        // 如果编码已存在，自动生成新编码
                        product.ProductCode = _productService.GenerateProductCode();
                        _txtProductCode.Text = product.ProductCode;
                    }
                    success = _productService.AddProduct(product);
                }

                if (success)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("保存失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存商品失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(_txtProductCode.Text))
            {
                MessageBox.Show("请输入商品编码！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtProductCode.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtName.Text))
            {
                MessageBox.Show("请输入商品名称！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtName.Focus();
                return false;
            }

            if (!decimal.TryParse(_txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("请输入有效的单价！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtPrice.Focus();
                return false;
            }

            if (!int.TryParse(_txtQuantity.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("请输入有效的库存数量！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtQuantity.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtUnit.Text))
            {
                MessageBox.Show("请输入商品单位！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtUnit.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(_cmbCategory.Text))
            {
                MessageBox.Show("请输入商品分类！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cmbCategory.Focus();
                return false;
            }

            if (!int.TryParse(_txtStockAlertThreshold.Text, out int threshold) || threshold < 0)
            {
                MessageBox.Show("请输入有效的库存预警值！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtStockAlertThreshold.Focus();
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// 供货方下拉框项
    /// </summary>
    internal class SupplierComboBoxItem
    {
        public Supplier Supplier { get; set; }
        public string DisplayText { get; set; }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}