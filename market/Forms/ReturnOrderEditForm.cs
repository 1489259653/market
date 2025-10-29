using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using market.Models;
using market.Services;

namespace market.Forms
{
    public partial class ReturnOrderEditForm : Form
    {
        private readonly ReturnService _returnService;
        private readonly AuthService _authService;
        private readonly ProductService _productService;
        
        private ReturnOrder _returnOrder;
        private List<ReturnOrderItem> _returnItems;
        private bool _isEditMode = false;
        
        private TextBox txtReturnNumber;
        private TextBox txtOriginalOrderNumber;
        private TextBox txtCustomer;
        private TextBox txtReason;
        private TextBox txtNotes;
        private DateTimePicker dtpReturnDate;
        private ComboBox cmbStatus;
        private DataGridView dgvReturnItems;
        private TextBox txtSearchProduct;
        private Button btnSearchProduct;
        private Button btnAddItem;
        private Button btnRemoveItem;
        private Label lblTotalAmount;
        private Button btnSave;
        private Button btnCancel;

        public ReturnOrderEditForm(ReturnService returnService, AuthService authService, ProductService productService, string returnNumber = null)
        {
            _returnService = returnService;
            _authService = authService;
            _productService = productService;
            _returnItems = new List<ReturnOrderItem>();
            
            if (!string.IsNullOrEmpty(returnNumber))
            {
                // 编辑模式
                _isEditMode = true;
                _returnOrder = _returnService.GetReturnOrder(returnNumber);
                if (_returnOrder != null)
                {
                    _returnItems = _returnOrder.Items;
                }
            }
            else
            {
                // 新建模式
                _isEditMode = false;
                _returnOrder = new ReturnOrder
                {
                    ReturnNumber = _returnService.GenerateReturnOrderNumber(),
                    ReturnDate = DateTime.Now,
                    Customer = "散客",
                    OperatorId = _authService.CurrentUser?.Id ?? "",
                    OperatorName = _authService.CurrentUser?.Username ?? "",
                    Status = ReturnOrderStatus.Pending,
                    Reason = "质量问题",
                    CreatedAt = DateTime.Now
                };
            }
            
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            // 窗体基本设置
            this.Text = _returnOrder.ReturnNumber == null ? "新建退货订单" : $"编辑退货订单 - {_returnOrder.ReturnNumber}";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // 创建主面板
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0,210,0,0)
            };

            // 退货单信息面板
            var infoPanel = new GroupBox
            {
                Text = "退货单信息",
                Location = new Point(10, 20),
                Size = new Size(760, 120),
                Font = new Font("Microsoft YaHei", 9f, FontStyle.Bold)
            };

            // 退货单号
            var lblReturnNumber = new Label { Text = "退货单号:", Location = new Point(10, 25), Size = new Size(80, 20) };
            txtReturnNumber = new TextBox { Location = new Point(90, 22), Size = new Size(150, 25), ReadOnly = true };

            // 原销售单号
            var lblOriginalOrderNumber = new Label { Text = "原销售单号:", Location = new Point(250, 25), Size = new Size(80, 20) };
            txtOriginalOrderNumber = new TextBox { Location = new Point(330, 22), Size = new Size(150, 25) };
            txtOriginalOrderNumber.TextChanged += TxtOriginalOrderNumber_TextChanged;

            // 顾客姓名
            var lblCustomer = new Label { Text = "顾客姓名:", Location = new Point(490, 25), Size = new Size(80, 20) };
            txtCustomer = new TextBox { Location = new Point(570, 22), Size = new Size(150, 25) };

            // 退货日期
            var lblReturnDate = new Label { Text = "退货日期:", Location = new Point(10, 60), Size = new Size(80, 20) };
            dtpReturnDate = new DateTimePicker { Location = new Point(90, 57), Size = new Size(150, 25), Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm" };

            // 退货状态
            var lblStatus = new Label { Text = "退货状态:", Location = new Point(250, 60), Size = new Size(80, 20) };
            cmbStatus = new ComboBox { Location = new Point(330, 57), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "待处理", "已审核", "已完成", "已取消" });

