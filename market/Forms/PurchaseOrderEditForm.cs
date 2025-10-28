using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using market.Models;
using market.Services;

namespace market.Forms
{
    public partial class PurchaseOrderEditForm : Form
    {
        private readonly DatabaseService _databaseService;
        private readonly AuthService _authService;
        private readonly PurchaseService _purchaseService;
        private readonly ProductService _productService;
        
        private PurchaseOrder _order;
        private bool _isEditMode;

        private List<PurchaseOrderItem> _items = new List<PurchaseOrderItem>();

        public PurchaseOrderEditForm(DatabaseService databaseService, AuthService authService, PurchaseOrder order = null)
        {
            InitializeComponent();
            
            _databaseService = databaseService;
            _authService = authService;
            _purchaseService = new PurchaseService(databaseService);
            _productService = new ProductService(databaseService);
            
            _order = order;
            _isEditMode = order != null;
            
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = _isEditMode ? "编辑进货单" : "新建进货单";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 创建主布局
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // 创建基本信息面板
            var basicInfoPanel = CreateBasicInfoPanel();
            mainPanel.Controls.Add(basicInfoPanel);

            // 创建商品明细面板
            var itemsPanel = CreateItemsPanel();
            mainPanel.Controls.Add(itemsPanel);

            // 创建按钮面板
            var buttonPanel = CreateButtonPanel();
            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);

            // 初始化数据
            InitializeData();
        }

