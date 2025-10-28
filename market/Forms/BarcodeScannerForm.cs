using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace market.Forms
{
    /// <summary>
    /// 条形码扫描窗体
    /// </summary>
    public partial class BarcodeScannerForm : Form
    {
        private Button _btnStartScan;
        private Button _btnCancel;
        private PictureBox _picCamera;
        private Label _lblStatus;
        private Label _lblBarcode;
        private TextBox _txtManualBarcode;
        private Button _btnManualInput;
        private System.Windows.Forms.Timer _scanTimer;
        
        public string ScannedBarcode { get; private set; }

        // 模拟的条形码数据（在实际应用中，这里应该通过摄像头实时获取）
        private readonly string[] _sampleBarcodes = {
            "6901234567890", // EAN-13
            "123456789012",   // UPC-A
            "9780201379624",  // ISBN
            "6923456789012",  // 商品条码
            "6934567890123",  // 商品条码
            "6945678901234",  // 商品条码
            "6956789012345"   // 商品条码
        };

        public BarcodeScannerForm()
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "条形码扫描";
            this.Size = new Size(600, 500);
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

            // 摄像头预览区域
            var lblCamera = new Label
            {
                Text = "摄像头预览",
                Location = new Point(10, 10),
                Size = new Size(200, 25),
                Font = new Font("微软雅黑", 12, FontStyle.Bold)
            };

            _picCamera = new PictureBox
            {
                Location = new Point(10, 40),
                Size = new Size(560, 200),
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle
            };

            // 状态标签
            _lblStatus = new Label
            {
                Text = "请点击'开始扫描'按钮启动摄像头",
                Location = new Point(10, 250),
                Size = new Size(560, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.LightYellow,
                Font = new Font("微软雅黑", 10)
            };

            // 扫描到的条形码显示
            var lblBarcodeTitle = new Label
            {
                Text = "扫描到的条形码:",
                Location = new Point(10, 290),
                Size = new Size(120, 25),
                Font = new Font("微软雅黑", 10)
            };

            _lblBarcode = new Label
            {
                Text = "",
                Location = new Point(140, 290),
                Size = new Size(200, 25),
                Font = new Font("微软雅黑", 10, FontStyle.Bold),
                ForeColor = Color.Blue
            };

            // 手动输入区域
            var lblManualInput = new Label
            {
                Text = "或手动输入条形码:",
                Location = new Point(10, 330),
                Size = new Size(120, 25),
                Font = new Font("微软雅黑", 10)
            };

            _txtManualBarcode = new TextBox
            {
                Location = new Point(140, 330),
                Size = new Size(200, 25),
                PlaceholderText = "请输入13位条形码"
            };

            _btnManualInput = new Button
            {
                Text = "确认输入",
                Location = new Point(350, 330),
                Size = new Size(80, 25)
            };

            // 按钮区域
            var buttonPanel = new Panel
            {
                Location = new Point(10, 380),
                Size = new Size(560, 50)
            };

            _btnStartScan = new Button
            {
                Text = "开始扫描",
                Size = new Size(100, 35),
                Location = new Point(150, 10),
                BackColor = Color.Green,
                ForeColor = Color.White
            };

            _btnCancel = new Button
            {
                Text = "取消",
                Size = new Size(100, 35),
                Location = new Point(270, 10),
                BackColor = Color.Gray,
                ForeColor = Color.White
            };

            buttonPanel.Controls.Add(_btnStartScan);
            buttonPanel.Controls.Add(_btnCancel);

            // 添加控件到主面板
            mainPanel.Controls.AddRange(new Control[] {
                lblCamera, _picCamera, _lblStatus,
                lblBarcodeTitle, _lblBarcode,
                lblManualInput, _txtManualBarcode, _btnManualInput,
                buttonPanel
            });

            // 添加主面板到窗体
            this.Controls.Add(mainPanel);

            // 绑定事件
            BindEvents();

            // 初始化扫描计时器
            _scanTimer = new System.Windows.Forms.Timer
            {
                Interval = 100 // 100ms模拟扫描间隔
            };
            _scanTimer.Tick += ScanTimer_Tick;
        }

        private void BindEvents()
        {
            _btnStartScan.Click += (s, e) => StartScan();
            _btnCancel.Click += (s, e) => CancelScan();
            _btnManualInput.Click += (s, e) => ManualInputBarcode();
            
            // 手动输入框回车键确认
            _txtManualBarcode.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    ManualInputBarcode();
                }
            };

            // 窗体关闭事件
            this.FormClosing += (s, e) =>
            {
                StopScan();
            };
        }

        private void StartScan()
        {
            try
            {
                // 在实际应用中，这里应该启动摄像头
                // 这里使用模拟实现
                
                _btnStartScan.Enabled = false;
                _btnStartScan.Text = "扫描中...";
                _lblStatus.Text = "正在扫描条形码，请将条形码对准摄像头...";
                _lblStatus.BackColor = Color.LightGreen;

                // 显示模拟的摄像头画面
                _picCamera.BackColor = Color.DarkGray;
                using (var graphics = _picCamera.CreateGraphics())
                {
                    graphics.Clear(Color.DarkGray);
                    var font = new Font("微软雅黑", 16, FontStyle.Bold);
                    var brush = new SolidBrush(Color.White);
                    graphics.DrawString("摄像头预览", font, brush, new PointF(200, 80));
                    graphics.DrawString("📷 模拟扫描中...", font, brush, new PointF(180, 120));
                }

                // 启动扫描计时器
                _scanTimer.Start();

                MessageBox.Show("摄像头已启动，请将条形码对准摄像头进行扫描。\n\n" +
                              "注意：这是模拟功能，在实际应用中会使用真实摄像头。", 
                              "扫描提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动摄像头失败: {ex.Message}\n\n" +
                              "原因可能是：\n" +
                              "1. 摄像头设备未连接\n" +
                              "2. 摄像头被其他程序占用\n" +
                              "3. 缺少摄像头驱动程序\n" +
                              "4. 用户拒绝了摄像头访问权限", 
                              "摄像头错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                StopScan();
            }
        }

        private void StopScan()
        {
            _scanTimer.Stop();
            
            // 在实际应用中，这里应该停止摄像头
            _picCamera.BackColor = Color.Black;
            _btnStartScan.Enabled = true;
            _btnStartScan.Text = "开始扫描";
            _lblStatus.Text = "扫描已停止";
            _lblStatus.BackColor = Color.LightGray;
        }

        private void CancelScan()
        {
            StopScan();
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ScanTimer_Tick(object sender, EventArgs e)
        {
            // 模拟扫描过程
            // 在实际应用中，这里应该分析摄像头画面中的条形码
            
            // 随机模拟扫描成功
            var random = new Random();
            if (random.Next(0, 20) == 0) // 5%的概率模拟扫描成功
            {
                var barcode = _sampleBarcodes[random.Next(_sampleBarcodes.Length)];
                ScanSuccess(barcode);
            }
        }

        private void ScanSuccess(string barcode)
        {
            _scanTimer.Stop();
            
            ScannedBarcode = barcode;
            _lblBarcode.Text = barcode;
            _lblStatus.Text = "✓ 条形码扫描成功!";
            _lblStatus.BackColor = Color.LightGreen;

            // 显示成功动画
            _picCamera.BackColor = Color.LightGreen;
            using (var graphics = _picCamera.CreateGraphics())
            {
                graphics.Clear(Color.LightGreen);
                var font = new Font("微软雅黑", 16, FontStyle.Bold);
                var brush = new SolidBrush(Color.DarkGreen);
                graphics.DrawString("✓ 扫描成功!", font, brush, new PointF(220, 80));
                graphics.DrawString($"条形码: {barcode}", font, brush, new PointF(180, 120));
            }

            // 自动关闭窗体
            Task.Delay(1000).ContinueWith(t =>
            {
                if (!this.IsDisposed)
                {
                    this.Invoke(new Action(() =>
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }));
                }
            });
        }

        private void ManualInputBarcode()
        {
            var barcode = _txtManualBarcode.Text.Trim();
            
            if (string.IsNullOrEmpty(barcode))
            {
                MessageBox.Show("请输入条形码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtManualBarcode.Focus();
                return;
            }

            // 验证条形码格式（简单验证：10-13位数字）
            if (barcode.Length < 10 || barcode.Length > 13 || !long.TryParse(barcode, out _))
            {
                MessageBox.Show("请输入有效的条形码（10-13位数字）", "格式错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtManualBarcode.Focus();
                return;
            }

            ScannedBarcode = barcode;
            _lblBarcode.Text = barcode;
            _lblStatus.Text = "✓ 手动输入成功!";
            _lblStatus.BackColor = Color.LightBlue;

            // 显示成功信息
            _picCamera.BackColor = Color.LightBlue;
            using (var graphics = _picCamera.CreateGraphics())
            {
                graphics.Clear(Color.LightBlue);
                var font = new Font("微软雅黑", 16, FontStyle.Bold);
                var brush = new SolidBrush(Color.DarkBlue);
                graphics.DrawString("✓ 手动输入成功!", font, brush, new PointF(200, 80));
                graphics.DrawString($"条形码: {barcode}", font, brush, new PointF(180, 120));
            }

            // 自动关闭窗体
            Task.Delay(1000).ContinueWith(t =>
            {
                if (!this.IsDisposed)
                {
                    this.Invoke(new Action(() =>
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }));
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _scanTimer?.Stop();
                _scanTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}