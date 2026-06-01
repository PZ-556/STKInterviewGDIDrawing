using GDIDrawing.BLL;
using GDIDrawing.Common;
using System.Drawing;

namespace GDIDrawing.UI
{
    public partial class MainForm : Form
    {
        private DrawingRepository _repository = null!;
        private CoordinateConverter _converter = null!;
        private MeasurementService _measurementService = null!;
        private DrawingService _drawingService = null!;

        private string _imagePath = "";
        private float _currentZoom = 1.0f;

        public MainForm()
        {
            InitializeServices();
            InitializeComponent();
        }

        private void InitializeServices()
        {
            _repository = new DrawingRepository();
            _converter = new CoordinateConverter(1000, 500, 20, 10);
            _measurementService = new MeasurementService(_converter);
            _drawingService = new DrawingService(_repository, _measurementService);
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
                        UpdateStatus($"已加载图片: {Path.GetFileName(_imagePath)} | 图片尺寸: {img.Width}x{img.Height} px");
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
