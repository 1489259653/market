using System;
using System.Drawing;
using System.Windows.Forms;
using market.Models;

namespace market.Forms
{
    /// <summary>
    /// 供应商编辑表单
    /// </summary>
    public partial class SupplierEditForm : Form
    {
        private readonly Supplier _supplier;
        private bool _isEditMode;

        // 表单属性
        public string SupplierName { get; private set; }
        public string ProductionLocation { get; private set; }
        public string ContactInfo { get; private set; }
        public string BusinessLicense { get; private set; }

        /// <summary>
        /// 新建供应商构造函数
        /// </summary>
        public SupplierEditForm()
        {
            _isEditMode = false;
            InitializeForm();
        }

        /// <summary>
        /// 编辑供应商构造函数
        /// </summary>
        public SupplierEditForm(Supplier supplier)
        {
            _supplier = supplier;
            _isEditMode = true;
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = _isEditMode ? "编辑供应商" : "新建供应商";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 创建主面板
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // 供应商名称
            var lblName = new Label { Text = "供应商名称:", Location = new Point(20, 20), Width = 100 };
            var txtName = new TextBox { Name = "txtName", Location = new Point(120, 17), Width = 300 };

            // 生产地
            var lblLocation = new Label { Text = "生产地:", Location = new Point(20, 60), Width = 100 };
            var txtLocation = new TextBox { Name = "txtLocation", Location = new Point(120, 57), Width = 300 };

            // 联系方式
            var lblContact = new Label { Text = "联系方式:", Location = new Point(20, 100), Width = 100 };
            var txtContact = new TextBox { Name = "txtContact", Location = new Point(120, 97), Width = 300 };
            var lblContactHint = new Label { 
                Text = "格式: 联系人 电话 邮箱", 
                Location = new Point(120, 120), 
                Width = 300, 
                ForeColor = Color.Gray,
                Font = new Font("微软雅黑", 8)
            };

            // 营业执照
            var lblLicense = new Label { Text = "营业执照:", Location = new Point(20, 160), Width = 100 };
            var txtLicense = new TextBox { Name = "txtLicense", Location = new Point(120, 157), Width = 300 };

            // 按钮面板
            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(20) };
            var btnSave = new Button { Text = "保存", Size = new Size(80, 30), Location = new Point(300, 10) };
            var btnCancel = new Button { Text = "取消", Size = new Size(80, 30), Location = new Point(390, 10) };

            mainPanel.Controls.AddRange(new Control[] {
                lblName, txtName,
                lblLocation, txtLocation,
                lblContact, txtContact, lblContactHint,
                lblLicense, txtLicense
            });

            buttonPanel.Controls.AddRange(new Control[] { btnSave, btnCancel });

            this.Controls.Add(mainPanel);
            this.Controls.Add(buttonPanel);

            // 事件处理
            btnSave.Click += (s, e) => SaveSupplier();
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            // 初始化数据
            InitializeData();
        }

        private void InitializeData()
        {
            if (_isEditMode && _supplier != null)
            {
                var txtName = this.Controls[0].Controls["txtName"] as TextBox;
                var txtLocation = this.Controls[0].Controls["txtLocation"] as TextBox;
                var txtContact = this.Controls[0].Controls["txtContact"] as TextBox;
                var txtLicense = this.Controls[0].Controls["txtLicense"] as TextBox;

                txtName.Text = _supplier.Name;
                txtLocation.Text = _supplier.ProductionLocation ?? "";
                txtContact.Text = _supplier.ContactInfo ?? "";
                txtLicense.Text = _supplier.BusinessLicense ?? "";
            }
        }

        private void SaveSupplier()
        {
            if (!ValidateForm()) return;

            try
            {
                var txtName = this.Controls[0].Controls["txtName"] as TextBox;
                var txtLocation = this.Controls[0].Controls["txtLocation"] as TextBox;
                var txtContact = this.Controls[0].Controls["txtContact"] as TextBox;
                var txtLicense = this.Controls[0].Controls["txtLicense"] as TextBox;

                SupplierName = txtName.Text.Trim();
                ProductionLocation = txtLocation.Text.Trim();
                ContactInfo = txtContact.Text.Trim();
                BusinessLicense = txtLicense.Text.Trim();

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            var txtName = this.Controls[0].Controls["txtName"] as TextBox;

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("请输入供应商名称", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return false;
            }

            return true;
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SupplierEditForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "SupplierEditForm";
            this.ResumeLayout(false);
        }
        #endregion
    }
}