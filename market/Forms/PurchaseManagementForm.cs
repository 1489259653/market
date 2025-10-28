using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using market.Models;
using market.Services;

namespace market.Forms
{
    public partial class PurchaseManagementForm : Form
    {
        private readonly DatabaseService _databaseService;
        private readonly PurchaseService _purchaseService;
        private readonly ProductService _productService;
        private readonly AuthService _authService;

        private List<PurchaseOrder> _orders = new List<PurchaseOrder>();
        private PurchaseOrderQuery _currentQuery = new PurchaseOrderQuery();

        public PurchaseManagementForm(DatabaseService databaseService, AuthService authService)
        {
            InitializeComponent();
            
            _databaseService = databaseService;
            _authService = authService;
            _purchaseService = new PurchaseService(databaseService);
            _productService = new ProductService(databaseService);
            
            InitializeForm();
        }

        private void InitializeForm()
        {
            // 设置窗体属性
            this.Text = "进货管理";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 创建主布局
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // 创建并添加底部控件（分页面板）
            var pagingPanel = CreatePagingPanel();
            mainPanel.Controls.Add(pagingPanel);

            // 创建并添加中间填充控件（数据网格）
            var dataGridPanel = CreateDataGridPanel();
            mainPanel.Controls.Add(dataGridPanel);

            // 创建并添加顶部控件（按钮面板）
            var buttonPanel = CreateButtonPanel();
            buttonPanel.Dock = DockStyle.Top;
            mainPanel.Controls.Add(buttonPanel);

            // 创建并添加顶部控件（查询面板）
            var queryPanel = CreateQueryPanel();
            mainPanel.Controls.Add(queryPanel);

            // 添加主面板到窗体
            this.Controls.Add(mainPanel);

            // 加载数据
            LoadData();
        }

        private Panel CreateQueryPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BorderStyle = BorderStyle.FixedSingle
            };

            // 进货单号查询
            var lblOrderNumber = new Label { Text = "进货单号:", Location = new Point(10, 15), Width = 70 };
            var txtOrderNumber = new TextBox { Location = new Point(80, 12), Width = 150 };

            // 供应商查询
            var lblSupplier = new Label { Text = "供应商:", Location = new Point(250, 15), Width = 70 };
            var cmbSupplier = new ComboBox { Location = new Point(320, 12), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };

