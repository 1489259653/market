using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;
using ZXing.Windows.Compatibility;
using AForge.Video;
using AForge.Video.DirectShow;

namespace market.Forms
{
    /// <summary>
    /// 条形码扫描窗体
    /// </summary>
    public partial class BarcodeScannerForm : Form
    {
        private Button _btnCancel;
        private PictureBox _picCamera;
        private Label _lblStatus;
        private Label _lblBarcode;
        private System.Windows.Forms.Timer _scanTimer;
        
        // 条形码扫描相关组件
        private VideoCaptureDevice _captureDevice; // 摄像头捕获设备
        private BarcodeReader _barcodeReader; // 条形码解码器
        private System.Threading.Thread _scanThread; // 扫描线程
        private bool _isScanning; // 扫描状态标志
        
        public string ScannedBarcode { get; private set; }
        
        // 兼容性属性，与原来的Barcode属性相同
        public string Barcode { get { return ScannedBarcode; } }

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
                Text = "正在启动摄像头...",
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

            // 按钮区域
            var buttonPanel = new Panel
            {
                Location = new Point(10, 330),
                Size = new Size(560, 50)
            };

            _btnCancel = new Button
            {
                Text = "取消",
                Size = new Size(100, 35),
                Location = new Point(230, 10),
                BackColor = Color.Gray,
                ForeColor = Color.White
            };

            buttonPanel.Controls.Add(_btnCancel);

            // 添加控件到主面板
            mainPanel.Controls.AddRange(new Control[] {
                lblCamera, _picCamera, _lblStatus,
                lblBarcodeTitle, _lblBarcode,
                buttonPanel
            });

            // 添加主面板到窗体
            this.Controls.Add(mainPanel);

            // 绑定事件
            BindEvents();

            // 初始化扫描计时器（备用）
            _scanTimer = new System.Windows.Forms.Timer
            {
                Interval = 100 // 100ms模拟扫描间隔
            };
            // 不再使用计时器，直接通过摄像头帧处理
            
            // 初始化条形码解码器
            _barcodeReader = new BarcodeReader
            {
                AutoRotate = true,
                Options = new ZXing.Common.DecodingOptions
                {
                    TryHarder = true,
                    TryInverted = true,
                    PossibleFormats = new[] {
                        ZXing.BarcodeFormat.EAN_13,
                        ZXing.BarcodeFormat.EAN_8,
                        ZXing.BarcodeFormat.UPC_A,
                        ZXing.BarcodeFormat.UPC_E,
                        ZXing.BarcodeFormat.CODE_128,
                        ZXing.BarcodeFormat.CODE_39
                    }
                }
            };
            
            // 检查是否有可用的摄像头
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
            {
                _btnCancel.Enabled = true;
                _lblStatus.Text = "未检测到摄像头设备";
                _lblStatus.BackColor = Color.LightPink;
            }
            else
            {
                // 窗口加载后自动开始扫描
                this.Load += (s, e) => StartScan();
            }
        }

        private void BindEvents()
        {
            _btnCancel.Click += (s, e) => CancelScan();

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
                // 停止任何正在运行的扫描
                StopScan();
                
                _btnCancel.Enabled = true;
                _lblStatus.Text = "正在启动摄像头...";
                _lblStatus.BackColor = Color.LightGreen;
                
                // 获取可用摄像头列表
                var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count > 0)
                {
                    // 使用第一个可用摄像头
                    _captureDevice = new VideoCaptureDevice(videoDevices[0].MonikerString);
                    _captureDevice.NewFrame += CaptureDevice_NewFrame;
                    _captureDevice.Start();
                }
                else
                {
                    throw new Exception("未找到可用的摄像头设备");
                }
                
                // 启动扫描线程
                _isScanning = true;
                _scanThread = new System.Threading.Thread(ScanLoop);
                _scanThread.IsBackground = true;
                _scanThread.Start();
                
