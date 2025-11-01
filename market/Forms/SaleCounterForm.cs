using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using market.Models;
using market.Services;
using System.Media;
// 使用别名避免命名冲突
using AForgeLib = AForge;
using AForgeVideo = AForge.Video;
using AForgeVideoDirectShow = AForge.Video.DirectShow;
using ZXing;
using ZXing.Windows.Compatibility;

namespace market.Forms
{
    public partial class SaleCounterForm : Form
    {
        private readonly SaleService _saleService;
        private readonly AuthService _authService;
        private readonly MemberService _memberService;
        private Member _selectedMember = null;
        
        private List<CartItem> _cartItems = new List<CartItem>();
        private decimal _totalAmount = 0;
        private decimal _discountAmount = 0;
        private decimal _pointsDiscountAmount = 0; // 积分抵扣金额
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
        private Label _lblMemberLevel;
        private ListBox _lbxMemberSearchResults;
        private bool _isSearching = false;

        // 摄像头扫描相关变量
        private PictureBox _cameraPictureBox;
        private System.Threading.Thread _scanThread;
        private bool _isScanning;
        private System.Windows.Forms.Timer _scanTimer;
        private Random _randomBarcodeGenerator;
        private DateTime _lastScanTime = DateTime.MinValue;
        private AForgeVideoDirectShow.VideoCaptureDevice _videoSource;
        private AForgeVideoDirectShow.FilterInfoCollection _videoDevices;
        private bool _isCameraInitialized = false;
        private Bitmap _lastFrame; // 存储最后一帧用于条形码识别
        private BarcodeReader _barcodeReader;
        
        // 模拟的条形码数据
        private readonly string[] _sampleBarcodes = {
            "6901234567890", // EAN-13
            "123456789012",   // UPC-A
            "9780201379624",  // ISBN
            "6923456789012",  // 商品条码
            "6934567890123",  // 商品条码
            "6945678901234",  // 商品条码
            "6956789012345"   // 商品条码
        };

        public SaleCounterForm(SaleService saleService, AuthService authService, MemberService memberService)
        {
            _saleService = saleService;
            _authService = authService;
            _memberService = memberService;
            
            InitializeComponent();
            InitializeForm();
            
            // 添加FormClosing事件处理，确保窗口关闭时释放资源
            this.FormClosing += SaleCounterForm_FormClosing;
            
            // 初始化条形码解码器
            _barcodeReader = new ZXing.Windows.Compatibility.BarcodeReader
            {
                AutoRotate = true,
                Options = new ZXing.Common.DecodingOptions
                {
                    TryHarder = true,
                    TryInverted = true,
                    PossibleFormats = new[] {
                        ZXing.BarcodeFormat.EAN_13,
                        ZXing.BarcodeFormat.EAN_8,
                        ZXing.BarcodeFormat.UPC_A,
                        ZXing.BarcodeFormat.UPC_E,
                        ZXing.BarcodeFormat.CODE_128,
                        ZXing.BarcodeFormat.CODE_39
                    }
                }
            };
        }
        
        private void SaleCounterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 停止摄像头扫描，释放所有相关资源
            StopCameraScan();
            
            // 释放_timer资源
            if (_scanTimer != null)
            {
                _scanTimer.Dispose();
                _scanTimer = null;
            }
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
            var mainPanel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            mainPanel.RowCount = 3;
            mainPanel.ColumnCount = 1;
            
