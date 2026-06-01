using System;
using System.Drawing;
using System.Windows.Forms;
using WinFormsApp1.BLL;
using WinFormsApp1.DAL;

namespace WinFormsApp1.UI
{
    public class MainForm : Form
    {
        private Panel panelToolbar;
        private Button btnLoadImage;
        private Button btnLineMode;
        private Button btnRectMode;
        private Button btnExitMode;
        private Button btnClearAll;
        private Button btnZoomIn;
        private Button btnZoomOut;
        private Label lblZoomLevel;
        private Label lblStatus;
        private DrawingPanel drawingPanel;

        private DrawingRepository _repository;
        private CoordinateConverter _converter;
        private MeasurementService _measurementService;
        private DrawingService _drawingService;

        private string _imagePath = "";
        private float _currentZoom = 1.0f;
        private const float MinZoom = 0.1f;
        private const float MaxZoom = 5.0f;

        public MainForm()
        {
            InitializeComponent();
            InitializeServices();
            SetupLayout();
        }

        private void InitializeServices()
        {
            _repository = new DrawingRepository();
            _converter = new CoordinateConverter(1000, 500, 20, 10);
            _measurementService = new MeasurementService(_converter);
            _drawingService = new DrawingService(_repository, _measurementService);
        }

        private void InitializeComponent()
        {
            this.Text = "GDI+ 绘图测量工具";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
        }

        private void SetupLayout()
        {
            panelToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            btnLoadImage = CreateButton("加载图片", 10, 8, 100);
            btnLoadImage.Click += BtnLoadImage_Click;

            btnLineMode = CreateButton("线段绘制", 120, 8, 100);
            btnLineMode.Click += BtnLineMode_Click;

            btnRectMode = CreateButton("矩形绘制", 230, 8, 100);
            btnRectMode.Click += BtnRectMode_Click;

            btnExitMode = CreateButton("退出绘制", 340, 8, 100);
            btnExitMode.Click += BtnExitMode_Click;

            btnClearAll = CreateButton("清除全部", 450, 8, 100);
            btnClearAll.Click += BtnClearAll_Click;

            btnZoomOut = CreateButton("缩小 (-)", 570, 8, 80);
            btnZoomOut.Click += BtnZoomOut_Click;

            btnZoomIn = CreateButton("放大 (+)", 660, 8, 80);
            btnZoomIn.Click += BtnZoomIn_Click;

            lblZoomLevel = new Label
            {
                Text = "100%",
                Location = new Point(750, 12),
                AutoSize = true,
                Font = new Font("Microsoft YaHei", 10F, FontStyle.Bold)
            };

            lblStatus = new Label
            {
                Text = "就绪 - 请先加载图片",
                Location = new Point(10, 42),
                AutoSize = true,
                Font = new Font("Microsoft YaHei", 9F)
            };

            panelToolbar.Controls.AddRange(new Control[] {
                btnLoadImage, btnLineMode, btnRectMode, btnExitMode, btnClearAll,
                btnZoomIn, btnZoomOut, lblZoomLevel, lblStatus
            });

            drawingPanel = new DrawingPanel(_repository, _drawingService, _converter)
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            this.Controls.Add(drawingPanel);
            this.Controls.Add(panelToolbar);
            this.Controls.SetChildIndex(drawingPanel, 0);

            btnLineMode.Enabled = false;
            btnRectMode.Enabled = false;
            btnClearAll.Enabled = false;
            btnZoomIn.Enabled = false;
            btnZoomOut.Enabled = false;

            UpdateModeButtons();
        }

        private Button CreateButton(string text, int x, int y, int width)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 30),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei", 9F),
                Cursor = Cursors.Hand
            };
        }

        public void UpdateStatus(string status)
        {
            lblStatus.Text = status;
        }

        public void UpdateZoomLabel()
        {
            lblZoomLevel.Text = $"{(int)(_currentZoom * 100)}%";
        }

        public float GetCurrentZoom()
        {
            return _currentZoom;
        }

        public void SetZoom(float zoom)
        {
            _currentZoom = zoom;
            UpdateZoomLabel();
        }

        public void EnableDrawingControls()
        {
            btnLineMode.Enabled = true;
            btnRectMode.Enabled = true;
            btnClearAll.Enabled = true;
            btnZoomIn.Enabled = true;
            btnZoomOut.Enabled = true;
        }

        private void UpdateModeButtons()
        {
            btnLineMode.BackColor = drawingPanel.DrawMode == DrawMode.Line ? Color.LightBlue : Color.FromArgb(240, 240, 240);
            btnRectMode.BackColor = drawingPanel.DrawMode == DrawMode.Rectangle ? Color.LightGreen : Color.FromArgb(240, 240, 240);
            btnExitMode.Enabled = drawingPanel.DrawMode != DrawMode.None;
        }

        private void BtnExitMode_Click(object? sender, EventArgs e)
        {
            drawingPanel.DrawMode = DrawMode.None;
            UpdateModeButtons();
            UpdateStatus("已退出绘制模式 - 点击矩形可选中并拖动");
        }

        private void BtnLoadImage_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                ofd.Title = "选择图片";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _imagePath = ofd.FileName;
                    try
                    {
                        Image img = Image.FromFile(_imagePath);
                        drawingPanel.SetImage(img);
                        _currentZoom = 1.0f;
                        UpdateZoomLabel();
                        EnableDrawingControls();
                        UpdateStatus($"已加载图片: {System.IO.Path.GetFileName(_imagePath)} | 图片尺寸: {img.Width}x{img.Height} px");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"加载图片失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnLineMode_Click(object? sender, EventArgs e)
        {
            drawingPanel.DrawMode = DrawMode.Line;
            UpdateModeButtons();
            UpdateStatus("线段绘制模式 - 左键点击定位点，右键结束绘制");
        }

        private void BtnRectMode_Click(object? sender, EventArgs e)
        {
            drawingPanel.DrawMode = DrawMode.Rectangle;
            UpdateModeButtons();
            UpdateStatus("矩形绘制模式 - 按住鼠标左键拖动绘制矩形");
        }

        private void BtnClearAll_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("确定要清除所有绘制内容吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _repository.ClearAll();
                drawingPanel.Invalidate();
                UpdateStatus("已清除所有绘制内容");
            }
        }

        private void BtnZoomIn_Click(object? sender, EventArgs e)
        {
            drawingPanel.ZoomAtMousePosition(1.2f);
        }

        private void BtnZoomOut_Click(object? sender, EventArgs e)
        {
            drawingPanel.ZoomAtMousePosition(0.8f);
        }
    }
}
