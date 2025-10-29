using System;
using System.Collections.Generic;
using System.Windows.Forms;
using market.Models;
using market.Services;

namespace market.Forms
{
    public partial class OperationLogForm : Form
    {
        private readonly LogService _logService;
        private int _currentPage = 1;
        private const int PageSize = 20;
        private int _totalRecords = 0;

        public OperationLogForm(LogService logService)
        {
            _logService = logService;
            InitializeUI();
        }

        private void InitializeUI()
        {
            Text = "操作日志管理";
            Size = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterScreen;

            // 使用TableLayoutPanel进行布局
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // 查询条件区域
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // 数据展示区域
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));  // 分页控制区域

            // 查询条件面板
            var searchPanel = CreateSearchPanel();
            mainLayout.Controls.Add(searchPanel, 0, 0);

            // 数据网格视图
            InitializeDataGridView();
            mainLayout.Controls.Add(_dgvLogs, 0, 1);

            // 分页控制面板
            var paginationPanel = CreatePaginationPanel();
            mainLayout.Controls.Add(paginationPanel, 0, 2);

            Controls.Add(mainLayout);

            // 加载操作类型下拉框
            LoadOperationTypes();
            // 加载数据
            LoadLogs();
        }

        private Panel CreateSearchPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            var searchLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75));
            searchLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            searchLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            // 操作类型
            searchLayout.Controls.Add(new Label { Text = "操作类型:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 0);
            _cmbOperationType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            _cmbOperationType.Items.Add("全部");
            searchLayout.Controls.Add(_cmbOperationType, 1, 0);

            // 操作用户
            searchLayout.Controls.Add(new Label { Text = "操作用户:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 2, 0);
            _txtUserId = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "输入用户ID或名称" };
            searchLayout.Controls.Add(_txtUserId, 3, 0);

            // 时间范围
            searchLayout.Controls.Add(new Label { Text = "开始时间:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 1);
            _dtpStartTime = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm", Dock = DockStyle.Fill };
            _dtpStartTime.Value = DateTime.Now.AddDays(-7);
            searchLayout.Controls.Add(_dtpStartTime, 1, 1);

            searchLayout.Controls.Add(new Label { Text = "结束时间:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 2, 1);
            _dtpEndTime = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm", Dock = DockStyle.Fill };
            _dtpEndTime.Value = DateTime.Now;
            searchLayout.Controls.Add(_dtpEndTime, 3, 1);

            panel.Controls.Add(searchLayout);

            // 查询按钮
            var btnSearch = new Button { Text = "查询", Width = 80, Dock = DockStyle.Right };
            btnSearch.Click += (s, e) =>
            {
                _currentPage = 1;
                LoadLogs();
            };
            panel.Controls.Add(btnSearch);

            return panel;
        }

        private void InitializeDataGridView()
        {
            _dgvLogs = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersDefaultCellStyle = { Font = new System.Drawing.Font("微软雅黑", 9, System.Drawing.FontStyle.Bold) }
            };

            // 添加列
            _dgvLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "日志ID",
                DataPropertyName = "Id",
                Width = 80
            });

            _dgvLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OperationType",
                HeaderText = "操作类型",
                DataPropertyName = "OperationType",
                Width = 120
            });

            _dgvLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "UserId",
                HeaderText = "用户ID",
                DataPropertyName = "UserId",
                Width = 100
            });

            _dgvLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Username",
                HeaderText = "用户名称",
                DataPropertyName = "Username",
                Width = 120
            });

            _dgvLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OperationTime",
                HeaderText = "操作时间",
                DataPropertyName = "OperationTime",
                Width = 180,
                DefaultCellStyle = { Format = "yyyy-MM-dd HH:mm:ss" }
            });

            _dgvLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Details",
                HeaderText = "操作详情",
                DataPropertyName = "Details",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        private Panel CreatePaginationPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            var paginationLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 1
            };
            paginationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            paginationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            paginationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            paginationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            paginationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));

            // 分页信息
            _lblPageInfo = new Label { TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            paginationLayout.Controls.Add(_lblPageInfo, 0, 0);

            // 上一页
            _btnPrevPage = new Button { Text = "上一页", Width = 70, Dock = DockStyle.Fill };
            _btnPrevPage.Click += (s, e) =>
            {
                if (_currentPage > 1)
                {
                    _currentPage--;
                    LoadLogs();
                }
            };
            paginationLayout.Controls.Add(_btnPrevPage, 1, 0);

            // 下一页
            _btnNextPage = new Button { Text = "下一页", Width = 70, Dock = DockStyle.Fill };
            _btnNextPage.Click += (s, e) =>
            {
                int totalPages = (int)Math.Ceiling((double)_totalRecords / PageSize);
                if (_currentPage < totalPages)
                {
                    _currentPage++;
                    LoadLogs();
                }
            };
            paginationLayout.Controls.Add(_btnNextPage, 2, 0);

            // 首页
            _btnFirstPage = new Button { Text = "首页", Width = 70, Dock = DockStyle.Fill };
            _btnFirstPage.Click += (s, e) =>
            {
                if (_currentPage > 1)
                {
                    _currentPage = 1;
                    LoadLogs();
                }
            };
            paginationLayout.Controls.Add(_btnFirstPage, 3, 0);

            // 末页
            _btnLastPage = new Button { Text = "末页", Width = 70, Dock = DockStyle.Fill };
            _btnLastPage.Click += (s, e) =>
            {
                int totalPages = (int)Math.Ceiling((double)_totalRecords / PageSize);
                if (_currentPage < totalPages)
                {
                    _currentPage = totalPages;
                    LoadLogs();
                }
            };
            paginationLayout.Controls.Add(_btnLastPage, 4, 0);

            panel.Controls.Add(paginationLayout);
            return panel;
        }

        private void LoadOperationTypes()
        {
            var types = _logService.GetAllOperationTypes();
            foreach (var type in types)
            {
                if (!_cmbOperationType.Items.Contains(type))
                {
                    _cmbOperationType.Items.Add(type);
                }
            }
            _cmbOperationType.SelectedIndex = 0;
        }

        private void LoadLogs()
        {
            try
            {
                // 获取查询条件
                string operationType = _cmbOperationType.SelectedIndex > 0 ? _cmbOperationType.SelectedItem.ToString() : null;
                string userId = string.IsNullOrWhiteSpace(_txtUserId.Text) ? null : _txtUserId.Text;
                DateTime startTime = _dtpStartTime.Value;
                DateTime endTime = _dtpEndTime.Value;

                // 查询数据
                var result = _logService.GetOperationLogs(operationType, userId, startTime, endTime, _currentPage, PageSize);
                var logs = result.Item1;
                _totalRecords = result.Item2;

                // 绑定数据
                _dgvLogs.DataSource = logs;

                // 更新分页信息
                int totalPages = (int)Math.Ceiling((double)_totalRecords / PageSize);
                _lblPageInfo.Text = $"共 {_totalRecords} 条记录，第 {_currentPage} / {totalPages} 页";

                // 更新分页按钮状态
                _btnFirstPage.Enabled = _currentPage > 1;
                _btnPrevPage.Enabled = _currentPage > 1;
                _btnNextPage.Enabled = _currentPage < totalPages;
                _btnLastPage.Enabled = _currentPage < totalPages;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载日志失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 声明控件
        private ComboBox _cmbOperationType;
        private TextBox _txtUserId;
        private DateTimePicker _dtpStartTime;
        private DateTimePicker _dtpEndTime;
        private DataGridView _dgvLogs;
        private Label _lblPageInfo;
        private Button _btnFirstPage;
        private Button _btnPrevPage;
        private Button _btnNextPage;
        private Button _btnLastPage;
    }
}