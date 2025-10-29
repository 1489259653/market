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
                // 检查窗体是否已释放
                if (this.IsDisposed)
                {
                    return;
                }
                
                // 停止任何正在运行的扫描
                StopScan();
                
                // 更新UI状态
                if (_btnCancel != null)
                    _btnCancel.Enabled = true;
                    
                if (_lblStatus != null)
                {
                    _lblStatus.Text = "正在启动摄像头...";
                    _lblStatus.BackColor = Color.LightGreen;
                }
                
                // 获取可用摄像头列表
                FilterInfoCollection videoDevices = null;
                try
                {
                    videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                    
                    if (videoDevices.Count == 0)
                    {
                        throw new Exception("未找到可用的摄像头设备");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"无法访问摄像头设备: {ex.Message}");
                }
                
                // 创建摄像头设备
                try
                {
                    if (videoDevices == null || videoDevices.Count == 0)
                    {
                        throw new Exception("摄像头设备列表为空");
                    }
                    
                    _captureDevice = new VideoCaptureDevice(videoDevices[0].MonikerString);
                    _captureDevice.NewFrame += CaptureDevice_NewFrame;
                    
                    // 设置摄像头参数以降低内存占用
                    _captureDevice.VideoResolution = _captureDevice.VideoCapabilities
                        .Where(cap => cap.FrameSize.Width <= 640 && cap.FrameSize.Height <= 480)
                        .OrderByDescending(cap => cap.FrameSize.Width * cap.FrameSize.Height)
                        .FirstOrDefault() ?? _captureDevice.VideoCapabilities[0];
                    
                    _captureDevice.Start();
                }
                catch (Exception ex)
                {
                    throw new Exception($"无法启动摄像头: {ex.Message}");
                }
                finally
                {
                    // 在创建摄像头设备后，videoDevices不再需要
                    videoDevices = null;
                }
                
                // 启动扫描线程
                _isScanning = true;
                _scanThread = new System.Threading.Thread(ScanLoop)
                {
                    Name = "BarcodeScannerThread",
                    IsBackground = true
                };
                _scanThread.Start();
                
                if (_lblStatus != null)
                    _lblStatus.Text = "摄像头已启动，正在扫描条形码...";
                
            }
            catch (Exception ex)
            {
                if (_lblStatus != null)
                {
                    _lblStatus.Text = "启动摄像头失败";
                    _lblStatus.BackColor = Color.LightPink;
                }
                
                MessageBox.Show($"无法启动摄像头: {ex.Message}\n请确保摄像头可用且未被其他程序占用。", 
                              "摄像头错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                StopScan();
            }
        }

        private void StopScan()
        {
            // 停止扫描线程
            _isScanning = false;
            
            // 给扫描线程一点时间自然结束
            if (_scanThread != null && _scanThread.IsAlive)
            {
                if (!_scanThread.Join(1000)) // 等待线程结束，最多1000ms
                {
                    // 在.NET Core/.NET 5+中，Thread.Abort()已废弃
                    // 使用CancellationToken或让线程自然结束
                    // 设置扫描标志为false，让线程自然退出
                    _isScanning = false;
                    
                    // 再给线程一些时间响应
                    if (!_scanThread.Join(500))
                    {
                        // 如果线程仍然不响应，记录警告但让线程继续运行
                        // 线程是后台线程，会在应用程序关闭时自动结束
                        System.Diagnostics.Debug.WriteLine("警告: 扫描线程未能在指定时间内结束");
                    }
                }
                _scanThread = null;
            }
            
            // 停止摄像头
            if (_captureDevice != null)
            {
                try
                {
                    _captureDevice.NewFrame -= CaptureDevice_NewFrame;
                    if (_captureDevice.IsRunning)
                    {
                        _captureDevice.SignalToStop();
                        _captureDevice.WaitForStop();
                    }
                }
                catch (Exception)
                {
                    // 忽略摄像头停止异常
                }
                finally
                {
                    // VideoCaptureDevice 不需要手动Dispose，只需释放引用
                    _captureDevice = null;
                }
            }
            
            // 停止计时器
            if (_scanTimer != null)
            {
                _scanTimer.Stop();
            }
            
            // 重置UI并释放图片资源
            if (_picCamera != null)
            {
                try
                {
                    if (_picCamera.Image != null)
                    {
                        var oldImage = _picCamera.Image;
                        _picCamera.Image = null;
                        oldImage.Dispose();
                    }
                    _picCamera.BackColor = Color.Black;
                }
                catch (Exception)
                {
                    // 忽略图片释放异常
                }
            }
            
            // 强制垃圾回收以释放内存
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            // 更新UI状态
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
            Bitmap bitmap = null;
            try
            {
                if (!_isScanning || this.IsDisposed || _picCamera == null)
                {
                    return;
                }

                // 创建新的Bitmap并立即释放原始帧
                bitmap = (Bitmap)eventArgs.Frame.Clone();
                
                // 创建镜像效果
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                
                // 安全地更新UI
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        UpdateCameraImage(bitmap);
                    }));
                }
                else
                {
                    UpdateCameraImage(bitmap);
                }
                
                // 在此方法中不要释放bitmap，UpdateCameraImage方法会处理
                bitmap = null; // 防止finally块中重复释放
            }
            catch (Exception)
            {
                // 忽略帧处理错误
            }
            finally
            {
                // 如果在try块中出现异常，确保bitmap被释放
                bitmap?.Dispose();
            }
        }
        
        private void UpdateCameraImage(Bitmap newImage)
        {
            if (this.IsDisposed || _picCamera == null || !_picCamera.IsHandleCreated)
            {
                newImage?.Dispose();
                return;
            }
            
            try
            {
                // 保存当前图像以便释放
                var oldImage = _picCamera.Image;
                
                // 设置新图像
                _picCamera.Image = newImage;
                
                // 释放旧图像
                oldImage?.Dispose();
            }
            catch (Exception)
            {
                // 如果UI更新失败，确保释放新图像
                newImage?.Dispose();
            }
        }
        
        private void ScanLoop()
        {
            // 添加扫描频率控制，避免过高的CPU使用率
            var scanInterval = TimeSpan.FromMilliseconds(200); // 每200ms扫描一次
            var lastScanTime = DateTime.Now;
            
            while (_isScanning && !this.IsDisposed)
            {
                try
                {
                    // 控制扫描频率
                    var now = DateTime.Now;
                    if (now - lastScanTime < scanInterval)
                    {
                        System.Threading.Thread.Sleep(10);
                        continue;
                    }
                    lastScanTime = now;
                    
                    Bitmap bitmap = null;
                    try
                    {
                        // 安全地获取当前图像
                        if (_picCamera?.Image != null && !this.IsDisposed)
                        {
                            // 在UI线程中安全地复制图像
                            if (this.InvokeRequired)
                            {
                                bitmap = (Bitmap)this.Invoke(new Func<Bitmap>(() =>
                                {
                                    return _picCamera?.Image != null ? 
                                        new Bitmap(_picCamera.Image) : null;
                                }));
                            }
                            else
                            {
                                bitmap = _picCamera?.Image != null ? 
                                    new Bitmap(_picCamera.Image) : null;
                            }
                            
                            if (bitmap != null)
                            {
                                // 尝试解码条形码
                                var result = _barcodeReader.Decode(bitmap);
                                
                                if (result != null && !string.IsNullOrEmpty(result.Text))
                                {
                                    // 扫描成功
                                    if (!this.IsDisposed)
                                    {
                                        this.Invoke(new Action(() =>
                                        {
                                            if (!this.IsDisposed)
                                            {
                                                ScanSuccess(result.Text);
                                            }
                                        }));
                                    }
                                    break; // 扫描成功后退出循环
                                }
                            }
                        }
                    }
                    finally
                    {
                        // 确保bitmap被释放
                        bitmap?.Dispose();
                    }
                    
                    // 短暂暂停以减少CPU使用率
                    System.Threading.Thread.Sleep(10);
                }
                catch (Exception)
                {
                    // 忽略扫描过程中的其他错误
                    System.Threading.Thread.Sleep(50);
                }
            }
        }

        private void ScanSuccess(string barcode)
        {
            try
            {
                // 停止扫描
                _isScanning = false;
                
                ScannedBarcode = barcode;
                
                if (_lblBarcode != null)
                    _lblBarcode.Text = barcode;
                    
                if (_lblStatus != null)
                {
                    _lblStatus.Text = "✓ 条形码扫描成功!";
                    _lblStatus.BackColor = Color.LightGreen;
                }

                // 播放提示音
                try
                {
                    SystemSounds.Beep.Play();
                }
                catch {}

                // 显示成功信息在摄像头画面上（如果图片存在）
                if (_picCamera?.Image != null)
                {
                    try
                    {
                        using (var graphics = Graphics.FromImage(_picCamera.Image))
                        {
                            // 创建半透明覆盖层
                            using (var overlayBrush = new SolidBrush(Color.FromArgb(100, Color.LightGreen)))
                            {
                                graphics.FillRectangle(overlayBrush, 0, 0, _picCamera.Image.Width, _picCamera.Image.Height);
                            }
                            
                            // 绘制成功信息
                            using (var font = new Font("微软雅黑", 16, FontStyle.Bold))
                            using (var brush = new SolidBrush(Color.DarkGreen))
                            {
                                var centerX = _picCamera.Image.Width / 2;
                                var centerY = _picCamera.Image.Height / 2;
                                
                                var successSize = graphics.MeasureString("✓ 扫描成功!", font);
                                graphics.DrawString("✓ 扫描成功!", font, brush, 
                                                   centerX - successSize.Width / 2, centerY - 30);
                                
                                var barcodeSize = graphics.MeasureString($"条形码: {barcode}", font);
                                graphics.DrawString($"条形码: {barcode}", font, brush, 
                                                   centerX - barcodeSize.Width / 2, centerY + 10);
                            }
                        }
                        
                        _picCamera.Refresh();
                    }
                    catch (Exception)
                    {
                        // 忽略图片处理错误
                    }
                }

                // 自动关闭窗体（使用Timer而不是Task，避免线程问题）
                var closeTimer = new System.Windows.Forms.Timer
                {
                    Interval = 1000,
                    Enabled = true
                };
                
                closeTimer.Tick += (s, e) =>
                {
                    closeTimer.Stop();
                    closeTimer.Dispose();
                    
                    if (!this.IsDisposed && !this.IsDisposed)
                    {
                        try
                        {
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        catch (Exception)
                        {
                            // 忽略关闭异常
                        }
                    }
                };
            }
            catch (Exception)
            {
                // 确保在异常情况下也能正常关闭
                try
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception)
                {
                    // 忽略关闭异常
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 确保停止所有扫描活动
                StopScan();
                
                // 释放计时器
                _scanTimer?.Stop();
                _scanTimer?.Dispose();
                _scanTimer = null;
                
                // 释放UI资源
                if (_picCamera != null)
                {
                    _picCamera.Image?.Dispose();
                    _picCamera.Image = null;
                }
                
                // BarcodeReader不需要Dispose，但可以设置为null
                _barcodeReader = null;
                
                // 强制垃圾回收
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            base.Dispose(disposing);
        }
        
        // 添加窗体关闭时的额外清理
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            StopScan();
        }
    }
}