            // 退货原因
            var lblReason = new Label { Text = "退货原因:", Location = new Point(490, 60), Size = new Size(80, 20) };
            txtReason = new TextBox { Location = new Point(570, 57), Size = new Size(150, 25) };

            // 备注
            var lblNotes = new Label { Text = "备注:", Location = new Point(10, 95), Size = new Size(80, 20) };
            txtNotes = new TextBox { Location = new Point(90, 92), Size = new Size(630, 25) };

            // 添加信息面板控件
            infoPanel.Controls.AddRange(new Control[] {
                lblReturnNumber, txtReturnNumber,
                lblOriginalOrderNumber, txtOriginalOrderNumber,
                lblCustomer, txtCustomer,
                lblReturnDate, dtpReturnDate,
                lblStatus, cmbStatus,
                lblReason, txtReason,
                lblNotes, txtNotes
            });

            // 退货商品面板
            var itemsPanel = new GroupBox
            {
                Text = "退货商品明细",
                Location = new Point(10, 150),
                Size = new Size(760, 300),
                Font = new Font("Microsoft YaHei", 9f, FontStyle.Bold)
            };

            // 商品搜索
            var lblSearchProduct = new Label { Text = "商品搜索:", Location = new Point(10, 25), Size = new Size(80, 20) };
            txtSearchProduct = new TextBox { Location = new Point(90, 22), Size = new Size(200, 25) };
            btnSearchProduct = new Button { Text = "搜索", Location = new Point(300, 22), Size = new Size(60, 25) };
            btnSearchProduct.Click += BtnSearchProduct_Click;

            // 添加商品按钮
            btnAddItem = new Button { Text = "添加商品", Location = new Point(370, 22), Size = new Size(80, 25) };
            btnAddItem.Click += BtnAddItem_Click;

            // 移除商品按钮
            btnRemoveItem = new Button { Text = "移除商品", Location = new Point(460, 22), Size = new Size(80, 25) };
            btnRemoveItem.Click += BtnRemoveItem_Click;

            // 退货商品数据表格
            dgvReturnItems = new DataGridView
            {
                Location = new Point(10, 55),
                Size = new Size(740, 200),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // 添加列
            dgvReturnItems.Columns.Add("ProductCode", "商品编码");
            dgvReturnItems.Columns.Add("ProductName", "商品名称");
            dgvReturnItems.Columns.Add("Quantity", "退货数量");
            dgvReturnItems.Columns.Add("ReturnPrice", "退货单价");
            dgvReturnItems.Columns.Add("Amount", "退货金额");
            dgvReturnItems.Columns.Add("Reason", "退货原因");

            // 总金额标签
            lblTotalAmount = new Label
            {
                Text = "退货总金额: ¥0.00",
                Location = new Point(10, 265),
                Size = new Size(200, 20),
                Font = new Font("Microsoft YaHei", 10f, FontStyle.Bold),
                ForeColor = Color.Red
            };

            // 添加商品面板控件
            itemsPanel.Controls.AddRange(new Control[] {
                lblSearchProduct, txtSearchProduct, btnSearchProduct,
                btnAddItem, btnRemoveItem,
                dgvReturnItems,
                lblTotalAmount
            });

            // 按钮面板
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.White
            };

            // 保存按钮
            btnSave = new Button
            {
                Text = "保存",
                Location = new Point(500, 10),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White
            };
            btnSave.Click += BtnSave_Click;

            // 取消按钮
            btnCancel = new Button
            {
                Text = "取消",
                Location = new Point(590, 10),
                Size = new Size(80, 30),
                BackColor = Color.Gray,
                ForeColor = Color.White
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            buttonPanel.Controls.AddRange(new Control[] { btnSave, btnCancel });

            // 添加所有面板到主面板
            mainPanel.Controls.AddRange(new Control[] { infoPanel, itemsPanel });
            this.Controls.AddRange(new Control[] { mainPanel, buttonPanel });
        }

