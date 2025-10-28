using System;
using System.Drawing;
using System.Windows.Forms;
using market.Models;

namespace market.Forms
{
    public partial class PaymentForm : Form
    {
        private decimal _amount;
        private PaymentMethod _paymentMethod;

        public decimal ReceivedAmount { get; private set; }
        public decimal ChangeAmount { get; private set; }

        private TextBox _txtReceivedAmount;
        private Label _lblChangeAmount;
        private ComboBox _cmbPaymentMethod;

        public PaymentForm(decimal amount, PaymentMethod paymentMethod = PaymentMethod.Cash)
        {
            _amount = amount;
            _paymentMethod = paymentMethod;
            
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeComponent()
        {
            this.Text = "支付结算";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void InitializeForm()
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // 应付金额显示
            var lblAmount = new Label 
            { 
                Text = $"应付金额:", 
                Location = new Point(10, 20), 
                Width = 80,
                Font = new Font("微软雅黑", 12, FontStyle.Bold)
            };
            
            var lblAmountValue = new Label 
            { 
                Text = $"￥{_amount:F2}", 
                Location = new Point(100, 20), 
                Width = 150,
                Font = new Font("微软雅黑", 14, FontStyle.Bold),
                ForeColor = Color.Blue
            };

            // 支付方式选择
            var lblPaymentMethod = new Label { Text = "支付方式:", Location = new Point(10, 60), Width = 80 };
            _cmbPaymentMethod = new ComboBox 
            { 
                Location = new Point(100, 57), 
                Width = 150, 
                DropDownStyle = ComboBoxStyle.DropDownList 
            };
            _cmbPaymentMethod.Items.AddRange(new object[] { "现金", "微信支付", "支付宝", "银行卡" });
            _cmbPaymentMethod.SelectedIndex = (int)_paymentMethod;

            // 实收金额输入
            var lblReceivedAmount = new Label { Text = "实收金额:", Location = new Point(10, 100), Width = 80 };
            _txtReceivedAmount = new TextBox { Location = new Point(100, 97), Width = 150 };
            _txtReceivedAmount.Text = _amount.ToString("F2");

            // 找零金额显示
            var lblChange = new Label { Text = "找零金额:", Location = new Point(10, 140), Width = 80 };
            _lblChangeAmount = new Label 
            { 
                Text = "￥0.00", 
                Location = new Point(100, 140), 
                Width = 150,
                Font = new Font("微软雅黑", 12, FontStyle.Bold),
                ForeColor = Color.Green
            };

            // 按钮
            var btnOK = new Button { Text = "确认支付", Location = new Point(80, 190), Size = new Size(80, 35) };
            var btnCancel = new Button { Text = "取消", Location = new Point(180, 190), Size = new Size(80, 35) };

            // 快速金额按钮
            var btn100 = new Button { Text = "100", Location = new Point(260, 57), Size = new Size(40, 25) };
            var btn200 = new Button { Text = "200", Location = new Point(310, 57), Size = new Size(40, 25) };
            var btn500 = new Button { Text = "500", Location = new Point(260, 87), Size = new Size(40, 25) };
            var btn1000 = new Button { Text = "1000", Location = new Point(310, 87), Size = new Size(40, 25) };

            mainPanel.Controls.AddRange(new Control[] {
                lblAmount, lblAmountValue,
                lblPaymentMethod, _cmbPaymentMethod,
                lblReceivedAmount, _txtReceivedAmount,
                lblChange, _lblChangeAmount,
                btnOK, btnCancel,
                btn100, btn200, btn500, btn1000
            });

            this.Controls.Add(mainPanel);

            // 事件处理
            btnOK.Click += (s, e) => ConfirmPayment();
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            _txtReceivedAmount.TextChanged += (s, e) => CalculateChange();
            _cmbPaymentMethod.SelectedIndexChanged += (s, e) => UpdatePaymentMethod();

            // 快速金额按钮事件
            btn100.Click += (s, e) => SetReceivedAmount(100);
            btn200.Click += (s, e) => SetReceivedAmount(200);
            btn500.Click += (s, e) => SetReceivedAmount(500);
            btn1000.Click += (s, e) => SetReceivedAmount(1000);

            // 初始化计算
            CalculateChange();
        }

        private void SetReceivedAmount(decimal amount)
        {
            _txtReceivedAmount.Text = amount.ToString("F2");
        }

        private void CalculateChange()
        {
            if (decimal.TryParse(_txtReceivedAmount.Text, out decimal received))
            {
                ReceivedAmount = received;
                ChangeAmount = received - _amount;
                
                if (ChangeAmount >= 0)
                {
                    _lblChangeAmount.Text = $"￥{ChangeAmount:F2}";
                    _lblChangeAmount.ForeColor = Color.Green;
                }
                else
                {
                    _lblChangeAmount.Text = $"不足: ￥{-ChangeAmount:F2}";
                    _lblChangeAmount.ForeColor = Color.Red;
                }
            }
            else
            {
                _lblChangeAmount.Text = "￥0.00";
                _lblChangeAmount.ForeColor = Color.Green;
            }
        }

        private void UpdatePaymentMethod()
        {
            var paymentMethod = (PaymentMethod)_cmbPaymentMethod.SelectedIndex;
            
            // 如果是电子支付，自动设置实收金额为应付金额
            if (paymentMethod != PaymentMethod.Cash)
            {
                _txtReceivedAmount.Text = _amount.ToString("F2");
                _txtReceivedAmount.ReadOnly = true;
            }
            else
            {
                _txtReceivedAmount.ReadOnly = false;
            }
        }

        private void ConfirmPayment()
        {
            try
            {
                // 验证实收金额
                if (!decimal.TryParse(_txtReceivedAmount.Text, out decimal received))
                {
                    MessageBox.Show("请输入有效的金额", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _txtReceivedAmount.Focus();
                    return;
                }

                ReceivedAmount = received;
                ChangeAmount = received - _amount;

                var paymentMethod = (PaymentMethod)_cmbPaymentMethod.SelectedIndex;

                // 如果是现金支付，检查实收金额是否足够
                if (paymentMethod == PaymentMethod.Cash && received < _amount)
                {
                    MessageBox.Show("实收金额不足，请重新输入", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _txtReceivedAmount.Focus();
                    return;
                }

                // 如果是电子支付，实收金额必须等于应付金额
                if (paymentMethod != PaymentMethod.Cash && received != _amount)
                {
                    MessageBox.Show("电子支付金额必须等于应付金额", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 显示确认信息
                string message = $"支付信息确认:\n" +
                              $"应付金额: ￥{_amount:F2}\n" +
                              $"实收金额: ￥{received:F2}\n";

                if (paymentMethod == PaymentMethod.Cash)
                {
                    message += $"找零金额: ￥{ChangeAmount:F2}\n";
                }

                message += $"支付方式: {_cmbPaymentMethod.SelectedItem}";

                if (MessageBox.Show(message, "确认支付", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    this.DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"支付确认失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            
            // 根据支付方式设置焦点
            if (_cmbPaymentMethod.SelectedIndex == (int)PaymentMethod.Cash)
            {
                _txtReceivedAmount.Focus();
                _txtReceivedAmount.SelectAll();
            }
            else
            {
                _cmbPaymentMethod.Focus();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // 支持ESC键取消
            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                return true;
            }
            
            // 支持Enter键确认
            if (keyData == Keys.Enter)
            {
                ConfirmPayment();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}