                _lblStatus.Text = "摄像头已启动，正在扫描条形码...";
                
            } catch (Exception ex)
            {
                _lblStatus.Text = "启动摄像头失败";
                _lblStatus.BackColor = Color.LightPink;
                MessageBox.Show($"无法启动摄像头: {ex.Message}\n请确保摄像头可用且未被其他程序占用。", 
                              "摄像头错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                StopScan();
            }
        }

        private void StopScan()
        {
            // 停止扫描线程
            _isScanning = false;
            if (_scanThread != null && _scanThread.IsAlive)
            {
                _scanThread.Join(1000); // 等待线程结束，最多1秒
            }
            
            // 停止摄像头
            if (_captureDevice != null)
            {
                _captureDevice.NewFrame -= CaptureDevice_NewFrame;
                if (_captureDevice.IsRunning)
                {
                    _captureDevice.SignalToStop();
                    _captureDevice.WaitForStop();
                }
                _captureDevice = null;
            }
            
            _scanTimer.Stop();
            
            // 重置UI
            if (_picCamera != null)
            {
                _picCamera.Image?.Dispose();
                _picCamera.Image = null;
                _picCamera.BackColor = Color.Black;
            }
            
            if (_btnCancel != null)
            {
                _btnCancel.Enabled = true;
            }
            
            if (_lblStatus != null)
            {
                _lblStatus.Text = "扫描已停止";
                _lblStatus.BackColor = Color.LightGray;
            }
        }

        private void CancelScan()
        {
            StopScan();
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void CaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                // 将摄像头捕获的帧显示在PictureBox中（镜像模式）
                Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
                
                // 创建镜像效果
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                
                if (!this.IsDisposed && _picCamera != null)
                {
                    // 先释放之前的图像，避免内存泄漏
                    Image oldImage = null;
                    this.Invoke(new Action(() =>
                    {
                        oldImage = _picCamera.Image;
                        _picCamera.Image = bitmap;
                    }));
                    
                    // 在UI线程外释放旧图像
                    oldImage?.Dispose();
                }
                else
                {
                    bitmap.Dispose();
                }
            } catch (Exception)
            {
                // 忽略帧处理错误
            }
        }
        
        private void ScanLoop()
        {
            while (_isScanning)
            {
                try
                {
                    if (_picCamera.Image != null)
                    {
                        // 复制图片以避免跨线程问题
                        Bitmap bitmap;
                        lock (_picCamera.Image)
                        {
                            bitmap = new Bitmap(_picCamera.Image);
                        }
                        
                        // 尝试解码条形码
                        var result = _barcodeReader.Decode(bitmap);
                        bitmap.Dispose();
                        
                        if (result != null)
                        {
                            // 扫描成功
                            this.Invoke(new Action(() =>
                            {
                                ScanSuccess(result.Text);
                            }));
                            break; // 扫描成功后退出循环
                        }
                    }
                    
                    // 短暂暂停以减少CPU使用率
                    System.Threading.Thread.Sleep(50);
                } catch (Exception)
                {
                    // 忽略扫描过程中的错误
                }
            }
        }

        private void ScanSuccess(string barcode)
        {
            // 停止扫描
            _isScanning = false;
            
            ScannedBarcode = barcode;
            _lblBarcode.Text = barcode;
            _lblStatus.Text = "✓ 条形码扫描成功!";
            _lblStatus.BackColor = Color.LightGreen;

            // 播放提示音
            try
            {
                SystemSounds.Beep.Play();
            } catch {}

            // 显示成功信息在摄像头画面上
            if (_picCamera.Image != null)
            {
                using (var graphics = Graphics.FromImage(_picCamera.Image))
                {
                    // 创建半透明覆盖层
                    using (var overlayBrush = new SolidBrush(Color.FromArgb(100, Color.LightGreen)))
                    {
                        graphics.FillRectangle(overlayBrush, 0, 0, _picCamera.Image.Width, _picCamera.Image.Height);
                    }
                    
                    // 绘制成功信息
                    var font = new Font("微软雅黑", 16, FontStyle.Bold);
                    var brush = new SolidBrush(Color.DarkGreen);
                    var centerX = _picCamera.Image.Width / 2;
                    var centerY = _picCamera.Image.Height / 2;
                    
                    var successSize = graphics.MeasureString("✓ 扫描成功!", font);
                    graphics.DrawString("✓ 扫描成功!", font, brush, 
                                       centerX - successSize.Width / 2, centerY - 30);
                    
                    var barcodeSize = graphics.MeasureString($"条形码: {barcode}", font);
                    graphics.DrawString($"条形码: {barcode}", font, brush, 
                                       centerX - barcodeSize.Width / 2, centerY + 10);
                }
                
                _picCamera.Refresh();
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
                StopScan();
                _scanTimer?.Dispose();
                // BarcodeReader不需要Dispose
            }
            base.Dispose(disposing);
        }
    }
}