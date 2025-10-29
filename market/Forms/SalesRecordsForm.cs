using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OfficeOpenXml;
using System.Diagnostics;
using System.IO;
using market.Models;
using market.Services;

namespace market.Forms
{
    public partial class SalesRecordsForm : Form
    {
        private readonly SaleService _saleService;
        private readonly AuthService _authService;
        
        // UI控件
        private TextBox _txtOrderNumber;
        private TextBox _txtCustomer;
        private ComboBox _cmbStatus;
        private ComboBox _cmbPaymentMethod;
        private DateTimePicker _dtpStartDate;
        private DateTimePicker _dtpEndDate;
        private TextBox _txtProductCode;
        private Button _btnSearch;
        private Button _btnReset;
        private DataGridView _dgvSales;
        private Button _btnViewDetails;
        private Button _btnExport;
        private Label _lblTotalOrders;
        private Label _lblTotalAmount;
        private Label _lblCurrentPage;
        private Button _btnPrevPage;
        private Button _btnNextPage;
        
        // 分页信息
        private int _currentPage = 1;
        private int _pageSize = 20;
        private int _totalCount = 0;
        private List<SaleOrder> _currentOrders = new List<SaleOrder>();

        public SalesRecordsForm(SaleService saleService, AuthService authService)
        {
            _saleService = saleService;
            _authService = authService;
            
            InitializeComponent();
            InitializeForm();
            LoadSalesData();
        }

        private void InitializeComponent()
        {
            this.Text = "销售记录查询";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
        }

        private void InitializeForm()
        {
            // 使用 TableLayoutPanel 来精确控制布局
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };
            