            // 状态查询
            var lblStatus = new Label { Text = "状态:", Location = new Point(490, 15), Width = 50 };
            var cmbStatus = new ComboBox { Location = new Point(540, 12), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "全部", "待审核", "已审核", "已到货", "已完成", "已取消" });
            cmbStatus.SelectedIndex = 0;

            // 日期范围
            var lblDateRange = new Label { Text = "日期范围:", Location = new Point(10, 45), Width = 70 };
            var dtpStartDate = new DateTimePicker { Location = new Point(80, 42), Width = 120, Format = DateTimePickerFormat.Short };
            var lblTo = new Label { Text = "至", Location = new Point(210, 45), Width = 20 };
            var dtpEndDate = new DateTimePicker { Location = new Point(230, 42), Width = 120, Format = DateTimePickerFormat.Short };

            // 查询按钮
            var btnSearch = new Button { Text = "查询", Location = new Point(700, 12), Width = 80 };
            var btnReset = new Button { Text = "重置", Location = new Point(790, 12), Width = 80 };

            // 添加到面板
            panel.Controls.AddRange(new Control[] {
                lblOrderNumber, txtOrderNumber,
                lblSupplier, cmbSupplier,
                lblStatus, cmbStatus,
                lblDateRange, dtpStartDate, lblTo, dtpEndDate,
                btnSearch, btnReset
            });

            // 事件处理
            btnSearch.Click += (s, e) =>
            {
                _currentQuery.OrderNumber = txtOrderNumber.Text.Trim();
                _currentQuery.SupplierId = cmbSupplier.SelectedValue?.ToString();
                _currentQuery.Status = cmbStatus.SelectedIndex > 0 ? (PurchaseOrderStatus?)(cmbStatus.SelectedIndex - 1) : null;
                _currentQuery.StartDate = dtpStartDate.Value.Date;
                _currentQuery.EndDate = dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1);
                _currentQuery.PageIndex = 1;

                LoadData();
            };

            btnReset.Click += (s, e) =>
            {
                txtOrderNumber.Text = "";
                cmbSupplier.SelectedIndex = -1;
                cmbStatus.SelectedIndex = 0;
                dtpStartDate.Value = DateTime.Today.AddDays(-30);
                dtpEndDate.Value = DateTime.Today;

                _currentQuery = new PurchaseOrderQuery();
                LoadData();
            };

            // 加载供应商列表
            LoadSuppliers(cmbSupplier);

            return panel;
        }

        private Panel CreateButtonPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(10, 5, 10, 5)
            };

            var btnAdd = new Button { Text = "新建进货单", Size = new Size(100, 30) };
            var btnEdit = new Button { Text = "编辑", Size = new Size(80, 30), Location = new Point(110, 0) };
            var btnView = new Button { Text = "查看", Size = new Size(80, 30), Location = new Point(200, 0) };
            var btnApprove = new Button { Text = "审核", Size = new Size(80, 30), Location = new Point(290, 0) };
            var btnComplete = new Button { Text = "完成", Size = new Size(80, 30), Location = new Point(380, 0) };
            var btnCancel = new Button { Text = "取消", Size = new Size(80, 30), Location = new Point(470, 0) };
            var btnPrint = new Button { Text = "打印", Size = new Size(80, 30), Location = new Point(560, 0) };

            panel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnView, btnApprove, btnComplete, btnCancel, btnPrint });

            // 事件处理
            btnAdd.Click += (s, e) =>
            {
                var editForm = new PurchaseOrderEditForm(_databaseService, _authService);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            };

            btnEdit.Click += (s, e) => EditSelectedOrder();
            btnView.Click += (s, e) => ViewSelectedOrder();
            btnApprove.Click += (s, e) => ApproveSelectedOrder();
            btnComplete.Click += (s, e) => CompleteSelectedOrder();
            btnCancel.Click += (s, e) => CancelSelectedOrder();

            return panel;
        }

        private Panel CreateDataGridPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };

            var dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // 添加列
            dataGridView.Columns.Add("OrderNumber", "进货单号");
            dataGridView.Columns.Add("OrderDate", "进货日期");
            dataGridView.Columns.Add("SupplierName", "供应商");
            dataGridView.Columns.Add("OperatorName", "操作人");
            dataGridView.Columns.Add("StatusText", "状态");
            dataGridView.Columns.Add("TotalAmount", "总金额");
            dataGridView.Columns.Add("ItemCount", "商品数量");

            // 格式化列
            dataGridView.Columns["OrderDate"].DefaultCellStyle.Format = "yyyy-MM-dd";
            dataGridView.Columns["TotalAmount"].DefaultCellStyle.Format = "C2";

            panel.Controls.Add(dataGridView);
            return panel;
        }

        private Panel CreatePagingPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblPageInfo = new Label { Location = new Point(10, 10), AutoSize = true };
            var btnFirst = new Button { Text = "首页", Location = new Point(200, 5), Size = new Size(60, 25) };
            var btnPrev = new Button { Text = "上页", Location = new Point(270, 5), Size = new Size(60, 25) };
            var btnNext = new Button { Text = "下页", Location = new Point(340, 5), Size = new Size(60, 25) };
            var btnLast = new Button { Text = "末页", Location = new Point(410, 5), Size = new Size(60, 25) };

            panel.Controls.AddRange(new Control[] { lblPageInfo, btnFirst, btnPrev, btnNext, btnLast });

            // 事件处理
            btnFirst.Click += (s, e) => { _currentQuery.PageIndex = 1; LoadData(); };
            btnPrev.Click += (s, e) => { if (_currentQuery.PageIndex > 1) { _currentQuery.PageIndex--; LoadData(); } };
            btnNext.Click += (s, e) => { _currentQuery.PageIndex++; LoadData(); };
            btnLast.Click += (s, e) => { _currentQuery.PageIndex = int.MaxValue; LoadData(); };

            return panel;
        }

        private void LoadData()
        {
            try
            {
                var result = _purchaseService.GetPurchaseOrdersPaged(_currentQuery);
                _orders = result.Orders;

                // 更新数据网格
                var dataGridView = GetDataGridView();
                dataGridView.Rows.Clear();

                foreach (var order in _orders)
                {
                    dataGridView.Rows.Add(
                        order.OrderNumber,
                        order.OrderDate,
                        order.SupplierName,
                        order.OperatorName,
                        order.StatusText,
                        order.TotalAmount,
                        order.Items.Count
                    );
                }

                // 更新分页信息
                UpdatePagingInfo(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private DataGridView GetDataGridView()
        {
            var mainPanel = this.Controls[0] as Panel;
            var dataGridPanel = mainPanel.Controls[1] as Panel;
            return dataGridPanel.Controls[0] as DataGridView;
        }

        private void UpdatePagingInfo(PurchaseOrderListResult result)
        {
            var mainPanel = this.Controls[0] as Panel;
            var pagingPanel = mainPanel.Controls[0] as Panel;
            var lblPageInfo = pagingPanel.Controls[0] as Label;

            lblPageInfo.Text = $"第 {result.CurrentPage} 页，共 {result.TotalPages} 页，总计 {result.TotalCount} 条记录";

            // 更新按钮状态
            var btnFirst = pagingPanel.Controls[1] as Button;
            var btnPrev = pagingPanel.Controls[2] as Button;
            var btnNext = pagingPanel.Controls[3] as Button;
            var btnLast = pagingPanel.Controls[4] as Button;

            btnFirst.Enabled = btnPrev.Enabled = result.CurrentPage > 1;
            btnNext.Enabled = btnLast.Enabled = result.CurrentPage < result.TotalPages;
        }

        private PurchaseOrder GetSelectedOrder()
        {
            var dataGridView = GetDataGridView();
            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择一条记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            var orderNumber = dataGridView.SelectedRows[0].Cells["OrderNumber"].Value.ToString();
            return _purchaseService.GetPurchaseOrder(orderNumber);
        }

        private void EditSelectedOrder()
        {
            var order = GetSelectedOrder();
            if (order == null) return;

            if (order.Status != PurchaseOrderStatus.Pending)
            {
                MessageBox.Show("只能编辑待审核状态的进货单", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var editForm = new PurchaseOrderEditForm(_databaseService, _authService, order);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void ViewSelectedOrder()
        {
            var order = GetSelectedOrder();
            if (order == null) return;

            var viewForm = new PurchaseOrderViewForm(_databaseService, order);
            viewForm.ShowDialog();
        }

        private void ApproveSelectedOrder()
        {
            var order = GetSelectedOrder();
            if (order == null) return;

            if (order.Status != PurchaseOrderStatus.Pending)
            {
                MessageBox.Show("只能审核待审核状态的进货单", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("确定要审核此进货单吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    if (_purchaseService.UpdatePurchaseOrderStatus(order.OrderNumber, PurchaseOrderStatus.Approved, _authService.CurrentUser.Id))
                    {
                        MessageBox.Show("审核成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"审核失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CompleteSelectedOrder()
        {
            var order = GetSelectedOrder();
            if (order == null) return;

            if (order.Status != PurchaseOrderStatus.Approved && order.Status != PurchaseOrderStatus.Delivered)
            {
                MessageBox.Show("只能完成已审核或已到货状态的进货单", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("确定要完成此进货单吗？完成后将更新库存", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    if (_purchaseService.CompletePurchaseOrder(order.OrderNumber, _authService.CurrentUser.Id))
                    {
                        MessageBox.Show("完成成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"完成失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CancelSelectedOrder()
        {
            var order = GetSelectedOrder();
            if (order == null) return;

            if (order.Status == PurchaseOrderStatus.Completed || order.Status == PurchaseOrderStatus.Cancelled)
            {
                MessageBox.Show("已完成或已取消的订单不能再次取消", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("确定要取消此进货单吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    if (_purchaseService.UpdatePurchaseOrderStatus(order.OrderNumber, PurchaseOrderStatus.Cancelled, _authService.CurrentUser.Id))
                    {
                        MessageBox.Show("取消成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"取消失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // PurchaseManagementForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "PurchaseManagementForm";
            this.ResumeLayout(false);
        }
        #endregion
    }
}