        private void LoadData()
        {
            // 加载退货订单信息
            txtReturnNumber.Text = _returnOrder.ReturnNumber;
            txtOriginalOrderNumber.Text = _returnOrder.OriginalOrderNumber;
            txtCustomer.Text = _returnOrder.Customer;
            txtReason.Text = _returnOrder.Reason;
            txtNotes.Text = _returnOrder.Notes;
            dtpReturnDate.Value = _returnOrder.ReturnDate;
            cmbStatus.SelectedIndex = (int)_returnOrder.Status;

            // 加载退货商品明细
            LoadReturnItems();
            CalculateTotalAmount();
        }

        private void LoadReturnItems()
        {
            dgvReturnItems.Rows.Clear();
            
            foreach (var item in _returnItems)
            {
                dgvReturnItems.Rows.Add(
                    item.ProductCode,
                    item.ProductName,
                    item.Quantity,
                    item.ReturnPrice,
                    item.Amount,
                    item.Reason
                );
            }
        }

        private void CalculateTotalAmount()
        {
            decimal totalAmount = _returnItems.Sum(item => item.Amount);
            lblTotalAmount.Text = $"退货总金额: ¥{totalAmount:F2}";
            _returnOrder.TotalAmount = totalAmount;
            _returnOrder.RefundAmount = totalAmount;
        }

        private void TxtOriginalOrderNumber_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var orderNumber = txtOriginalOrderNumber.Text.Trim();
                if (!string.IsNullOrEmpty(orderNumber))
                {
                    var saleOrder = _returnService.GetSaleOrderByNumber(orderNumber);
                    if (saleOrder != null)
                    {
                        txtCustomer.Text = saleOrder.Customer;
                        _returnOrder.Customer = saleOrder.Customer;
                        
                        // 加载销售订单的商品明细
                        var saleItems = _returnService.GetSaleOrderItems(orderNumber);
                        // 这里可以实现自动填充退货商品的功能
                    }
                }
            }
            catch (Exception)
            {
                // 忽略错误，可能订单号不存在
            }
        }

        private void BtnSearchProduct_Click(object sender, EventArgs e)
        {
            // 实现商品搜索功能
            MessageBox.Show("商品搜索功能待实现", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            try
            {
                var returnItemForm = new ReturnOrderItemEditForm(_productService);
                if (returnItemForm.ShowDialog() == DialogResult.OK)
                {
                    var newItem = returnItemForm.GetReturnItem();
                    if (newItem != null)
                    {
                        _returnItems.Add(newItem);
                        LoadReturnItems();
                        CalculateTotalAmount();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加退货商品失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvReturnItems.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择要移除的退货商品", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var selectedIndex = dgvReturnItems.SelectedRows[0].Index;
                if (selectedIndex >= 0 && selectedIndex < _returnItems.Count)
                {
                    _returnItems.RemoveAt(selectedIndex);
                    LoadReturnItems();
                    CalculateTotalAmount();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移除退货商品失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // 验证数据
                if (string.IsNullOrEmpty(txtOriginalOrderNumber.Text))
                {
                    MessageBox.Show("请输入原销售单号", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtOriginalOrderNumber.Focus();
                    return;
                }

                if (_returnItems.Count == 0)
                {
                    MessageBox.Show("请添加退货商品", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 更新退货订单信息
                _returnOrder.OriginalOrderNumber = txtOriginalOrderNumber.Text;
                _returnOrder.Customer = txtCustomer.Text;
                _returnOrder.ReturnDate = dtpReturnDate.Value;
                _returnOrder.Status = (ReturnOrderStatus)cmbStatus.SelectedIndex;
                _returnOrder.Reason = txtReason.Text;
                _returnOrder.Notes = txtNotes.Text;
                _returnOrder.Items = _returnItems;

                // 计算金额
                _returnOrder.CalculateAmounts();

                // 保存退货订单
                bool success;
                if (_isEditMode)
                {
                    // 编辑模式：更新退货订单
                    success = _returnService.UpdateReturnOrder(_returnOrder);
                }
                else
                {
                    // 新建模式：创建退货订单
                    success = _returnService.CreateReturnOrder(_returnOrder);
                }
                
                if (success)
                {
                    MessageBox.Show("退货订单保存成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("退货订单保存失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存退货订单失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 获取退货订单信息
        /// </summary>
        /// <returns>退货订单</returns>
        public ReturnOrder GetReturnOrder()
        {
            return _returnOrder;
        }
    }
}