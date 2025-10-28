using System;
using System.Drawing;
using System.Windows.Forms;
using market.Models;
using market.Services;

namespace market.Forms
{
    public partial class PurchaseOrderViewForm : Form
    {
        private readonly PurchaseOrder _order;
        public PurchaseOrderViewForm(DatabaseService databaseService, PurchaseOrder order)
        {
            InitializeComponent();
            
            _order = order;
            
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = $"进货单详情 - {_order.OrderNumber}";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 创建主布局
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // 创建基本信息面板
            var basicInfoPanel = CreateBasicInfoPanel();
            mainPanel.Controls.Add(basicInfoPanel);

            // 创建商品明细面板 - 设置Dock为Fill以填充剩余空间
            var itemsPanel = CreateItemsPanel();
            itemsPanel.Dock = DockStyle.Fill;
            mainPanel.Controls.Add(itemsPanel);

            // 创建按钮面板 - 设置Dock为Bottom以固定在底部
            var buttonPanel = CreateButtonPanel();
            buttonPanel.Dock = DockStyle.Bottom;
            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);

            // 显示数据
            DisplayOrderData();
        }

        private Panel CreateBasicInfoPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 160,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            // 创建基本信息标签
            var infoLabels = new Label[]
            {
                new Label { Text = "进货单号:", Location = new Point(10, 15), Width = 80 },
                new Label { Text = "进货日期:", Location = new Point(10, 45), Width = 80 },
                new Label { Text = "供应商:", Location = new Point(10, 75), Width = 80 },
                new Label { Text = "操作人:", Location = new Point(10, 105), Width = 80 },
                new Label { Text = "状态:", Location = new Point(300, 15), Width = 80 },
                new Label { Text = "创建时间:", Location = new Point(300, 45), Width = 80 },
                new Label { Text = "完成时间:", Location = new Point(300, 75), Width = 80 },
                new Label { Text = "备注:", Location = new Point(300, 105), Width = 80 }
            };

            // 创建值标签
            var valueLabels = new Label[]
            {
                new Label { Location = new Point(90, 15), Width = 200, Text = _order.OrderNumber },
                new Label { Location = new Point(90, 45), Width = 200, Text = _order.OrderDate.ToString("yyyy-MM-dd") },
                new Label { Location = new Point(90, 75), Width = 200, Text = _order.SupplierName },
                new Label { Location = new Point(90, 105), Width = 200, Text = _order.OperatorName },
                new Label { Location = new Point(380, 15), Width = 200, Text = _order.StatusText, 
                           ForeColor = GetStatusColor(_order.Status) },
                new Label { Location = new Point(380, 45), Width = 200, Text = _order.CreatedAt.ToString("yyyy-MM-dd HH:mm") },
                new Label { Location = new Point(380, 75), Width = 200, Text = _order.CompletedAt?.ToString("yyyy-MM-dd HH:mm") ?? "-" },
                new Label { Location = new Point(380, 105), Width = 300, Text = _order.Notes ?? "-" }
            };

            // 金额信息
            var amountLabels = new Label[]
            {
                new Label { Text = "合计金额:", Location = new Point(10, 135), Width = 80, Font = new Font("微软雅黑", 9, FontStyle.Bold) },
                new Label { Text = "税额:", Location = new Point(150, 135), Width = 80 },
                new Label { Text = "最终金额:", Location = new Point(300, 135), Width = 80, Font = new Font("微软雅黑", 9, FontStyle.Bold) }
            };

            var amountValues = new Label[]
            {
                new Label { Location = new Point(90, 135), Width = 100, Text = $"￥{_order.TotalAmount:F2}", Font = new Font("微软雅黑", 9, FontStyle.Bold) },
                new Label { Location = new Point(230, 135), Width = 100, Text = $"￥{_order.TaxAmount:F2}" },
                new Label { Location = new Point(380, 135), Width = 100, Text = $"￥{_order.FinalAmount:F2}", Font = new Font("微软雅黑", 9, FontStyle.Bold), ForeColor = Color.Blue }
            };

            // 添加到面板
            panel.Controls.AddRange(infoLabels);
            panel.Controls.AddRange(valueLabels);
            panel.Controls.AddRange(amountLabels);
            panel.Controls.AddRange(amountValues);

