using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using market.Models;
using market.Services;

namespace market.Forms
{
    public partial class SaleCounterForm : Form
    {
        private readonly SaleService _saleService;
        private readonly AuthService _authService;
        
        private List<CartItem> _cartItems = new List<CartItem>();
        private decimal _totalAmount = 0;
        private decimal _discountAmount = 0;
        private decimal _finalAmount = 0;

        // UI控件
        private TextBox _txtProductCode;
        private NumericUpDown _numQuantity;
        private DataGridView _dgvCart;
        private Label _lblTotalAmount;
        private Label _lblDiscountAmount;
        private Label _lblFinalAmount;
        private TextBox _txtCustomer;
        private TextBox _txtNotes;
        private ComboBox _cmbPaymentMethod;

        public SaleCounterForm(SaleService saleService, AuthService authService)
        {
            _saleService = saleService;
            _authService = authService;
            
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeComponent()
        {
            this.Text = "销售收银";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
        }

        private void InitializeForm()
        {
            // 创建主布局面板
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // 创建顶部商品输入面板
            var inputPanel = CreateInputPanel();
            mainPanel.Controls.Add(inputPanel);

            // 创建购物车面板
            var cartPanel = CreateCartPanel();
            cartPanel.Dock = DockStyle.Fill;
            mainPanel.Controls.Add(cartPanel);

            // 创建底部结算面板
            var settlementPanel = CreateSettlementPanel();
            settlementPanel.Dock = DockStyle.Bottom;
            mainPanel.Controls.Add(settlementPanel);

            this.Controls.Add(mainPanel);
        }

        private Panel CreateInputPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BorderStyle = BorderStyle.FixedSingle
            };

            // 商品编码输入
            var lblProductCode = new Label { Text = "商品编码/条形码:", Location = new Point(10, 15), Width = 120 };
            _txtProductCode = new TextBox { Location = new Point(130, 12), Width = 200, TabIndex = 0 };

            // 数量输入
            var lblQuantity = new Label { Text = "数量:", Location = new Point(350, 15), Width = 50 };
            _numQuantity = new NumericUpDown { Location = new Point(400, 12), Width = 80, Minimum = 1, Value = 1 };

            // 添加按钮
            var btnAdd = new Button { Text = "添加商品", Location = new Point(500, 10), Size = new Size(80, 30) };
            var btnScan = new Button { Text = "扫描", Location = new Point(590, 10), Size = new Size(60, 30) };

            // 快速商品按钮
            var btnQuickProduct1 = new Button { Text = "商品A", Location = new Point(10, 50), Size = new Size(80, 30) };
            var btnQuickProduct2 = new Button { Text = "商品B", Location = new Point(100, 50), Size = new Size(80, 30) };
            var btnQuickProduct3 = new Button { Text = "商品C", Location = new Point(190, 50), Size = new Size(80, 30) };

            panel.Controls.AddRange(new Control[] {
                lblProductCode, _txtProductCode,
                lblQuantity, _numQuantity,
                btnAdd, btnScan,
                btnQuickProduct1, btnQuickProduct2, btnQuickProduct3
            });