        private Panel CreateBasicInfoPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BorderStyle = BorderStyle.FixedSingle
            };

            // 进货单号
            var lblOrderNumber = new Label { Text = "进货单号:", Location = new Point(10, 15), Width = 70 };
            var txtOrderNumber = new TextBox { Location = new Point(80, 12), Width = 150, ReadOnly = _isEditMode };

            // 进货日期
            var lblOrderDate = new Label { Text = "进货日期:", Location = new Point(250, 15), Width = 70 };
            var dtpOrderDate = new DateTimePicker { Location = new Point(320, 12), Width = 120, Format = DateTimePickerFormat.Short };

            // 供应商
            var lblSupplier = new Label { Text = "供应商:", Location = new Point(470, 15), Width = 70 };
            var cmbSupplier = new ComboBox { Location = new Point(540, 12), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            // 税率
            var lblTaxRate = new Label { Text = "税率(%):", Location = new Point(770, 15), Width = 70 };
            var numTaxRate = new NumericUpDown { Location = new Point(840, 12), Width = 80, Minimum = 0, Maximum = 100, DecimalPlaces = 2 };

            // 备注
            var lblNotes = new Label { Text = "备注:", Location = new Point(10, 50), Width = 70 };
            var txtNotes = new TextBox { Location = new Point(80, 47), Width = 400, Height = 60, Multiline = true, ScrollBars = ScrollBars.Vertical };

            // 合计信息
            var lblTotalAmount = new Label { Text = "合计金额:", Location = new Point(500, 50), Width = 70 };
            var lblTotalAmountValue = new Label { Location = new Point(580, 50), Width = 100, Text = "￥0.00", Font = new Font("微软雅黑", 10, FontStyle.Bold) };

            var lblTaxAmount = new Label { Text = "税额:", Location = new Point(500, 80), Width = 70 };
            var lblTaxAmountValue = new Label { Location = new Point(580, 80), Width = 100, Text = "￥0.00" };

            var lblFinalAmount = new Label { Text = "最终金额:", Location = new Point(700, 50), Width = 70 };
            var lblFinalAmountValue = new Label { Location = new Point(780, 50), Width = 100, Text = "￥0.00", Font = new Font("微软雅黑", 10, FontStyle.Bold), ForeColor = Color.Blue };

            panel.Controls.AddRange(new Control[] {
                lblOrderNumber, txtOrderNumber,
                lblOrderDate, dtpOrderDate,
                lblSupplier, cmbSupplier,
                lblTaxRate, numTaxRate,
                lblNotes, txtNotes,
                lblTotalAmount, lblTotalAmountValue,
                lblTaxAmount, lblTaxAmountValue,
                lblFinalAmount, lblFinalAmountValue
            });

            // 事件处理
            numTaxRate.ValueChanged += (s, e) => CalculateAmounts();

            // 加载供应商列表
            LoadSuppliers(cmbSupplier);

            return panel;
        }

        private Panel CreateItemsPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };

            // 商品明细标题
            var lblItems = new Label { Text = "商品明细", Location = new Point(10, 10), Font = new Font("微软雅黑", 10, FontStyle.Bold) };

            // 商品明细数据网格
            var dataGridView = new DataGridView
            {
                Location = new Point(10, 40),
                Size = new Size(950, 400), // 固定高度，避免依赖面板高度
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // 添加列
            dataGridView.Columns.Add("ProductCode", "商品编码");
            dataGridView.Columns.Add("ProductName", "商品名称");
            dataGridView.Columns.Add("Quantity", "数量");
            dataGridView.Columns.Add("PurchasePrice", "进货单价");
            dataGridView.Columns.Add("Amount", "金额");
            dataGridView.Columns.Add("BatchNumber", "批次号");

            // 操作按钮 - 使用固定位置
            var btnAddItem = new Button { Text = "添加商品", Location = new Point(10, 450), Size = new Size(80, 30) };
            var btnEditItem = new Button { Text = "编辑", Location = new Point(100, 450), Size = new Size(60, 30) };
            var btnDeleteItem = new Button { Text = "删除", Location = new Point(170, 450), Size = new Size(60, 30) };

            panel.Controls.AddRange(new Control[] { lblItems, dataGridView, btnAddItem, btnEditItem, btnDeleteItem });

            // 事件处理
            btnAddItem.Click += (s, e) => AddItem();
            btnEditItem.Click += (s, e) => EditItem();
            btnDeleteItem.Click += (s, e) => DeleteItem();

            return panel;
        }

        private Panel CreateButtonPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BorderStyle = BorderStyle.FixedSingle
            };

            var btnSave = new Button { Text = "保存", Size = new Size(80, 30), Location = new Point(300, 10) };
            var btnCancel = new Button { Text = "取消", Size = new Size(80, 30), Location = new Point(400, 10) };

            panel.Controls.AddRange(new Control[] { btnSave, btnCancel });

            // 事件处理
            btnSave.Click += (s, e) => SaveOrder();
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            return panel;
        }

        private void InitializeData()
        {
            if (_isEditMode)
            {
                // 编辑模式：加载现有数据
                var basicPanel = GetBasicInfoPanel();
                var txtOrderNumber = basicPanel.Controls[1] as TextBox;
                var dtpOrderDate = basicPanel.Controls[3] as DateTimePicker;
                var cmbSupplier = basicPanel.Controls[5] as ComboBox;
                var numTaxRate = basicPanel.Controls[7] as NumericUpDown;
                var txtNotes = basicPanel.Controls[9] as TextBox;

                txtOrderNumber.Text = _order.OrderNumber;
                dtpOrderDate.Value = _order.OrderDate;
                cmbSupplier.SelectedValue = _order.SupplierId;
                numTaxRate.Value = _order.TaxAmount / _order.TotalAmount * 100;
                txtNotes.Text = _order.Notes ?? "";

                _items = _order.Items;
                RefreshItemsGrid();
                CalculateAmounts();
            }
            else
            {
                // 新建模式：设置默认值
                var basicPanel = GetBasicInfoPanel();
                var txtOrderNumber = basicPanel.Controls[1] as TextBox;
                var dtpOrderDate = basicPanel.Controls[3] as DateTimePicker;

                txtOrderNumber.Text = _purchaseService.GeneratePurchaseOrderNumber();
                dtpOrderDate.Value = DateTime.Today;
            }
        }

        private void LoadSuppliers(ComboBox comboBox)
        {
            try
            {
                var suppliers = _productService.GetAllSuppliers();
                comboBox.DataSource = suppliers;
                comboBox.DisplayMember = "Name";
                comboBox.ValueMember = "Id";
                comboBox.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载供应商列表失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddItem()
        {
            var itemForm = new PurchaseOrderItemEditForm(_productService, null);
            if (itemForm.ShowDialog() == DialogResult.OK)
            {
                _items.Add(itemForm.Item);
                RefreshItemsGrid();
                CalculateAmounts();
            }
        }

        private void EditItem()
        {
            var selectedItem = GetSelectedItem();
            if (selectedItem == null) return;

            var itemForm = new PurchaseOrderItemEditForm(_productService, selectedItem);
            if (itemForm.ShowDialog() == DialogResult.OK)
            {
                RefreshItemsGrid();
                CalculateAmounts();
            }
        }

        private void DeleteItem()
        {
            var selectedItem = GetSelectedItem();
            if (selectedItem == null) return;

            if (MessageBox.Show("确定要删除此商品吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _items.Remove(selectedItem);
                RefreshItemsGrid();
                CalculateAmounts();
            }
        }

        private PurchaseOrderItem GetSelectedItem()
        {
            var itemsPanel = GetItemsPanel();
            var dataGridView = itemsPanel.Controls[1] as DataGridView;

            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择一条商品记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            var productCode = dataGridView.SelectedRows[0].Cells["ProductCode"].Value.ToString();
            return _items.FirstOrDefault(i => i.ProductCode == productCode);
        }

        private void RefreshItemsGrid()
        {
            var itemsPanel = GetItemsPanel();
            var dataGridView = itemsPanel.Controls[1] as DataGridView;

            dataGridView.Rows.Clear();
            foreach (var item in _items)
            {
                dataGridView.Rows.Add(
                    item.ProductCode,
                    item.ProductName,
                    item.Quantity,
                    item.PurchasePrice,
                    item.Amount,
                    item.BatchNumber
                );
            }
        }

        private void CalculateAmounts()
        {
            decimal totalAmount = _items.Sum(i => i.Amount);
            var basicPanel = GetBasicInfoPanel();
            var numTaxRate = basicPanel.Controls[7] as NumericUpDown;
            
            decimal taxRate = numTaxRate.Value / 100;
            decimal taxAmount = totalAmount * taxRate;
            decimal finalAmount = totalAmount + taxAmount;

            var lblTotalAmountValue = basicPanel.Controls[11] as Label;
            var lblTaxAmountValue = basicPanel.Controls[13] as Label;
            var lblFinalAmountValue = basicPanel.Controls[15] as Label;

            lblTotalAmountValue.Text = $"￥{totalAmount:F2}";
            lblTaxAmountValue.Text = $"￥{taxAmount:F2}";
            lblFinalAmountValue.Text = $"￥{finalAmount:F2}";
        }

        private void SaveOrder()
        {
            if (!ValidateForm()) return;

            try
            {
                var order = CreateOrderFromForm();

                if (_isEditMode)
                {
                    // 更新现有订单
                    if (_purchaseService.UpdatePurchaseOrderStatus(order.OrderNumber, PurchaseOrderStatus.Pending, _authService.CurrentUser.Id))
                    {
                        MessageBox.Show("保存成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                    }
                }
                else
                {
                    // 创建新订单
                    if (_purchaseService.CreatePurchaseOrder(order))
                    {
                        MessageBox.Show("创建成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            var basicPanel = GetBasicInfoPanel();
            var cmbSupplier = basicPanel.Controls[5] as ComboBox;

            if (cmbSupplier.SelectedIndex == -1)
            {
                MessageBox.Show("请选择供应商", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_items.Count == 0)
            {
                MessageBox.Show("请至少添加一个商品", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private PurchaseOrder CreateOrderFromForm()
        {
            var basicPanel = GetBasicInfoPanel();
            var txtOrderNumber = basicPanel.Controls[1] as TextBox;
            var dtpOrderDate = basicPanel.Controls[3] as DateTimePicker;
            var cmbSupplier = basicPanel.Controls[5] as ComboBox;
            var numTaxRate = basicPanel.Controls[7] as NumericUpDown;
            var txtNotes = basicPanel.Controls[9] as TextBox;

            var order = new PurchaseOrder
            {
                OrderNumber = txtOrderNumber.Text,
                OrderDate = dtpOrderDate.Value,
                SupplierId = cmbSupplier.SelectedValue.ToString(),
                OperatorId = _authService.CurrentUser.Id,
                Status = PurchaseOrderStatus.Pending,
                Notes = txtNotes.Text,
                Items = _items
            };

            // 计算金额
            decimal taxRate = numTaxRate.Value / 100;
            order.TotalAmount = _items.Sum(i => i.Amount);
            order.TaxAmount = order.TotalAmount * taxRate;
            order.FinalAmount = order.TotalAmount + order.TaxAmount;

            return order;
        }

        private Panel GetBasicInfoPanel()
        {
            var mainPanel = this.Controls[0] as Panel;
            return mainPanel.Controls[0] as Panel;
        }

        private Panel GetItemsPanel()
        {
            var mainPanel = this.Controls[0] as Panel;
            return mainPanel.Controls[1] as Panel;
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // PurchaseOrderEditForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "PurchaseOrderEditForm";
            this.ResumeLayout(false);
        }
        #endregion
    }
}