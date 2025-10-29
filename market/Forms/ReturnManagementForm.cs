using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using market.Models;
using market.Services;

namespace market.Forms
{
    public partial class ReturnManagementForm : Form
    {
        private readonly ReturnService _returnService;
        private readonly AuthService _authService;
        private readonly ProductService _productService;
        
        private DataGridView dgvReturnOrders;
        private TextBox txtSearchReturnNumber;
        private TextBox txtSearchOriginalOrderNumber;
        private ComboBox cmbStatusFilter;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private Button btnSearch;
        private Button btnNewReturn;
        private Button btnViewReturn;
        private Button btnEditReturn;
        private Button btnDeleteReturn;
        private Label lblTotalCount;

        public ReturnManagementForm(ReturnService returnService, AuthService authService, ProductService productService)
        {
            _returnService = returnService;
            _authService = authService;
            _productService = productService;
            
            InitializeComponent();
            LoadReturnOrders();
        }

        private void InitializeComponent()
        {
            // 窗体基本设置
            this.Text = "退货管理";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Padding = new Padding(0, 20, 0, 0);
            
            // 创建搜索面板
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.White
            };

            // 退货单号搜索
            var lblReturnNumber = new Label
            {
                Text = "退货单号:",
                Location = new Point(10, 15),
                Size = new Size(80, 20)
            };
            txtSearchReturnNumber = new TextBox
            {
                Location = new Point(90, 12),
                Size = new Size(120, 25)
            };

            // 原销售单号搜索
            var lblOriginalOrderNumber = new Label
            {
                Text = "原销售单号:",
                Location = new Point(220, 15),
                Size = new Size(80, 20)
            };
            txtSearchOriginalOrderNumber = new TextBox
            {
                Location = new Point(300, 12),
                Size = new Size(120, 25)
            };

            // 状态筛选
            var lblStatus = new Label
            {
                Text = "退货状态:",
                Location = new Point(430, 15),
                Size = new Size(80, 20)
            };
            cmbStatusFilter = new ComboBox
            {
                Location = new Point(510, 12),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatusFilter.Items.AddRange(new object[] { "全部", "待处理", "已审核", "已完成", "已取消" });
            cmbStatusFilter.SelectedIndex = 0;

            // 日期筛选
            var lblStartDate = new Label
            {
                Text = "开始日期:",
                Location = new Point(10, 50),
                Size = new Size(80, 20)
            };
            dtpStartDate = new DateTimePicker
            {
                Location = new Point(90, 47),
                Size = new Size(120, 25),
                Value = DateTime.Now.AddDays(-30)
            };

            var lblEndDate = new Label
            {
                Text = "结束日期:",
                Location = new Point(220, 50),
                Size = new Size(80, 20)
            };
            dtpEndDate = new DateTimePicker
            {
                Location = new Point(300, 47),
                Size = new Size(120, 25),
                Value = DateTime.Now
            };

            // 搜索按钮
            btnSearch = new Button
            {
                Text = "搜索",
                Location = new Point(430, 47),
                Size = new Size(80, 25),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White
            };
            btnSearch.Click += BtnSearch_Click;

            // 按钮面板
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.White
            };

            // 新建退货按钮
            btnNewReturn = new Button
            {
                Text = "新建退货",
                Location = new Point(10, 10),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White
            };
            btnNewReturn.Click += BtnNewReturn_Click;

            // 查看退货按钮
            btnViewReturn = new Button
            {
                Text = "查看",
                Location = new Point(100, 10),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White
            };
            btnViewReturn.Click += BtnViewReturn_Click;

            // 编辑退货按钮
            btnEditReturn = new Button
            {
                Text = "编辑",
                Location = new Point(190, 10),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White
            };
            btnEditReturn.Click += BtnEditReturn_Click;

            // 删除退货按钮
            btnDeleteReturn = new Button
            {
                Text = "删除",
                Location = new Point(280, 10),
                Size = new Size(80, 30),
                BackColor = Color.Red,
                ForeColor = Color.White
            };
            btnDeleteReturn.Click += BtnDeleteReturn_Click;

            // 总记录数标签
            lblTotalCount = new Label
            {
                Text = "总记录数: 0",
                Location = new Point(370, 15),
                Size = new Size(200, 20),
                ForeColor = Color.Gray
            };

            // 数据表格
            dgvReturnOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // 添加列
            dgvReturnOrders.Columns.Add("ReturnNumber", "退货单号");
            dgvReturnOrders.Columns.Add("OriginalOrderNumber", "原销售单号");
            dgvReturnOrders.Columns.Add("ReturnDate", "退货日期");
            dgvReturnOrders.Columns.Add("Customer", "顾客姓名");
            dgvReturnOrders.Columns.Add("TotalAmount", "退货金额");
            dgvReturnOrders.Columns.Add("Status", "状态");
            dgvReturnOrders.Columns.Add("Reason", "退货原因");
            dgvReturnOrders.Columns.Add("Operator", "操作人");