            // 事件处理
            btnAdd.Click += (s, e) => AddProductToCart();
            btnScan.Click += (s, e) => ScanBarcode();
            _txtProductCode.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    AddProductToCart();
                    e.Handled = true;
                }
            };

            return panel;
        }

        private Panel CreateCartPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            // 购物车标题
            var lblCartTitle = new Label { Text = "购物车", Location = new Point(10, 10), Font = new Font("微软雅黑", 12, FontStyle.Bold) };

            // 购物车数据网格
            _dgvCart = new DataGridView
            {
                Location = new Point(10, 40),
                Size = new Size(panel.Width - 40, panel.Height - 100),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // 添加列
            _dgvCart.Columns.Add("ProductCode", "商品编码");
            _dgvCart.Columns.Add("ProductName", "商品名称");
            _dgvCart.Columns.Add("Quantity", "数量");
            _dgvCart.Columns.Add("SalePrice", "单价");
            _dgvCart.Columns.Add("Amount", "金额");

            // 设置金额列格式
            _dgvCart.Columns["SalePrice"].DefaultCellStyle.Format = "C2";
            _dgvCart.Columns["SalePrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvCart.Columns["Amount"].DefaultCellStyle.Format = "C2";
            _dgvCart.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // 操作按钮
            var btnEdit = new Button { Text = "修改数量", Location = new Point(10, panel.Height - 50), Size = new Size(80, 30) };
            var btnRemove = new Button { Text = "删除商品", Location = new Point(100, panel.Height - 50), Size = new Size(80, 30) };
            var btnClear = new Button { Text = "清空购物车", Location = new Point(190, panel.Height - 50), Size = new Size(80, 30) };

            panel.Controls.AddRange(new Control[] {
                lblCartTitle, _dgvCart, btnEdit, btnRemove, btnClear
            });

            // 事件处理
            btnEdit.Click += (s, e) => EditCartItem();
            btnRemove.Click += (s, e) => RemoveCartItem();
            btnClear.Click += (s, e) => ClearCart();
            _dgvCart.SelectionChanged += (s, e) => UpdateButtonStates();

            return panel;
        }

        private Panel CreateSettlementPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 200,
                BorderStyle = BorderStyle.FixedSingle
            };

            // 金额显示
            var lblTotalAmount = new Label { Text = "商品总金额:", Location = new Point(10, 15), Width = 100 };
            _lblTotalAmount = new Label { Location = new Point(120, 15), Width = 100, Text = "￥0.00", Font = new Font("微软雅黑", 12, FontStyle.Bold) };

            var lblDiscountAmount = new Label { Text = "折扣金额:", Location = new Point(250, 15), Width = 80 };
            _lblDiscountAmount = new Label { Location = new Point(340, 15), Width = 100, Text = "￥0.00" };

            var lblFinalAmount = new Label { Text = "应付金额:", Location = new Point(470, 15), Width = 80 };
            _lblFinalAmount = new Label { Location = new Point(560, 15), Width = 120, Text = "￥0.00", Font = new Font("微软雅黑", 14, FontStyle.Bold), ForeColor = Color.Blue };

            // 顾客信息
            var lblCustomer = new Label { Text = "顾客姓名:", Location = new Point(10, 50), Width = 80 };
            _txtCustomer = new TextBox { Location = new Point(90, 47), Width = 150, Text = "散客" };

            // 支付方式
            var lblPaymentMethod = new Label { Text = "支付方式:", Location = new Point(270, 50), Width = 80 };
            _cmbPaymentMethod = new ComboBox { Location = new Point(350, 47), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbPaymentMethod.Items.AddRange(new object[] { "现金", "微信支付", "支付宝", "银行卡" });
            _cmbPaymentMethod.SelectedIndex = 0;

            // 备注
            var lblNotes = new Label { Text = "备注:", Location = new Point(500, 50), Width = 50 };
            _txtNotes = new TextBox { Location = new Point(550, 47), Width = 300 };

            // 结算按钮
            var btnSettle = new Button { Text = "结算", Location = new Point(10, 90), Size = new Size(100, 40), 
                Font = new Font("微软雅黑", 12, FontStyle.Bold), BackColor = Color.LightGreen };
            var btnCancel = new Button { Text = "取消", Location = new Point(120, 90), Size = new Size(80, 40) };
            var btnPrint = new Button { Text = "打印小票", Location = new Point(210, 90), Size = new Size(80, 40) };

            panel.Controls.AddRange(new Control[] {
                lblTotalAmount, _lblTotalAmount,
                lblDiscountAmount, _lblDiscountAmount,
                lblFinalAmount, _lblFinalAmount,
                lblCustomer, _txtCustomer,
                lblPaymentMethod, _cmbPaymentMethod,
                lblNotes, _txtNotes,
                btnSettle, btnCancel, btnPrint
            });

            // 事件处理
            btnSettle.Click += (s, e) => SettleSale();
            btnCancel.Click += (s, e) => this.Close();
            btnPrint.Click += (s, e) => PrintReceipt();

            return panel;
        }

        private void AddProductToCart()
        {
            try
            {
                string productCode = _txtProductCode.Text.Trim();
                if (string.IsNullOrEmpty(productCode))
                {
                    MessageBox.Show("请输入商品编码或条形码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int quantity = (int)_numQuantity.Value;
                if (quantity <= 0)
                {
                    MessageBox.Show("数量必须大于0", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 获取商品信息
                Product product = null;
                
                // 先尝试按商品编码查找
                if (!string.IsNullOrEmpty(productCode))
                {
                    product = _saleService.GetProductByCode(productCode);
                }

                // 如果没找到，尝试按条形码查找
                if (product == null && !string.IsNullOrEmpty(productCode))
                {
                    product = _saleService.GetProductByBarcode(productCode);
                }

                if (product == null)
                {
                    MessageBox.Show("未找到该商品，请检查商品编码或条形码", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 检查库存
                if (!_saleService.CheckStock(product.ProductCode, quantity))
                {
                    MessageBox.Show($"商品 {product.Name} 库存不足，当前库存: {product.Quantity}", "库存不足", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 添加商品到购物车
                var existingItem = _cartItems.FirstOrDefault(item => item.ProductCode == product.ProductCode);
                if (existingItem != null)
                {
                    // 如果商品已存在，增加数量
                    existingItem.Quantity += quantity;
                    existingItem.CalculateAmount();
                }
                else
                {
                    // 添加新商品
                    var cartItem = new CartItem
                    {
                        ProductCode = product.ProductCode,
                        ProductName = product.Name,
                        Quantity = quantity,
                        SalePrice = product.Price,
                        IsWeight = false
                    };
                    cartItem.CalculateAmount();
                    _cartItems.Add(cartItem);
                }

                // 更新显示
                RefreshCartDisplay();
                CalculateAmounts();

                // 清空输入框
                _txtProductCode.Text = "";
                _numQuantity.Value = 1;
                _txtProductCode.Focus();

                MessageBox.Show($"商品 {product.Name} 已添加到购物车", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加商品失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ScanBarcode()
        {
            try
            {
                var scannerForm = new BarcodeScannerForm();
                if (scannerForm.ShowDialog() == DialogResult.OK)
                {
                    _txtProductCode.Text = scannerForm.Barcode;
                    AddProductToCart();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"扫描失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditCartItem()
        {
            if (_dgvCart.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择要修改的商品", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRow = _dgvCart.SelectedRows[0];
            var productCode = selectedRow.Cells["ProductCode"].Value.ToString();
            var cartItem = _cartItems.FirstOrDefault(item => item.ProductCode == productCode);

            if (cartItem != null)
            {
                // 弹出修改数量对话框
                string input = Microsoft.VisualBasic.Interaction.InputBox("请输入新的数量:", "修改数量", cartItem.Quantity.ToString(), -1, -1);
                if (!string.IsNullOrEmpty(input) && int.TryParse(input, out int newQuantity) && newQuantity > 0)
                {
                    // 检查库存
                    if (!_saleService.CheckStock(cartItem.ProductCode, newQuantity))
                    {
                        var product = _saleService.GetProductByCode(cartItem.ProductCode);
                        MessageBox.Show($"商品 {product.Name} 库存不足，当前库存: {product.Quantity}", "库存不足", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    cartItem.Quantity = newQuantity;
                    cartItem.CalculateAmount();
                    
                    RefreshCartDisplay();
                    CalculateAmounts();
                }
            }
        }

        private void RemoveCartItem()
        {
            if (_dgvCart.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择要删除的商品", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRow = _dgvCart.SelectedRows[0];
            var productCode = selectedRow.Cells["ProductCode"].Value.ToString();
            
            var cartItem = _cartItems.FirstOrDefault(item => item.ProductCode == productCode);
            if (cartItem != null)
            {
                if (MessageBox.Show($"确定要删除商品 {cartItem.ProductName} 吗？", "确认删除", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _cartItems.Remove(cartItem);
                    RefreshCartDisplay();
                    CalculateAmounts();
                }
            }
        }

        private void ClearCart()
        {
            if (_cartItems.Count > 0)
            {
                if (MessageBox.Show("确定要清空购物车吗？", "确认清空", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _cartItems.Clear();
                    RefreshCartDisplay();
                    CalculateAmounts();
                }
            }
        }

        private void RefreshCartDisplay()
        {
            _dgvCart.Rows.Clear();
            
            foreach (var item in _cartItems)
            {
                _dgvCart.Rows.Add(
                    item.ProductCode,
                    item.ProductName,
                    item.Quantity,
                    item.SalePrice,
                    item.Amount
                );
            }

            UpdateButtonStates();
        }

        private void CalculateAmounts()
        {
            _totalAmount = _cartItems.Sum(item => item.Amount);
            _finalAmount = _totalAmount - _discountAmount;

            _lblTotalAmount.Text = $"￥{_totalAmount:F2}";
            _lblDiscountAmount.Text = $"￥{_discountAmount:F2}";
            _lblFinalAmount.Text = $"￥{_finalAmount:F2}";
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = _dgvCart.SelectedRows.Count > 0;
            // 可以在这里更新按钮的Enabled状态
        }

        private void SettleSale()
        {
            try
            {
                if (_cartItems.Count == 0)
                {
                    MessageBox.Show("购物车为空，请先添加商品", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 创建销售订单
                var saleOrder = new SaleOrder
                {
                    OrderNumber = _saleService.GenerateSaleOrderNumber(),
                    OrderDate = DateTime.Now,
                    Customer = _txtCustomer.Text.Trim(),
                    OperatorId = _authService.CurrentUser.Id,
                    Status = SaleOrderStatus.Pending,
                    TotalAmount = _totalAmount,
                    DiscountAmount = _discountAmount,
                    FinalAmount = _finalAmount,
                    PaymentMethod = (PaymentMethod)_cmbPaymentMethod.SelectedIndex,
                    Notes = _txtNotes.Text.Trim(),
                    CreatedAt = DateTime.Now
                };

                // 转换购物车商品为销售明细
                foreach (var cartItem in _cartItems)
                {
                    saleOrder.Items.Add(new SaleOrderItem
                    {
                        ProductCode = cartItem.ProductCode,
                        ProductName = cartItem.ProductName,
                        Quantity = cartItem.Quantity,
                        SalePrice = cartItem.SalePrice,
                        Amount = cartItem.Amount,
                        OriginalPrice = cartItem.SalePrice, // 暂时与原价相同
                        DiscountRate = 0 // 暂时无折扣
                    });
                }

                // 显示支付对话框
                var paymentForm = new PaymentForm(saleOrder.FinalAmount, (PaymentMethod)saleOrder.PaymentMethod);
                if (paymentForm.ShowDialog() == DialogResult.OK)
                {
                    saleOrder.ReceivedAmount = paymentForm.ReceivedAmount;
                    saleOrder.ChangeAmount = paymentForm.ChangeAmount;
                    saleOrder.Status = SaleOrderStatus.Paid;

                    // 保存销售订单
                    if (_saleService.CreateSaleOrder(saleOrder))
                    {
                        MessageBox.Show($"销售成功！单号: {saleOrder.OrderNumber}\n实收: ￥{saleOrder.ReceivedAmount:F2}\n找零: ￥{saleOrder.ChangeAmount:F2}", 
                            "销售成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // 清空购物车
                        _cartItems.Clear();
                        RefreshCartDisplay();
                        CalculateAmounts();

                        // 询问是否打印小票
                        if (MessageBox.Show("是否打印小票？", "打印小票", 
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            PrintReceipt(saleOrder);
                        }
                    }
                    else
                    {
                        MessageBox.Show("销售失败，请重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"结算失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintReceipt(SaleOrder order = null)
        {
            try
            {
                if (order == null)
                {
                    MessageBox.Show("没有可打印的销售记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 模拟打印小票
                string receipt = $"=== 超市销售小票 ===\n" +
                                $"单号: {order.OrderNumber}\n" +
                                $"日期: {order.OrderDate:yyyy-MM-dd HH:mm:ss}\n" +
                                $"收银员: {order.OperatorName}\n" +
                                $"顾客: {order.Customer}\n" +
                                $"----------------------------\n";

                foreach (var item in order.Items)
                {
                    receipt += $"{item.ProductName} x{item.Quantity} ￥{item.SalePrice:F2}\n";
                }

                receipt += $"----------------------------\n" +
                          $"商品总金额: ￥{order.TotalAmount:F2}\n" +
                          $"折扣金额: ￥{order.DiscountAmount:F2}\n" +
                          $"应付金额: ￥{order.FinalAmount:F2}\n" +
                          $"实收金额: ￥{order.ReceivedAmount:F2}\n" +
                          $"找零金额: ￥{order.ChangeAmount:F2}\n" +
                          $"支付方式: {order.PaymentMethodText}\n" +
                          $"----------------------------\n" +
                          $"感谢您的光临！\n";

                MessageBox.Show(receipt, "销售小票预览", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打印小票失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _txtProductCode.Focus();
        }
    }
}