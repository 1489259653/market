using System;
using System.Drawing;
using System.Windows.Forms;
using market.Models;
using market.Services;

namespace market.Forms
{
    public partial class ReturnOrderItemEditForm : Form
    {
        private ReturnOrderItem _returnItem;
        private readonly ProductService _productService;
        
        private TextBox txtProductCode;
        private TextBox txtProductName;
        private NumericUpDown numQuantity;
        private NumericUpDown numReturnPrice;
        private TextBox txtReason;
        private Button btnSearchProduct;
        private Button btnSave;
        private Button btnCancel;

        public ReturnOrderItemEditForm(ProductService productService, ReturnOrderItem existingItem = null)
        {
            _productService = productService;
            _returnItem = existingItem ?? new ReturnOrderItem();
            
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            // 窗体基本设置
            this.Text = "退货商品明细编辑";
            this.Size = new Size(400, 300);
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

            // 商品编码
            var lblProductCode = new Label { Text = "商品编码:", Location = new Point(10, 20), Size = new Size(80, 20) };
            txtProductCode = new TextBox { Location = new Point(100, 17), Size = new Size(200, 25) };
            btnSearchProduct = new Button { Text = "搜索", Location = new Point(310, 17), Size = new Size(60, 25) };
            btnSearchProduct.Click += BtnSearchProduct_Click;

            // 商品名称
            var lblProductName = new Label { Text = "商品名称:", Location = new Point(10, 55), Size = new Size(80, 20) };
            txtProductName = new TextBox { Location = new Point(100, 52), Size = new Size(270, 25), ReadOnly = true };

            // 退货数量
            var lblQuantity = new Label { Text = "退货数量:", Location = new Point(10, 90), Size = new Size(80, 20) };
            numQuantity = new NumericUpDown { Location = new Point(100, 87), Size = new Size(120, 25), Minimum = 1, Maximum = 10000 };

            // 退货单价
            var lblReturnPrice = new Label { Text = "退货单价:", Location = new Point(10, 125), Size = new Size(80, 20) };
            numReturnPrice = new NumericUpDown { Location = new Point(100, 122), Size = new Size(120, 25), DecimalPlaces = 2, Minimum = 0, Maximum = 100000 };

            // 退货原因
            var lblReason = new Label { Text = "退货原因:", Location = new Point(10, 160), Size = new Size(80, 20) };
            txtReason = new TextBox { Location = new Point(100, 157), Size = new Size(270, 25) };

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
                Location = new Point(200, 10),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White
            };
            btnSave.Click += BtnSave_Click;

            // 取消按钮
            btnCancel = new Button
            {
                Text = "取消",
                Location = new Point(290, 10),
                Size = new Size(80, 30),
                BackColor = Color.Gray,
                ForeColor = Color.White
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            // 添加控件到主面板
            mainPanel.Controls.AddRange(new Control[] {
                lblProductCode, txtProductCode, btnSearchProduct,
                lblProductName, txtProductName,
                lblQuantity, numQuantity,
                lblReturnPrice, numReturnPrice,
                lblReason, txtReason
            });

            buttonPanel.Controls.AddRange(new Control[] { btnSave, btnCancel });

            // 添加面板到窗体
            this.Controls.AddRange(new Control[] { mainPanel, buttonPanel });
        }

        private void LoadData()
        {
            // 加载现有数据
            txtProductCode.Text = _returnItem.ProductCode;
            txtProductName.Text = _returnItem.ProductName;
            numQuantity.Value = _returnItem.Quantity > 0 ? _returnItem.Quantity : 1;
            numReturnPrice.Value = _returnItem.ReturnPrice > 0 ? _returnItem.ReturnPrice : 0;
            txtReason.Text = _returnItem.Reason;
        }

        private void BtnSearchProduct_Click(object sender, EventArgs e)
        {
            try
            {
                // 调用商品搜索窗体
                var selectedProduct = ProductSearchForm.ShowProductSearch(_productService);
                
                if (selectedProduct != null)
                {
                    // 自动填充商品信息
                    txtProductCode.Text = selectedProduct.ProductCode;
                    txtProductName.Text = selectedProduct.Name;
                    numReturnPrice.Value = selectedProduct.Price; // 默认使用销售价格作为退货单价
                    numQuantity.Value = 1; // 默认数量为1
                    
                    // 自动聚焦到数量输入框
                    numQuantity.Focus();
                    numQuantity.Select(0, numQuantity.Text.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"商品搜索失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // 验证数据
                if (string.IsNullOrEmpty(txtProductCode.Text))
                {
                    MessageBox.Show("请输入商品编码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtProductCode.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(txtProductName.Text))
                {
                    MessageBox.Show("请输入商品名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (numQuantity.Value <= 0)
                {
                    MessageBox.Show("退货数量必须大于0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    numQuantity.Focus();
                    return;
                }

                if (numReturnPrice.Value < 0)
                {
                    MessageBox.Show("退货单价不能为负数", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    numReturnPrice.Focus();
                    return;
                }

                // 更新退货商品明细
                _returnItem.ProductCode = txtProductCode.Text;
                _returnItem.ProductName = txtProductName.Text;
                _returnItem.Quantity = (int)numQuantity.Value;
                _returnItem.ReturnPrice = numReturnPrice.Value;
                _returnItem.Reason = txtReason.Text;
                _returnItem.CalculateAmount();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存退货商品明细失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 获取退货商品明细
        /// </summary>
        /// <returns>退货商品明细</returns>
        public ReturnOrderItem GetReturnItem()
        {
            return _returnItem;
        }
    }
}