            // 格式化列
            dgvReturnOrders.Columns["ReturnDate"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
            dgvReturnOrders.Columns["TotalAmount"].DefaultCellStyle.Format = "C2";

            // 添加控件到面板
            searchPanel.Controls.AddRange(new Control[] {
                lblReturnNumber, txtSearchReturnNumber,
                lblOriginalOrderNumber, txtSearchOriginalOrderNumber,
                lblStatus, cmbStatusFilter,
                lblStartDate, dtpStartDate,
                lblEndDate, dtpEndDate,
                btnSearch
            });

            buttonPanel.Controls.AddRange(new Control[] {
                btnNewReturn, btnViewReturn, btnEditReturn, btnDeleteReturn, lblTotalCount
            });

            // 添加面板到窗体
            this.Controls.AddRange(new Control[] { dgvReturnOrders, buttonPanel, searchPanel });
        }

        private void LoadReturnOrders()
        {
            try
            {
                var query = new ReturnOrderQuery
                {
                    ReturnNumber = txtSearchReturnNumber.Text,
                    OriginalOrderNumber = txtSearchOriginalOrderNumber.Text,
                    StartDate = dtpStartDate.Value.Date,
                    EndDate = dtpEndDate.Value.Date,
                    PageSize = 100,
                    PageIndex = 1
                };

                // 处理状态筛选
                if (cmbStatusFilter.SelectedIndex > 0)
                {
                    query.Status = (ReturnOrderStatus)(cmbStatusFilter.SelectedIndex - 1);
                }

                var result = _returnService.GetReturnOrdersPaged(query);
                
                dgvReturnOrders.Rows.Clear();
                
                foreach (var order in result.Orders)
                {
                    dgvReturnOrders.Rows.Add(
                        order.ReturnNumber,
                        order.OriginalOrderNumber,
                        order.ReturnDate,
                        order.Customer,
                        order.TotalAmount,
                        order.StatusText,
                        order.Reason,
                        order.OperatorName
                    );
                }
                
                lblTotalCount.Text = $"总记录数: {result.TotalCount}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载退货订单失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            LoadReturnOrders();
        }

        private void BtnNewReturn_Click(object sender, EventArgs e)
        {
            try
            {
                var returnEditForm = new ReturnOrderEditForm(_returnService, _authService, _productService);
                if (returnEditForm.ShowDialog() == DialogResult.OK)
                {
                    LoadReturnOrders();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开退货编辑界面失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnViewReturn_Click(object sender, EventArgs e)
        {
            if (dgvReturnOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择要查看的退货订单", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var returnNumber = dgvReturnOrders.SelectedRows[0].Cells["ReturnNumber"].Value.ToString();
                var returnViewForm = new ReturnOrderViewForm(_returnService, returnNumber);
                returnViewForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查看退货订单失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEditReturn_Click(object sender, EventArgs e)
        {
            if (dgvReturnOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择要编辑的退货订单", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var returnNumber = dgvReturnOrders.SelectedRows[0].Cells["ReturnNumber"].Value.ToString();
                var returnEditForm = new ReturnOrderEditForm(_returnService, _authService, _productService, returnNumber);
                if (returnEditForm.ShowDialog() == DialogResult.OK)
                {
                    LoadReturnOrders();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"编辑退货订单失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeleteReturn_Click(object sender, EventArgs e)
        {
            if (dgvReturnOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择要删除的退货订单", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var returnNumber = dgvReturnOrders.SelectedRows[0].Cells["ReturnNumber"].Value.ToString();
                var status = dgvReturnOrders.SelectedRows[0].Cells["Status"].Value.ToString();
                
                if (status == "已完成" || status == "已取消")
                {
                    MessageBox.Show("已完成或已取消的退货订单不能删除", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show($"确定要删除退货订单 {returnNumber} 吗？此操作不可恢复。", "确认删除", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                
                if (result == DialogResult.Yes)
                {
                    // 删除退货订单的逻辑需要实现
                    MessageBox.Show("删除功能待实现", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadReturnOrders();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除退货订单失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 获取当前选中的退货订单编号
        /// </summary>
        /// <returns>退货订单编号</returns>
        public string GetSelectedReturnNumber()
        {
            if (dgvReturnOrders.SelectedRows.Count > 0)
            {
                return dgvReturnOrders.SelectedRows[0].Cells["ReturnNumber"].Value.ToString();
            }
            return null;
        }
    }
}