            // 设置行高比例：输入面板固定高度，购物车面板填充剩余空间，结算面板固定高度
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 150)); // 输入面板
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // 购物车面板（填充剩余空间）
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 200)); // 结算面板

            // 创建顶部商品输入面板
            var inputPanel = CreateInputPanel();
            inputPanel.Dock = DockStyle.Fill;
            mainPanel.Controls.Add(inputPanel, 0, 0);

            // 创建购物车面板
            var cartPanel = CreateCartPanel();
            cartPanel.Dock = DockStyle.Fill;
            mainPanel.Controls.Add(cartPanel, 0, 1);

            // 创建底部结算面板
            var settlementPanel = CreateSettlementPanel();
            settlementPanel.Dock = DockStyle.Fill;
            mainPanel.Controls.Add(settlementPanel, 0, 2);

            this.Controls.Add(mainPanel);
        }

        private Panel CreateInputPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 400, // 增加高度以容纳摄像头显示框
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

            // 摄像头显示框（替换原来的扫描按钮位置）
            var picCamera = new PictureBox
            {
                Location = new Point(600, 0),
                Size = new Size(400, 143),
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle
            };

            // 快速商品按钮
            var btnQuickProduct1 = new Button { Text = "商品A", Location = new Point(130, 40), Size = new Size(80, 30) };
            var btnQuickProduct2 = new Button { Text = "商品B", Location = new Point(210, 40), Size = new Size(80, 30) };
            var btnQuickProduct3 = new Button { Text = "商品C", Location = new Point(290, 40), Size = new Size(80, 30) };

            panel.Controls.AddRange(new Control[] {
                lblProductCode, _txtProductCode,
                lblQuantity, _numQuantity,
                btnAdd,
                picCamera,
                btnQuickProduct1, btnQuickProduct2, btnQuickProduct3
            });

            // 事件处理
            btnAdd.Click += (s, e) => AddProductToCart();
            
            // 商品编码输入框回车事件
            _txtProductCode.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    AddProductToCart();
                    e.Handled = true;
                }
            };

            // 初始化摄像头扫描功能
            InitializeCameraScanner(picCamera);

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
            var lblCartTitle = new Label { Text = "购物车", Location = new Point(10, 10), Font = new Font("微软雅黑", 12, FontStyle.Bold), AutoSize = true };

            // 购物车数据网格
            _dgvCart = new DataGridView
            {
                Location = new Point(10, 50),
                Size = new Size(panel.ClientSize.Width - 40, panel.ClientSize.Height - 120),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
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

            // 操作按钮面板
            var buttonPanel = new Panel
            {
                Location = new Point(10, panel.ClientSize.Height - 60),
                Size = new Size(300, 40),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            // 操作按钮
            var btnEdit = new Button { Text = "修改数量", Location = new Point(0, 0), Size = new Size(80, 30) };
            var btnRemove = new Button { Text = "删除商品", Location = new Point(90, 0), Size = new Size(80, 30) };
            var btnClear = new Button { Text = "清空购物车", Location = new Point(180, 0), Size = new Size(100, 30) };

            buttonPanel.Controls.AddRange(new Control[] { btnEdit, btnRemove, btnClear });

            panel.Controls.AddRange(new Control[] {
                lblCartTitle, _dgvCart, buttonPanel
            });

            // 事件处理
            btnEdit.Click += (s, e) => EditCartItem();
            btnRemove.Click += (s, e) => RemoveCartItem();
            btnClear.Click += (s, e) => ClearCart();
            _dgvCart.SelectionChanged += (s, e) => UpdateButtonStates();

            // 处理面板大小变化
            panel.SizeChanged += (s, e) =>
            {
                if (_dgvCart != null)
                {
                    _dgvCart.Size = new Size(panel.ClientSize.Width - 40, panel.ClientSize.Height - 120);
                    buttonPanel.Location = new Point(10, panel.ClientSize.Height - 60);
                }
            };

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
            var lblCustomer = new Label { Text = "会员搜索:", Location = new Point(10, 50), Width = 80 };
            _txtCustomer = new TextBox { Location = new Point(90, 47), Width = 150, Text = "" };
            _lblMemberLevel = new Label { Location = new Point(250, 47), Width = 100, Text = "", Font = new Font(Font, FontStyle.Bold) };
            
            // 会员搜索结果下拉框
            _lbxMemberSearchResults = new ListBox { Location = new Point(90, 67), Width = 150, Height = 100, Visible = false };
            _lbxMemberSearchResults.Click += (s, e) => SelectMemberFromList();
            
            // 支付方式
            var lblPaymentMethod = new Label { Text = "支付方式:", Location = new Point(360, 50), Width = 80 };
            _cmbPaymentMethod = new ComboBox { Location = new Point(440, 47), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbPaymentMethod.Items.AddRange(new object[] { "现金", "微信支付", "支付宝", "银行卡" });
            _cmbPaymentMethod.SelectedIndex = 0;

            // 备注
            var lblNotes = new Label { Text = "备注:", Location = new Point(570, 50), Width = 50 };
            _txtNotes = new TextBox { Location = new Point(620, 47), Width = 300 };

            // 结算按钮
            var btnSettle = new Button { Text = "结算", Location = new Point(10, 90), Size = new Size(100, 40), 
                Font = new Font("微软雅黑", 12, FontStyle.Bold), BackColor = Color.LightGreen };
            var btnCancel = new Button { Text = "取消", Location = new Point(120, 90), Size = new Size(80, 40) };
            var btnPrint = new Button { Text = "打印小票", Location = new Point(210, 90), Size = new Size(80, 40) };

            panel.Controls.AddRange(new Control[] {
                lblTotalAmount, _lblTotalAmount,
                lblDiscountAmount, _lblDiscountAmount,
                lblFinalAmount, _lblFinalAmount,
                lblCustomer, _txtCustomer, _lblMemberLevel,
                _lbxMemberSearchResults,
                lblPaymentMethod, _cmbPaymentMethod,
                lblNotes, _txtNotes,
                btnSettle, btnCancel, btnPrint
            });

            // 事件处理
            btnSettle.Click += (s, e) => SettleSale();
            btnCancel.Click += (s, e) => this.Close();
            btnPrint.Click += (s, e) => PrintReceipt();
            
            // 会员搜索事件
            _txtCustomer.TextChanged += (s, e) => SearchMember();
            _txtCustomer.LostFocus += (s, e) => {
                // 延迟隐藏搜索结果，以便用户可以点击选择
                var timer = new System.Windows.Forms.Timer();
                timer.Interval = 200;
                timer.Tick += (timerS, timerE) => {
                    _lbxMemberSearchResults.Visible = false;
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            };
            _txtCustomer.GotFocus += (s, e) => {
                if (!string.IsNullOrEmpty(_txtCustomer.Text) && _lbxMemberSearchResults.Items.Count > 0) {
                    _lbxMemberSearchResults.Visible = true;
                }
            };

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
            
            // 根据会员等级计算折扣
            if (_selectedMember != null)
            {
                _discountAmount = CalculateMemberDiscount(_totalAmount, _selectedMember.Discount);
            }
            else
            {
                _discountAmount = 0;
                _pointsDiscountAmount = 0; // 非会员不使用积分抵扣
            }
            
            // 计算最终金额（总金额 - 会员折扣 - 积分抵扣）
            _finalAmount = Math.Max(0, _totalAmount - _discountAmount - _pointsDiscountAmount);

            _lblTotalAmount.Text = $"￥{_totalAmount:F2}";
            _lblDiscountAmount.Text = $"￥{_discountAmount:F2}{(_pointsDiscountAmount > 0 ? $" (含积分抵扣￥{_pointsDiscountAmount:F2})" : "")}";
            _lblFinalAmount.Text = $"￥{_finalAmount:F2}";
        }
        
        private decimal CalculateMemberDiscount(decimal amount, decimal discount)
        {
            // 使用会员的折扣率计算折扣金额
            // discount为折扣率，如0.98表示98折
            // 折扣金额 = 原价 × (1 - 折扣率)
            return amount * (1 - discount);
        }
        
        private void SearchMember()
        {
            if (_isSearching) return;
            
            _isSearching = true;
            string keyword = _txtCustomer.Text.Trim();
            
            if (string.IsNullOrEmpty(keyword))
            {
                _lbxMemberSearchResults.Items.Clear();
                _lbxMemberSearchResults.Visible = false;
                _selectedMember = null;
                _lblMemberLevel.Text = "";
                CalculateAmounts();
                _isSearching = false;
                return;
            }
            
            // 使用数据库搜索功能查找会员
            var searchResults = _memberService.SearchMembers(keyword);
            
            _lbxMemberSearchResults.Items.Clear();
            foreach (var member in searchResults)
            {
                _lbxMemberSearchResults.Items.Add($"{member.Name} - {member.PhoneNumber} ({GetLevelName(member.Level)})");
            }
            
            if (searchResults.Count > 0)
            {
                _lbxMemberSearchResults.Visible = true;
            }
            else
            {
                _lbxMemberSearchResults.Visible = false;
                _selectedMember = null;
                _lblMemberLevel.Text = "";  
                CalculateAmounts();
            }
            
            _isSearching = false;
        }
        
        private void SelectMemberFromList()
        {
            if (_lbxMemberSearchResults.SelectedIndex >= 0)
            {
                string selectedText = _lbxMemberSearchResults.SelectedItem.ToString();
                string keyword = _txtCustomer.Text.Trim();
                
                // 查找选中的会员
                var allMembers = _memberService.GetAllMembers();
                _selectedMember = allMembers.FirstOrDefault(m => 
                    (m.Name.Contains(keyword) || m.PhoneNumber.Contains(keyword)) && 
                    selectedText.Contains(m.Name) && selectedText.Contains(m.PhoneNumber)
                );
                
                if (_selectedMember != null)
                {
                    _txtCustomer.Text = _selectedMember.Name;
                    _lblMemberLevel.Text = $"{GetLevelName(_selectedMember.Level)} | 积分:{_selectedMember.Points} | 累计消费:{_selectedMember.TotalSpending:C2}";
                    
                    // 询问是否使用积分抵扣
                    if (_selectedMember.Points > 0 && _totalAmount > 0)
                    {
                        DialogResult result = MessageBox.Show($"当前有 {_selectedMember.Points} 积分，是否使用积分抵扣？\n100积分 = 1元", 
                            "积分抵扣", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        
                        if (result == DialogResult.Yes)
                        {
                            decimal maxDeductPoints = Math.Min(_selectedMember.Points, Math.Floor(_totalAmount * 100));
                            string input = Microsoft.VisualBasic.Interaction.InputBox("请输入要使用的积分数量:\n(100积分 = 1元，最多可使用" + maxDeductPoints + "积分)", 
                                "积分抵扣", maxDeductPoints.ToString(), -1, -1);
                            
                            if (!string.IsNullOrEmpty(input) && decimal.TryParse(input, out decimal pointsToUse) && pointsToUse > 0 && pointsToUse <= maxDeductPoints)
                            {
                                // 计算积分抵扣金额（100积分 = 1元）
                                _pointsDiscountAmount = pointsToUse / 100;
                            }
                        }
                    }
                    
                    // 重新计算所有购物车商品的折扣
                    CalculateAmounts();
                }
            }
            
            _lbxMemberSearchResults.Visible = false;
        }
        
        private string GetLevelName(MemberLevel level)
        {
            switch (level)
            {
                case MemberLevel.Bronze: return "铜牌会员";
                case MemberLevel.Silver: return "银牌会员";
                case MemberLevel.Gold: return "金牌会员";
                case MemberLevel.Platinum: return "铂金会员";
                default: return "普通顾客";
            }
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
                    MemberId = _selectedMember?.Id,
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
                        // 更新会员积分和累积消费金额
                        if (_selectedMember != null)
                        {
                            // 1. 使用积分抵扣
                            if (_pointsDiscountAmount > 0)
                            {
                                decimal pointsToDeduct = _pointsDiscountAmount * 100; // 1元 = 100积分
                                _memberService.DeductPoints(_selectedMember.Id, pointsToDeduct);
                            }
                            
                            // 2. 按照消费金额的1%累计积分（基于实际支付金额）
                            decimal pointsToAdd = Math.Round(_finalAmount * 0.01m, 0);
                            _memberService.UpdatePoints(_selectedMember.Id, pointsToAdd);
                            
                            // 3. 更新累积消费金额（使用实际支付金额）
                            _memberService.UpdateTotalSpending(_selectedMember.Id, _finalAmount);
                        }
                        
                        MessageBox.Show($"销售成功！单号: {saleOrder.OrderNumber}\n实收: ￥{saleOrder.ReceivedAmount:F2}\n找零: ￥{saleOrder.ChangeAmount:F2}", 
                            "销售成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // 清空购物车和会员信息
                    _cartItems.Clear();
                    _selectedMember = null;
                    _txtCustomer.Text = "";
                    _lblMemberLevel.Text = "";
                    _pointsDiscountAmount = 0; // 重置积分抵扣金额
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
            // 自动启动摄像头扫描
            StartCameraScan();
        }

        private void InitializeCameraScanner(PictureBox cameraPictureBox)
        {
            _cameraPictureBox = cameraPictureBox;
            _randomBarcodeGenerator = new Random();
            
            // 初始化扫描计时器
            _scanTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000 // 每1秒检查一次
            };
            _scanTimer.Tick += (s, e) => ProcessCameraFrame();
            
            // 初始化条形码扫描需要的变量
            _lastFrame = null;
            
            // 初始化摄像头设备列表
            try
            {
                _videoDevices = new AForgeVideoDirectShow.FilterInfoCollection(AForgeVideoDirectShow.FilterCategory.VideoInputDevice);
                _isCameraInitialized = _videoDevices.Count > 0;
                
                if (_isCameraInitialized)
                {
                    UpdateCameraStatus("摄像头准备就绪\n请对准条形码");
                }
                else
                {
                    UpdateCameraStatus("未检测到摄像头设备\n请检查设备连接");
                }
            }
            catch (Exception ex)
            {
                _isCameraInitialized = false;
                UpdateCameraStatus($"摄像头初始化失败: {ex.Message}");
            }
        }


        private void StartCameraScan()
        {
            try
            {
                // 停止之前可能运行的扫描
                StopCameraScan();

                _isScanning = true;
                
                if (_isCameraInitialized && _videoDevices.Count > 0)
                {
                    // 使用第一个可用的摄像头设备
                    _videoSource = new AForgeVideoDirectShow.VideoCaptureDevice(_videoDevices[0].MonikerString);
                    
                    // 设置视频分辨率为VGA级别，降低内存占用
                    if (_videoSource.VideoCapabilities.Length > 0)
                    {
                        var bestResolution = _videoSource.VideoCapabilities
                            .Where(cap => cap.FrameSize.Width <= 640 && cap.FrameSize.Height <= 480)
                            .OrderByDescending(cap => cap.FrameSize.Width * cap.FrameSize.Height)
                            .FirstOrDefault() ?? _videoSource.VideoCapabilities[0];
                        if (bestResolution != null)
                        {
                            _videoSource.VideoResolution = bestResolution;
                        }
                    }
                    
                    // 注册视频帧事件处理
                    _videoSource.NewFrame += VideoSource_NewFrame;
                    
                    // 启动视频捕获
                    _videoSource.Start();
                    
                    // 启动扫描线程进行条形码识别
                    _scanThread = new System.Threading.Thread(ScanLoop)
                    {
                        Name = "BarcodeScanThread",
                        IsBackground = true
                    };
                    _scanThread.Start();
                    
                    UpdateCameraStatus("扫描已启动\n请将条形码对准摄像头");
                }
                else
                {
                    // 如果没有摄像头设备，使用模拟模式
                    _scanTimer.Start();
                    UpdateCameraStatus("未检测到摄像头\n进入模拟扫描模式");
                }
            }
            catch (Exception ex)
            {
                UpdateCameraStatus($"启动扫描失败: {ex.Message}");
                _isScanning = false;
            }
        }

        private void StopCameraScan()
        {
            try
            {
                _isScanning = false;
                
                // 停止扫描线程（使用更安全的停止方式）
                if (_scanThread != null)
                {
                    // 设置扫描停止标志
                    _isScanning = false;
                    
                    // 尝试优雅地停止线程
                    if (_scanThread.IsAlive)
                    {
                        // 给线程一点时间自己结束
                        if (!_scanThread.Join(500)) // 只等待500ms
                        {
                            // 如果线程没有及时结束，强制中断（但要确保安全）
                            try
                            {
                                _scanThread.Interrupt();
                            }
                            catch
                            {
                                // 忽略中断异常
                            }
                        }
                    }
                    _scanThread = null;
                }
                
                // 停止计时器
                if (_scanTimer != null)
                {
                    _scanTimer.Stop();
                    _scanTimer.Dispose();
                    _scanTimer = null;
                }
                
                // 停止视频源
                if (_videoSource != null)
                {
                    try
                    {
                        if (_videoSource.IsRunning)
                        {
                            _videoSource.SignalToStop();
                            
                            // 等待视频源停止，但不要无限等待
                            for (int i = 0; i < 10; i++) // 最多等待1秒
                            {
                                if (!_videoSource.IsRunning)
                                    break;
                                System.Threading.Thread.Sleep(100);
                            }
                        }
                        _videoSource.NewFrame -= VideoSource_NewFrame;
                        _videoSource = null;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"停止视频源异常: {ex.Message}");
                        _videoSource = null;
                    }
                }
                
                // 释放_lastFrame资源
                lock (this)
                {
                    if (_lastFrame != null)
                    {
                        _lastFrame.Dispose();
                        _lastFrame = null;
                    }
                }
                
                // 清空摄像头图像
                if (_cameraPictureBox != null && !_cameraPictureBox.IsDisposed)
                {
                    SafeUpdateCameraImage(null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"停止摄像头扫描异常: {ex.Message}");
            }
        }

        private void UpdateCameraStatus(string status)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateCameraStatus(status)));
                return;
            }

            // 在摄像头画面上显示状态信息
            if (_cameraPictureBox != null && !this.IsDisposed)
            {
                using (var bitmap = new Bitmap(_cameraPictureBox.Width, _cameraPictureBox.Height))
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(Color.Black);
                    
                    using (var font = new Font("微软雅黑", 12, FontStyle.Bold))
                    using (var brush = new SolidBrush(Color.White))
                    {
                        var textSize = graphics.MeasureString(status, font);
                        var x = (bitmap.Width - textSize.Width) / 2;
                        var y = (bitmap.Height - textSize.Height) / 2;
                        
                        graphics.DrawString(status, font, brush, x, y);
                    }
                    
                    if (_cameraPictureBox.Image != null)
                    {
                        _cameraPictureBox.Image.Dispose();
                    }
                    _cameraPictureBox.Image = new Bitmap(bitmap);
                }
            }
        }

        private void ProcessCameraFrame()
        {
            if (!_isScanning || this.IsDisposed || this.IsDisposed)
                return;

            try
            {
                // 只有在没有使用真实摄像头时才运行模拟模式
                if (_videoSource == null || !_videoSource.IsRunning)
                {
                    UpdateCameraStatus("模拟扫描中...\n请使用真实摄像头以获得最佳体验");
                    
                    // 创建一个简单的扫描动画效果
                    if (_cameraPictureBox != null && !this.IsDisposed && !_cameraPictureBox.IsDisposed)
                    {
                        using (var bitmap = new Bitmap(_cameraPictureBox.Width, _cameraPictureBox.Height))
                        using (var graphics = Graphics.FromImage(bitmap))
                        {
                            graphics.Clear(Color.Black);
                            
                            // 绘制扫描线动画
                            using (var pen = new Pen(Color.Lime, 2))
                            {
                                var scanY = (DateTime.Now.Millisecond % 1000) * bitmap.Height / 1000;
                                graphics.DrawLine(pen, 0, scanY, bitmap.Width, scanY);
                            }
                            
                            // 更新画面
                            if (_cameraPictureBox.InvokeRequired)
                            {
                                _cameraPictureBox.Invoke(new Action<Bitmap>((bmp) =>
                                {
                                    if (_cameraPictureBox != null && !_cameraPictureBox.IsDisposed)
                                    {
                                        if (_cameraPictureBox.Image != null)
                                            _cameraPictureBox.Image.Dispose();
                                        _cameraPictureBox.Image = new Bitmap(bmp);
                                    }
                                }), bitmap);
                            }
                            else
                            {
                                if (_cameraPictureBox != null && !_cameraPictureBox.IsDisposed)
                                {
                                    if (_cameraPictureBox.Image != null)
                                        _cameraPictureBox.Image.Dispose();
                                    _cameraPictureBox.Image = new Bitmap(bitmap);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 忽略处理异常
                System.Diagnostics.Debug.WriteLine($"扫描处理异常: {ex.Message}");
            }
        }
        
        private void ScanLoop()
        {
            // 控制扫描频率，避免过高的CPU使用率
            var scanInterval = TimeSpan.FromMilliseconds(200); // 每200ms扫描一次
            var lastScanTime = DateTime.Now;
            
            while (_isScanning && !this.IsDisposed)
            {
                try
                {
                    // 更频繁地检查退出条件
                    if (!_isScanning || this.IsDisposed)
                        break;
                        
                    // 控制扫描频率
                    var now = DateTime.Now;
                    if (now - lastScanTime < scanInterval)
                    {
                        System.Threading.Thread.Sleep(20); // 稍微增加睡眠时间
                        continue;
                    }
                    lastScanTime = now;
                    
                    Bitmap bitmap = null;
                    try
                    {
                        // 安全地获取当前图像
                        lock (this)
                        {
                            if (_lastFrame != null)
                            {
                                bitmap = (Bitmap)_lastFrame.Clone();
                            }
                        }
                        
                        if (bitmap != null)
                        {
                            // 尝试解码条形码
                            var result = _barcodeReader.Decode(bitmap);
                            
                            if (result != null && !string.IsNullOrEmpty(result.Text))
                            {
                                // 扫描成功
                                string barcode = result.Text;
                                
                                // 防止短时间内重复扫描相同的条形码
                                if (DateTime.Now - _lastScanTime >= TimeSpan.FromSeconds(2))
                                {
                                    if (!this.IsDisposed)
                                    {
                                        this.Invoke(new Action<string>((barcodeValue) =>
                                        {
                                            if (!this.IsDisposed)
                                            {
                                                HandleScannedBarcode(barcodeValue);
                                            }
                                        }), barcode);
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        // 确保bitmap被释放
                        bitmap?.Dispose();
                    }
                    
                    // 短暂暂停以减少CPU使用率
                    System.Threading.Thread.Sleep(10);
                }
                catch (ThreadInterruptedException)
                {
                    // 线程被中断，正常退出
                    break;
                }
                catch (Exception ex)
                {
                    // 忽略扫描过程中的其他错误
                    System.Diagnostics.Debug.WriteLine($"条形码扫描异常: {ex.Message}");
                    System.Threading.Thread.Sleep(50);
                }
            }
            
            System.Diagnostics.Debug.WriteLine("扫描线程已安全退出");
        }
        
        private void ProcessCameraFrame(Bitmap bitmap)
        {
            // 这里可以添加图像处理逻辑
            // 实际的条形码扫描在ScanLoop线程中进行
        }
        
        private void VideoSource_NewFrame(object sender, AForgeVideo.NewFrameEventArgs eventArgs)
        {
            if (this.IsDisposed || _cameraPictureBox == null || _cameraPictureBox.IsDisposed)
                return;
            
            // 复制帧以避免并发问题
            Bitmap bitmap = null;
            try
            {
                bitmap = (Bitmap)eventArgs.Frame.Clone();
                
                // 存储最新帧用于扫描
                lock (this)
                {
                    if (_lastFrame != null)
                    {
                        _lastFrame.Dispose();
                    }
                    _lastFrame = bitmap;
                    bitmap = null; // 不再需要这个引用，因为已经复制到_lastFrame
                }
                
                // 使用_lastFrame的副本进行UI更新
                if (_lastFrame != null)
                {
                    // 创建新的位图副本用于UI显示
                    using (var displayBitmap = new Bitmap(_lastFrame))
                    {
                        SafeUpdateCameraImage(new Bitmap(displayBitmap));
                    }
                    
                    // 处理摄像头帧（可用于添加图像处理逻辑）
                    using (var processBitmap = new Bitmap(_lastFrame))
                    {
                        ProcessCameraFrame(processBitmap);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"处理视频帧异常: {ex.Message}");
                if (bitmap != null)
                {
                    bitmap.Dispose();
                }
            }
        }
        
        private void SafeUpdateCameraImage(Bitmap newImage)
        {
            try
            {
                if (_cameraPictureBox == null || this.IsDisposed || _cameraPictureBox.IsDisposed)
                {
                    newImage?.Dispose();
                    return;
                }
                
                Bitmap imageToDisplay = null;
                
                try
                {
                    // 对图像进行水平翻转（镜像）处理
                    if (newImage != null)
                    {
                        // 创建新的位图用于镜像处理
                        imageToDisplay = new Bitmap(newImage.Width, newImage.Height);
                        
                        using (Graphics g = Graphics.FromImage(imageToDisplay))
                        {
                            // 水平翻转
                            g.ScaleTransform(-1.0f, 1.0f);
                            g.TranslateTransform(-newImage.Width, 0);
                            
                            // 绘制原图像
                            g.DrawImage(newImage, 0, 0, newImage.Width, newImage.Height);
                        }
                        
                        // 释放原始图像
                        newImage.Dispose();
                    }
                    
                    // 安全的UI更新
                    if (_cameraPictureBox.InvokeRequired)
                    {
                        _cameraPictureBox.Invoke(new Action(() => 
                        {
                            if (_cameraPictureBox != null && !_cameraPictureBox.IsDisposed)
                            {
                                // 释放旧图像
                                if (_cameraPictureBox.Image != null)
                                {
                                    var oldImage = _cameraPictureBox.Image;
                                    _cameraPictureBox.Image = null;
                                    oldImage.Dispose();
                                }
                                
                                // 设置新图像
                                _cameraPictureBox.Image = imageToDisplay;
                                imageToDisplay = null; // 图像所有权已转移
                            }
                        }));
                    }
                    else
                    {
                        if (_cameraPictureBox != null && !_cameraPictureBox.IsDisposed)
                        {
                            // 释放旧图像
                            if (_cameraPictureBox.Image != null)
                            {
                                var oldImage = _cameraPictureBox.Image;
                                _cameraPictureBox.Image = null;
                                oldImage.Dispose();
                            }
                            
                            // 设置新图像
                            _cameraPictureBox.Image = imageToDisplay;
                            imageToDisplay = null; // 图像所有权已转移
                        }
                    }
                }
                finally
                {
                    // 确保未使用的图像资源被释放
                    imageToDisplay?.Dispose();
                    newImage?.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新摄像头图像失败: {ex.Message}");
                newImage?.Dispose();
            }
        }

        private void HandleScannedBarcode(string barcode)
        {
            try
            {
                // 清理条形码，移除可能的空格
                barcode = barcode?.Trim();
                if (string.IsNullOrEmpty(barcode))
                {
                    return;
                }

                // 防止重复扫描相同的条形码（2秒内）
                if (DateTime.Now - _lastScanTime < TimeSpan.FromSeconds(2))
                {
                    return;
                }
                _lastScanTime = DateTime.Now;
                
                // 显示扫描到的条形码（调试用）
                System.Diagnostics.Debug.WriteLine($"扫描到条形码: {barcode}");

                // 先验证商品是否存在，避免不必要的弹窗
                Product product = _saleService.GetProductByBarcode(barcode);
                if (product == null)
                {
                    // 商品不存在，静默处理
                    UpdateCameraStatus($"未识别到有效商品\n条形码: {barcode}");
                }
                else
                {
                    // 商品存在，自动添加到购物车
                    _txtProductCode.Text = barcode;
                    
                    // 检查库存
                    int quantity = (int)_numQuantity.Value;
                    if (_saleService.CheckStock(product.ProductCode, quantity))
                    {
                        // 添加商品到购物车
                        var existingItem = _cartItems.FirstOrDefault(item => item.ProductCode == product.ProductCode);
                        if (existingItem != null)
                        {
                            existingItem.Quantity += quantity;
                            existingItem.CalculateAmount();
                        }
                        else
                        {
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

                        // 只在成功添加时播放轻柔提示音
                        try
                        {
                            System.Media.SystemSounds.Asterisk.Play(); // 更轻柔的提示音
                        }
                        catch { }

                        UpdateCameraStatus($"商品添加成功\n{product.Name} x{quantity}");
                    }
                    else
                    {
                        UpdateCameraStatus($"库存不足\n{product.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateCameraStatus($"扫描失败: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                // 在窗体关闭前停止所有后台任务
                StopCameraScan();
                
                // 确保所有事件处理程序被取消订阅
                if (_videoSource != null)
                {
                    _videoSource.NewFrame -= VideoSource_NewFrame;
                }
                
                // 释放UI资源
                if (_cameraPictureBox != null && !_cameraPictureBox.IsDisposed)
                {
                    if (_cameraPictureBox.Image != null)
                    {
                        var oldImage = _cameraPictureBox.Image;
                        _cameraPictureBox.Image = null;
                        oldImage.Dispose();
                    }
                }
                
                base.OnFormClosing(e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"窗体关闭异常: {ex.Message}");
                base.OnFormClosing(e);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                // 确保所有资源被释放
                StopCameraScan();
                
                // 清理其他可能的资源
                if (_scanTimer != null)
                {
                    _scanTimer.Stop();
                    _scanTimer.Dispose();
                    _scanTimer = null;
                }
                
                base.OnFormClosed(e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"窗体关闭后清理异常: {ex.Message}");
                base.OnFormClosed(e);
            }
        }
    }
}