            // 设置行高比例
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130)); // 查询面板固定高度
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // 数据面板自适应
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // 底部面板固定高度
            
            this.Controls.Add(tableLayout);

            // 查询条件区域
            var queryPanel = CreateQueryPanel();
            queryPanel.Dock = DockStyle.Fill;
            tableLayout.Controls.Add(queryPanel, 0, 0);

            // 数据表格区域
            var dataPanel = CreateDataPanel();
            dataPanel.Dock = DockStyle.Fill;
            tableLayout.Controls.Add(dataPanel, 0, 1);

            // 分页和统计区域
            var footerPanel = CreateFooterPanel();
            footerPanel.Dock = DockStyle.Fill;
            tableLayout.Controls.Add(footerPanel, 0, 2);
        }

        private Panel CreateQueryPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(0,15,0,0)
            };

            // 第一行
            var row1 = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Top,
                Height = 30,
                Margin = new Padding(0, 10, 0, 5)
            };

            row1.Controls.Add(new Label { Text = "销售单号:", AutoSize = true, Margin = new Padding(0, 5, 5, 0) });
            _txtOrderNumber = new TextBox { Width = 120, Margin = new Padding(0, 5, 15, 0) };
            row1.Controls.Add(_txtOrderNumber);

            row1.Controls.Add(new Label { Text = "客户姓名:", AutoSize = true, Margin = new Padding(0, 5, 5, 0) });
            _txtCustomer = new TextBox { Width = 100, Margin = new Padding(0, 5, 15, 0) };
            row1.Controls.Add(_txtCustomer);

            row1.Controls.Add(new Label { Text = "订单状态:", AutoSize = true, Margin = new Padding(0, 5, 5, 0) });
            _cmbStatus = new ComboBox { Width = 100, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 5, 15, 0) };
            _cmbStatus.Items.AddRange(new object[] { "全部", "待支付", "已支付", "已完成", "已取消" });
            _cmbStatus.SelectedIndex = 0;
            row1.Controls.Add(_cmbStatus);

            // 第二行
            var row2 = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Top,
                Height = 30,
                Margin = new Padding(0, 0, 0, 5)
            };

            row2.Controls.Add(new Label { Text = "支付方式:", AutoSize = true, Margin = new Padding(0, 5, 5, 0) });
            _cmbPaymentMethod = new ComboBox { Width = 100, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 5, 15, 0) };
            _cmbPaymentMethod.Items.AddRange(new object[] { "全部", "现金", "微信支付", "支付宝", "银行卡" });
            _cmbPaymentMethod.SelectedIndex = 0;
            row2.Controls.Add(_cmbPaymentMethod);

            row2.Controls.Add(new Label { Text = "开始日期:", AutoSize = true, Margin = new Padding(0, 5, 5, 0) });
            _dtpStartDate = new DateTimePicker { Width = 120, Margin = new Padding(0, 5, 15, 0), Value = DateTime.Now.AddDays(-30) };
            row2.Controls.Add(_dtpStartDate);

            row2.Controls.Add(new Label { Text = "结束日期:", AutoSize = true, Margin = new Padding(0, 5, 5, 0) });
            _dtpEndDate = new DateTimePicker { Width = 120, Margin = new Padding(0, 5, 15, 0), Value = DateTime.Now };
            row2.Controls.Add(_dtpEndDate);

            // 第三行
            var row3 = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Top,
                Height = 30
            };

            row3.Controls.Add(new Label { Text = "商品编码:", AutoSize = true, Margin = new Padding(0, 5, 5, 0) });
            _txtProductCode = new TextBox { Width = 100, Margin = new Padding(0, 5, 15, 0) };
            row3.Controls.Add(_txtProductCode);

            _btnSearch = new Button { Text = "查询", Width = 80, Margin = new Padding(0, 5, 15, 0) };
            _btnSearch.Click += (s, e) => LoadSalesData();
            row3.Controls.Add(_btnSearch);

            _btnReset = new Button { Text = "重置", Width = 80, Margin = new Padding(0, 5, 15, 0) };
            _btnReset.Click += (s, e) => ResetQuery();
            row3.Controls.Add(_btnReset);

            panel.Controls.Add(row1);
            panel.Controls.Add(row2);
            panel.Controls.Add(row3);

            return panel;
        }

        private Panel CreateDataPanel()
        {
            var panel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle
            };

            // 创建数据表格
            _dgvSales = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // 添加列
            _dgvSales.Columns.Add("OrderNumber", "销售单号");
            _dgvSales.Columns.Add("OrderDate", "销售日期");
            _dgvSales.Columns.Add("Customer", "客户");
            _dgvSales.Columns.Add("TotalAmount", "总金额");
            _dgvSales.Columns.Add("FinalAmount", "实收金额");
            _dgvSales.Columns.Add("StatusText", "状态");
            _dgvSales.Columns.Add("PaymentMethodText", "支付方式");
            _dgvSales.Columns.Add("OperatorName", "操作员");

            // 设置金额列的格式
            _dgvSales.Columns["TotalAmount"].DefaultCellStyle.Format = "C2";
            _dgvSales.Columns["FinalAmount"].DefaultCellStyle.Format = "C2";

            panel.Controls.Add(_dgvSales);

            return panel;
        }

        private Panel CreateFooterPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BorderStyle = BorderStyle.FixedSingle
            };

            // 左侧统计信息
            var statsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Left,
                Width = 400,
                Padding = new Padding(10)
            };

            _lblTotalOrders = new Label { AutoSize = true, Text = "总订单数: 0" };
            statsPanel.Controls.Add(_lblTotalOrders);

            statsPanel.Controls.Add(new Label { Text = " | " });

            _lblTotalAmount = new Label { AutoSize = true, Text = "总金额: ￥0.00" };
            statsPanel.Controls.Add(_lblTotalAmount);

            // 右侧操作按钮
            var buttonsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Right,
                Width = 400,
                Padding = new Padding(10)
            };

            _btnExport = new Button { Text = "导出", Width = 80 };
            _btnExport.Click += (s, e) => ExportData();
            buttonsPanel.Controls.Add(_btnExport);

            _btnViewDetails = new Button { Text = "查看详情", Width = 80, Margin = new Padding(0, 0, 10, 0) };
            _btnViewDetails.Click += (s, e) => ViewOrderDetails();
            buttonsPanel.Controls.Add(_btnViewDetails);

            // 分页控件
            var pagingPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Fill,
                Height = 30,
                Padding = new Padding(10)
            };

            _btnPrevPage = new Button { Text = "上一页", Width = 80 };
            _btnPrevPage.Click += (s, e) => GoToPage(_currentPage - 1);
            pagingPanel.Controls.Add(_btnPrevPage);

            _lblCurrentPage = new Label { AutoSize = true, Text = "第 1 页", Margin = new Padding(20, 5, 20, 0) };
            pagingPanel.Controls.Add(_lblCurrentPage);

            _btnNextPage = new Button { Text = "下一页", Width = 80 };
            _btnNextPage.Click += (s, e) => GoToPage(_currentPage + 1);
            pagingPanel.Controls.Add(_btnNextPage);

            panel.Controls.Add(statsPanel);
            panel.Controls.Add(pagingPanel);
            panel.Controls.Add(buttonsPanel);

            return panel;
        }

        private void LoadSalesData()
        {
            try
            {
                var query = new SaleOrderQuery
                {
                    OrderNumber = _txtOrderNumber.Text.Trim(),
                    Customer = _txtCustomer.Text.Trim(),
                    Status = GetSelectedStatus(),
                    PaymentMethod = GetSelectedPaymentMethod(),
                    StartDate = _dtpStartDate.Value,
                    EndDate = _dtpEndDate.Value,
                    ProductCode = _txtProductCode.Text.Trim(),
                    PageIndex = _currentPage,
                    PageSize = _pageSize
                };

                var result = _saleService.GetSaleOrdersPaged(query);
                _currentOrders = result.Orders;
                _totalCount = result.TotalCount;

                DisplaySalesData();
                UpdateStatistics();
                UpdatePagingButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载销售数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplaySalesData()
        {
            _dgvSales.Rows.Clear();

            foreach (var order in _currentOrders)
            {
                _dgvSales.Rows.Add(
                    order.OrderNumber,
                    order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    order.Customer,
                    order.TotalAmount,
                    order.FinalAmount,
                    order.StatusText,
                    order.PaymentMethodText,
                    order.OperatorName
                );
            }
        }

        private void UpdateStatistics()
        {
            var totalOrders = _totalCount;
            var totalAmount = _currentOrders.Sum(o => o.FinalAmount);

            _lblTotalOrders.Text = $"总订单数: {totalOrders}";
            _lblTotalAmount.Text = $"总金额: ￥{totalAmount:F2}";
        }

        private void UpdatePagingButtons()
        {
            _lblCurrentPage.Text = $"第 {_currentPage} 页 / 共 {Math.Ceiling((double)_totalCount / _pageSize)} 页";
            
            _btnPrevPage.Enabled = _currentPage > 1;
            _btnNextPage.Enabled = _currentPage < Math.Ceiling((double)_totalCount / _pageSize);
        }

        private void GoToPage(int page)
        {
            if (page < 1 || page > Math.Ceiling((double)_totalCount / _pageSize))
                return;

            _currentPage = page;
            LoadSalesData();
        }

        private void ResetQuery()
        {
            _txtOrderNumber.Text = string.Empty;
            _txtCustomer.Text = string.Empty;
            _cmbStatus.SelectedIndex = 0;
            _cmbPaymentMethod.SelectedIndex = 0;
            _dtpStartDate.Value = DateTime.Now.AddDays(-30);
            _dtpEndDate.Value = DateTime.Now;
            _txtProductCode.Text = string.Empty;
            _currentPage = 1;
            
            LoadSalesData();
        }

        private void ViewOrderDetails()
        {
            if (_dgvSales.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择要查看的销售订单", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var orderNumber = _dgvSales.SelectedRows[0].Cells["OrderNumber"].Value.ToString();
                var order = _saleService.GetSaleOrderByNumber(orderNumber);
                
                if (order != null)
                {
                    ShowOrderDetails(order);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查看订单详情失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowOrderDetails(SaleOrder order)
        {
            var detailsForm = new Form
            {
                Text = $"销售订单详情 - {order.OrderNumber}",
                Size = new Size(900, 700),
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = true,
                MaximizeBox = true
            };

            // 使用 TableLayoutPanel 作为主容器
            var mainTableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(15),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            
            // 设置行高比例
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180)); // 订单基本信息固定高度
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // 商品明细自适应
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // 底部操作栏固定高度
            
            detailsForm.Controls.Add(mainTableLayout);

            // 1. 订单基本信息区域
            var infoPanel = new GroupBox
            {
                Text = "订单基本信息",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var infoLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(5)
            };

            // 设置列宽比例
            infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            
            // 设置行高
            for (int i = 0; i < 5; i++)
            {
                infoLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
            }

            // 添加订单信息
            infoLayout.Controls.Add(new Label { Text = "销售单号:", AutoSize = true, Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            infoLayout.Controls.Add(new Label { Text = order.OrderNumber, AutoSize = true, Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft }, 1, 0);

            infoLayout.Controls.Add(new Label { Text = "销售日期:", AutoSize = true, Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            infoLayout.Controls.Add(new Label { Text = order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"), AutoSize = true, Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft }, 1, 1);

            infoLayout.Controls.Add(new Label { Text = "客户信息:", AutoSize = true, Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
            infoLayout.Controls.Add(new Label { Text = order.Customer, AutoSize = true, Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft }, 1, 2);

            infoLayout.Controls.Add(new Label { Text = "操作员:", AutoSize = true, Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
            infoLayout.Controls.Add(new Label { Text = order.OperatorName, AutoSize = true, Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft }, 1, 3);

            infoLayout.Controls.Add(new Label { Text = "备注信息:", AutoSize = true, Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft }, 0, 4);
            infoLayout.Controls.Add(new Label { Text = (string.IsNullOrEmpty(order.Notes) ? "无" : order.Notes), AutoSize = true, Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft }, 1, 4);

            infoPanel.Controls.Add(infoLayout);
            mainTableLayout.Controls.Add(infoPanel, 0, 0);

            // 2. 商品明细区域
            var itemsPanel = new GroupBox
            {
                Text = "商品明细",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var itemsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            
            itemsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // 统计信息
            itemsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // 商品列表

            // 统计信息
            var statsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5)
            };
            
            statsPanel.Controls.Add(new Label { Text = $"商品总数: {order.Items.Count}", AutoSize = true, Margin = new Padding(0, 5, 20, 0) });
            statsPanel.Controls.Add(new Label { Text = $"总金额: ￥{order.TotalAmount:F2}", AutoSize = true, Margin = new Padding(0, 5, 20, 0) });
            statsPanel.Controls.Add(new Label { Text = $"优惠金额: ￥{order.DiscountAmount:F2}", AutoSize = true, Margin = new Padding(0, 5, 20, 0) });
            statsPanel.Controls.Add(new Label { Text = $"实收金额: ￥{order.FinalAmount:F2}", AutoSize = true, Margin = new Padding(0, 5, 20, 0) });
            statsPanel.Controls.Add(new Label { Text = $"支付方式: {order.PaymentMethodText}", AutoSize = true, Margin = new Padding(0, 5, 20, 0) });
            statsPanel.Controls.Add(new Label { Text = $"订单状态: {order.StatusText}", AutoSize = true, Margin = new Padding(0, 5, 20, 0) });

            // 商品数据表格
            var dgvItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BorderStyle = BorderStyle.Fixed3D
            };

            // 添加列
            dgvItems.Columns.Add("ProductCode", "商品编码");
            dgvItems.Columns.Add("ProductName", "商品名称");
            dgvItems.Columns.Add("Quantity", "数量");
            dgvItems.Columns.Add("OriginalPrice", "原价");
            dgvItems.Columns.Add("SalePrice", "销售价");
            dgvItems.Columns.Add("DiscountRate", "折扣率");
            dgvItems.Columns.Add("Amount", "金额");

            // 设置列格式
            dgvItems.Columns["OriginalPrice"].DefaultCellStyle.Format = "C2";
            dgvItems.Columns["SalePrice"].DefaultCellStyle.Format = "C2";
            dgvItems.Columns["DiscountRate"].DefaultCellStyle.Format = "P2";
            dgvItems.Columns["Amount"].DefaultCellStyle.Format = "C2";

            // 填充数据
            foreach (var item in order.Items)
            {
                dgvItems.Rows.Add(
                    item.ProductCode,
                    item.ProductName,
                    item.Quantity,
                    item.OriginalPrice,
                    item.SalePrice,
                    item.DiscountRate,
                    item.Amount
                );
            }

            itemsLayout.Controls.Add(statsPanel);
            itemsLayout.Controls.Add(dgvItems);
            itemsPanel.Controls.Add(itemsLayout);
            mainTableLayout.Controls.Add(itemsPanel, 0, 1);

            // 3. 底部操作栏
            var actionPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10)
            };

            var btnClose = new Button { Text = "关闭", Width = 80, Margin = new Padding(10, 0, 0, 0) };
            btnClose.Click += (s, e) => detailsForm.Close();
            actionPanel.Controls.Add(btnClose);

            mainTableLayout.Controls.Add(actionPanel, 0, 2);

            detailsForm.ShowDialog();
        }

        private void ExportData()
        {
            try
            {
                if (_dgvSales.Rows.Count == 0)
                {
                    MessageBox.Show("没有数据可导出", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 显示保存文件对话框
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Excel文件 (*.xlsx)|*.xlsx";
                    saveFileDialog.FileName = $"销售记录_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.xlsx";
                    saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // 设置EPPlus许可
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                        // 创建Excel包
                        using (ExcelPackage package = new ExcelPackage())
                        {
                            // 添加工作表
                            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("销售记录");

                            // 设置表头
                            for (int i = 0; i < _dgvSales.Columns.Count; i++)
                            {
                                worksheet.Cells[1, i + 1].Value = _dgvSales.Columns[i].HeaderText;
                                // 设置表头样式
                                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                                worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                                worksheet.Cells[1, i + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            }

                            // 填充数据
                            for (int row = 0; row < _dgvSales.Rows.Count; row++)
                            {
                                for (int col = 0; col < _dgvSales.Columns.Count; col++)
                                {
                                    // 跳过空行
                                    if (_dgvSales.Rows[row].Cells[col].Value == null || _dgvSales.Rows[row].Cells[col].Value.ToString() == string.Empty)
                                    {
                                        worksheet.Cells[row + 2, col + 1].Value = "";
                                    }
                                    else
                                    {
                                        // 处理金额列，确保格式正确
                                        if (_dgvSales.Columns[col].Name == "TotalAmount" || _dgvSales.Columns[col].Name == "FinalAmount")
                                        {
                                            if (double.TryParse(_dgvSales.Rows[row].Cells[col].Value.ToString(), out double amount))
                                            {
                                                worksheet.Cells[row + 2, col + 1].Value = amount;
                                                worksheet.Cells[row + 2, col + 1].Style.Numberformat.Format = "¥#,##0.00";
                                            }
                                            else
                                            {
                                                worksheet.Cells[row + 2, col + 1].Value = _dgvSales.Rows[row].Cells[col].Value;
                                            }
                                        }
                                        else
                                        {
                                            worksheet.Cells[row + 2, col + 1].Value = _dgvSales.Rows[row].Cells[col].Value;
                                        }
                                    }
                                }
                            }

                            // 自动调整列宽
                            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                            // 保存文件
                            FileInfo excelFile = new FileInfo(saveFileDialog.FileName);
                            package.SaveAs(excelFile);

                            MessageBox.Show($"数据已成功导出到：\n{excelFile.FullName}", "导出成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // 询问是否打开文件
                            if (MessageBox.Show("是否要打开导出的文件？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                try
                                {
                                    // 使用ProcessStartInfo并设置UseShellExecute=true，以便Windows使用默认关联程序打开文件
                                    ProcessStartInfo startInfo = new ProcessStartInfo(excelFile.FullName)
                                    {
                                        UseShellExecute = true
                                    };
                                    System.Diagnostics.Process.Start(startInfo);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"无法打开文件：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private SaleOrderStatus? GetSelectedStatus()
        {
            return _cmbStatus.SelectedIndex switch
            {
                1 => SaleOrderStatus.Pending,
                2 => SaleOrderStatus.Paid,
                3 => SaleOrderStatus.Completed,
                4 => SaleOrderStatus.Cancelled,
                _ => null
            };
        }

        private PaymentMethod? GetSelectedPaymentMethod()
        {
            return _cmbPaymentMethod.SelectedIndex switch
            {
                1 => PaymentMethod.Cash,
                2 => PaymentMethod.WeChat,
                3 => PaymentMethod.Alipay,
                4 => PaymentMethod.Card,
                _ => null
            };
        }
    }
}