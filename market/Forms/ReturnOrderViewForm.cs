using System;
using System.Drawing;
using System.Windows.Forms;
using market.Models;
using market.Services;

namespace market.Forms
{
    public partial class ReturnOrderViewForm : Form
    {
        private readonly ReturnService _returnService;
        private readonly ReturnOrder _returnOrder;
        
        private Label lblReturnNumber;
        private Label lblOriginalOrderNumber;
        private Label lblCustomer;
        private Label lblReturnDate;
        private Label lblStatus;
        private Label lblReason;
        private Label lblNotes;
        private Label lblTotalAmount;
        private Label lblRefundAmount;
        private Label lblOperator;
        private DataGridView dgvReturnItems;
        private Button btnClose;
        private Button btnPrint;

        public ReturnOrderViewForm(ReturnService returnService, string returnNumber)
        {
            _returnService = returnService;
            _returnOrder = _returnService.GetReturnOrder(returnNumber);
            
            if (_returnOrder == null)
            {
                throw new ArgumentException($"退货订单 {returnNumber} 不存在");
            }
            
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            // 窗体基本设置
            this.Text = $"退货订单详情 - {_returnOrder.ReturnNumber}";
            this.Size = new Size(800, 600);
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

            // 退货单信息面板
            var infoPanel = new GroupBox
            {
                Text = "退货单信息",
                Location = new Point(10, 20),
                Size = new Size(760, 200),
                Font = new Font("Microsoft YaHei", 9f, FontStyle.Bold)
            };

            // 退货单号
            var lblReturnNumberTitle = new Label { Text = "退货单号:", Location = new Point(10, 25), Size = new Size(80, 20), Font = new Font("Microsoft YaHei", 9f, FontStyle.Bold) };
            lblReturnNumber = new Label { Text = _returnOrder.ReturnNumber, Location = new Point(100, 25), Size = new Size(200, 20), ForeColor = Color.Blue };

            // 原销售单号
            var lblOriginalOrderNumberTitle = new Label { Text = "原销售单号:", Location = new Point(10, 55), Size = new Size(80, 20), Font = new Font("Microsoft YaHei", 9f, FontStyle.Bold) };
            lblOriginalOrderNumber = new Label { Text = _returnOrder.OriginalOrderNumber, Location = new Point(100, 55), Size = new Size(200, 20) };

            // 顾客姓名
            var lblCustomerTitle = new Label { Text = "顾客姓名:", Location = new Point(10, 85), Size = new Size(80, 20), Font = new Font("Microsoft YaHei", 9f, FontStyle.Bold) };
            lblCustomer = new Label { Text = _returnOrder.Customer, Location = new Point(100, 85), Size = new Size(200, 20) };

            // 退货日期
            var lblReturnDateTitle = new Label { Text = "退货日期:", Location = new Point(10, 115), Size = new Size(80, 20), Font = new Font("Microsoft YaHei", 9f, FontStyle.Bold) };
            lblReturnDate = new Label { Text = _returnOrder.ReturnDate.ToString("yyyy-MM-dd HH:mm"), Location = new Point(100, 115), Size = new Size(200, 20) };

            // 退货状态
            var lblStatusTitle = new Label { Text = "退货状态:", Location = new Point(350, 25), Size = new Size(80, 20), Font = new Font("Microsoft YaHei", 9f, FontStyle.Bold) };
            lblStatus = new Label { Text = _returnOrder.StatusText, Location = new Point(440, 25), Size = new Size(200, 20), ForeColor = GetStatusColor(_returnOrder.Status) };

            // 退货总金额
            var lblTotalAmountTitle = new Label { Text = "退货总金额:", Location = new Point(350, 55), Size = new Size(80, 20), Font = new Font("Microsoft YaHei", 9f, FontStyle.Bold) };
            lblTotalAmount = new Label { Text = $"¥{_returnOrder.TotalAmount:F2}", Location = new Point(440, 55), Size = new Size(200, 20), ForeColor = Color.Red, Font = new Font("Microsoft YaHei", 10f, FontStyle.Bold) };

            // 退款金额
            var lblRefundAmountTitle = new Label { Text = "退款金额:", Location = new Point(350, 85), Size = new Size(80, 20), Font = new Font("Microsoft YaHei", 9f, FontStyle.Bold) };
            lblRefundAmount = new Label { Text = $"¥{_returnOrder.RefundAmount:F2}", Location = new Point(440, 85), Size = new Size(200, 20), ForeColor = Color.Red, Font = new Font("Microsoft YaHei", 10f, FontStyle.Bold) };

            // 操作人
            var lblOperatorTitle = new Label { Text = "操作人:", Location = new Point(350, 115), Size = new Size(80, 20), Font = new Font("Microsoft YaHei", 9f, FontStyle.Bold) };
            lblOperator = new Label { Text = _returnOrder.OperatorName, Location = new Point(440, 115), Size = new Size(200, 20) };

            // 退货原因
            var lblReasonTitle = new Label { Text = "退货原因:", Location = new Point(10, 145), Size = new Size(80, 20), Font = new Font("Microsoft YaHei", 9f, FontStyle.Bold) };
            lblReason = new Label { Text = _returnOrder.Reason, Location = new Point(100, 145), Size = new Size(600, 20) };

            // 备注
            var lblNotesTitle = new Label { Text = "备注:", Location = new Point(10, 175), Size = new Size(80, 20), Font = new Font("Microsoft YaHei", 9f, FontStyle.Bold) };
            lblNotes = new Label { Text = _returnOrder.Notes, Location = new Point(100, 175), Size = new Size(600, 20) };

            // 添加信息面板控件
            infoPanel.Controls.AddRange(new Control[] {
                lblReturnNumberTitle, lblReturnNumber,
                lblOriginalOrderNumberTitle, lblOriginalOrderNumber,
                lblCustomerTitle, lblCustomer,
                lblReturnDateTitle, lblReturnDate,
                lblStatusTitle, lblStatus,
                lblTotalAmountTitle, lblTotalAmount,
                lblRefundAmountTitle, lblRefundAmount,
                lblOperatorTitle, lblOperator,
                lblReasonTitle, lblReason,
                lblNotesTitle, lblNotes
            });

            // 退货商品明细面板
            var itemsPanel = new GroupBox
            {
                Text = "退货商品明细",
                Location = new Point(10, 230),
                Size = new Size(760, 250),
                Font = new Font("Microsoft YaHei", 9f, FontStyle.Bold)
            };

            // 退货商品数据表格
            dgvReturnItems = new DataGridView
            {
                Location = new Point(10, 25),
                Size = new Size(740, 180),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // 添加列
            dgvReturnItems.Columns.Add("ProductCode", "商品编码");
            dgvReturnItems.Columns.Add("ProductName", "商品名称");
            dgvReturnItems.Columns.Add("Quantity", "退货数量");
            dgvReturnItems.Columns.Add("ReturnPrice", "退货单价");
            dgvReturnItems.Columns.Add("Amount", "退货金额");
            dgvReturnItems.Columns.Add("Reason", "退货原因");

            // 格式化列
            dgvReturnItems.Columns["ReturnPrice"].DefaultCellStyle.Format = "C2";
            dgvReturnItems.Columns["Amount"].DefaultCellStyle.Format = "C2";

            // 添加商品面板控件
            itemsPanel.Controls.Add(dgvReturnItems);

            // 按钮面板
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.White
            };

            // 打印按钮
            btnPrint = new Button
            {
                Text = "打印",
                Location = new Point(500, 10),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White
            };
            btnPrint.Click += BtnPrint_Click;

            // 关闭按钮
            btnClose = new Button
            {
                Text = "关闭",
                Location = new Point(590, 10),
                Size = new Size(80, 30),
                BackColor = Color.Gray,
                ForeColor = Color.White
            };
            btnClose.Click += (s, e) => this.Close();

            buttonPanel.Controls.AddRange(new Control[] { btnPrint, btnClose });

            // 添加所有面板到主面板
            mainPanel.Controls.AddRange(new Control[] { infoPanel, itemsPanel });
            this.Controls.AddRange(new Control[] { mainPanel, buttonPanel });
        }

        private void LoadData()
        {
            // 加载退货商品明细
            dgvReturnItems.Rows.Clear();
            
            foreach (var item in _returnOrder.Items)
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

        private Color GetStatusColor(ReturnOrderStatus status)
        {
            switch (status)
            {
                case ReturnOrderStatus.Pending:
                    return Color.Orange;
                case ReturnOrderStatus.Approved:
                    return Color.Blue;
                case ReturnOrderStatus.Completed:
                    return Color.Green;
                case ReturnOrderStatus.Cancelled:
                    return Color.Red;
                default:
                    return Color.Black;
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                // 实现打印功能
                MessageBox.Show("打印功能待实现", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打印失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}