            return panel;
        }

        private Panel CreateItemsPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            // 商品明细标题
            var lblItems = new Label { 
                Text = $"商品明细 ({_order.Items.Count} 项)", 
                Location = new Point(10, 10), 
                Font = new Font("微软雅黑", 10, FontStyle.Bold) 
            };

            // 商品明细数据网格 - 只调整垂直位置避免被遮挡
            var dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            
            // 创建一个包装面板来放置标题和DataGridView
            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 180, 0, 40) // 只调整顶部padding（margin top）
            };
            
            panel.Controls.Add(lblItems);
            contentPanel.Controls.Add(dataGridView);
            panel.Controls.Add(contentPanel);

            // 添加列
            dataGridView.Columns.Add("ProductCode", "商品编码");
                dataGridView.Columns.Add("ProductName", "商品名称");
                dataGridView.Columns.Add("Quantity", "数量");
                dataGridView.Columns.Add("PurchasePrice", "进货单价");
                dataGridView.Columns.Add("Amount", "金额");
                dataGridView.Columns.Add("BatchNumber", "批次号");
                dataGridView.Columns.Add("ExpiryDate", "有效期");

            // 格式化列
            dataGridView.Columns["PurchasePrice"].DefaultCellStyle.Format = "C2";
            dataGridView.Columns["Amount"].DefaultCellStyle.Format = "C2";
            dataGridView.Columns["ExpiryDate"].DefaultCellStyle.Format = "yyyy-MM-dd";

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

            var btnPrint = new Button { Text = "打印", Size = new Size(80, 30), Location = new Point(300, 10) };
            var btnClose = new Button { Text = "关闭", Size = new Size(80, 30), Location = new Point(400, 10) };

            panel.Controls.AddRange(new Control[] { btnPrint, btnClose });

            // 事件处理
            btnPrint.Click += (s, e) => PrintOrder();
            btnClose.Click += (s, e) => this.Close();

            return panel;
        }

        private void DisplayOrderData()
        {
            // 显示商品明细
            var mainPanel = this.Controls[0] as Panel;
            var itemsPanel = mainPanel.Controls[1] as Panel;
            // 获取 contentPanel，然后从中获取 DataGridView（注意：contentPanel 是索引1）
            var contentPanel = itemsPanel.Controls[1] as Panel;
            var dataGridView = contentPanel.Controls[0] as DataGridView;

            // 确保DataGridView和列都正确初始化
            if (dataGridView == null)
            {
                MessageBox.Show("DataGridView未找到", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (dataGridView.Columns.Count != 7)
            {
                MessageBox.Show($"列数量不匹配: 期望7列, 实际{dataGridView.Columns.Count}列", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // 清除并重新添加行
            dataGridView.Rows.Clear();
            Console.WriteLine($"当前订单物品数量: {_order.Items}");
            // 检查_order.Items集合是否有数据
            if (_order.Items == null || _order.Items.Count == 0)
            {
                    // 测试添加一行示例数据
                dataGridView.Rows.Add("TEST001", "测试商品", 10, 50.5m, 505.0m, "BATCH001", "2024-12-31");
                MessageBox.Show("当前没有物品数据，已添加测试数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                foreach (var item in _order.Items)
                {
                    dataGridView.Rows.Add(
                        item.ProductCode,
                        item.ProductName,
                        item.Quantity,
                        item.PurchasePrice,
                        item.Amount,
                        item.BatchNumber ?? "-",
                        item.ExpiryDate.HasValue ? item.ExpiryDate.Value.ToString("yyyy-MM-dd") : "-"
                    );
                }
            }
        }

        private Color GetStatusColor(PurchaseOrderStatus status)
        {
            switch (status)
            {
                case PurchaseOrderStatus.Pending:
                    return Color.Orange;
                case PurchaseOrderStatus.Approved:
                    return Color.Blue;
                case PurchaseOrderStatus.Delivered:
                    return Color.Green;
                case PurchaseOrderStatus.Completed:
                    return Color.DarkGreen;
                case PurchaseOrderStatus.Cancelled:
                    return Color.Red;
                default:
                    return Color.Black;
            }
        }

        private void PrintOrder()
        {
            // 简单的打印实现（实际项目中应该使用报表工具）
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                // 这里可以实现实际的打印逻辑
                MessageBox.Show("打印功能暂未实现，请联系系统管理员", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // PurchaseOrderViewForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "PurchaseOrderViewForm";
            this.ResumeLayout(false);
        }
        #endregion
    }
}