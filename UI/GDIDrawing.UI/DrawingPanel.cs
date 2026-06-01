using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GDIDrawing.BLL;
using GDIDrawing.Common;
using GDIDrawing.Model;

namespace GDIDrawing.UI
{
    public enum DrawMode
    {
        None,
        Line,
        Rectangle
    }

    public class DrawingPanel : Panel
    {
        private DrawingRepository _repository;
        private DrawingService _drawingService;
        private CoordinateConverter _converter;

        private Image? _image;
        private DrawMode _drawMode;
        private PointF _offset;
        private float _zoom = 1.0f;
        private const float MinZoom = 0.1f;
        private const float MaxZoom = 5.0f;

        private bool _isDrawing;
        private PointF _drawStartPoint;
        private PointF _currentMousePoint;

        private bool _isDragging;
        private DrawingRectangle? _selectedRect;
        private PointF _dragStartPoint;
        private PointF _rectOriginalLocation;

        private bool _isLineDrawing;
        private DrawingPoint? _lineStartPoint;
        private bool _hasLineStart;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DrawMode DrawMode
        {
            get => _drawMode;
            set
            {
                _drawMode = value;
                if (_drawMode != DrawMode.Line)
                {
                    _isLineDrawing = false;
                    _hasLineStart = false;
                }
                if (_drawMode != DrawMode.Rectangle)
                {
                    _isDrawing = false;
                }
                this.Invalidate();
            }
        }

        public DrawingPanel(DrawingRepository repository, DrawingService drawingService, CoordinateConverter converter)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            UpdateStyles();

            _repository = repository;
            _drawingService = drawingService;
            _converter = converter;
            _offset = new PointF(0, 0);
            _drawMode = DrawMode.None;

            this.MouseDown += DrawingPanel_MouseDown;
            this.MouseMove += DrawingPanel_MouseMove;
            this.MouseUp += DrawingPanel_MouseUp;
            this.MouseWheel += DrawingPanel_MouseWheel;
            this.Paint += DrawingPanel_Paint;
        }

        public void SetImage(Image image)
        {
            _image = image;
            CenterImage();
            this.Invalidate();
        }

        private void CenterImage()
        {
            if (_image == null) return;

            float imgWidth = _image.Width * _zoom;
            float imgHeight = _image.Height * _zoom;

            _offset.X = (this.Width - imgWidth) / 2;
            _offset.Y = (this.Height - imgHeight) / 2;
        }

        public void ZoomAtMousePosition(float zoomFactor)
        {
            if (_image == null) return;

            float oldZoom = _zoom;
            float newZoom = _zoom * zoomFactor;
            newZoom = Math.Max(MinZoom, Math.Min(MaxZoom, newZoom));

            if (Math.Abs(newZoom - oldZoom) < 0.001f) return;

            Point mousePos = this.PointToClient(Cursor.Position);
            float mouseX = mousePos.X;
            float mouseY = mousePos.Y;

            float imageX = (mouseX - _offset.X) / oldZoom;
            float imageY = (mouseY - _offset.Y) / oldZoom;

            _zoom = newZoom;

            _offset.X = mouseX - imageX * _zoom;
            _offset.Y = mouseY - imageY * _zoom;

            ((MainForm)this.FindForm()).SetZoom(_zoom);
            this.Invalidate();
        }

        private void DrawingPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            if (_image == null) return;

            float zoomFactor = e.Delta > 0 ? 1.1f : 0.9f;
            ZoomAtMousePosition(zoomFactor);
        }

        private void DrawingPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            if (_image == null)
            {
                e.Graphics.DrawString("请加载图片", new Font("Microsoft YaHei", 20), Brushes.Gray, 
                    this.Width / 2 - 80, this.Height / 2 - 20);
                return;
            }

            float imgWidth = _image.Width * _zoom;
            float imgHeight = _image.Height * _zoom;

            e.Graphics.DrawImage(_image, _offset.X, _offset.Y, imgWidth, imgHeight);

            e.Graphics.TranslateTransform(_offset.X, _offset.Y);
            e.Graphics.ScaleTransform(_zoom, _zoom);

            foreach (var line in _repository.LineSegments)
            {
                DrawLineSegment(e.Graphics, line);
            }

            foreach (var rect in _repository.Rectangles)
            {
                DrawRectangle(e.Graphics, rect);
            }

            if (_isDrawing && _drawMode == DrawMode.Rectangle)
            {
                DrawPreviewRectangle(e.Graphics);
            }

            if (_isLineDrawing && _hasLineStart)
            {
                DrawPreviewLine(e.Graphics);
            }

            e.Graphics.ResetTransform();
        }

        private void DrawLineSegment(Graphics g, LineSegment line)
        {
            using (Pen pen = new Pen(line.LineColor, 2.0f / _zoom))
            {
                g.DrawLine(pen, line.StartPoint.X, line.StartPoint.Y, 
                    line.EndPoint.X, line.EndPoint.Y);
            }

            DrawPointMarker(g, line.StartPoint.X, line.StartPoint.Y);
            DrawPointMarker(g, line.EndPoint.X, line.EndPoint.Y);

            DrawDistanceLabel(g, line.StartPoint.X, line.StartPoint.Y, 
                line.EndPoint.X, line.EndPoint.Y, line.DistanceMM);
        }

        private void DrawPointMarker(Graphics g, float x, float y)
        {
            float size = 4.0f / _zoom;
            using (SolidBrush brush = new SolidBrush(Color.Red))
            {
                g.FillEllipse(brush, x - size, y - size, size * 2, size * 2);
            }
        }

        private void DrawDistanceLabel(Graphics g, float x1, float y1, float x2, float y2, double distanceMM)
        {
            float midX = (x1 + x2) / 2;
            float midY = (y1 + y2) / 2;

            string text = $"{distanceMM:F4} mm";
            Font font = new Font("Microsoft YaHei", 10.0f / _zoom);
            SizeF textSize = g.MeasureString(text, font);

            float padding = 4.0f / _zoom;
            RectangleF bgRect = new RectangleF(
                midX - textSize.Width / 2 - padding,
                midY - textSize.Height / 2 - padding,
                textSize.Width + padding * 2,
                textSize.Height + padding * 2);

            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
            {
                g.FillRectangle(bgBrush, bgRect);
            }

            using (Pen borderPen = new Pen(Color.Gray, 1.0f / _zoom))
            {
                g.DrawRectangle(borderPen, bgRect.X, bgRect.Y, bgRect.Width, bgRect.Height);
            }

            using (SolidBrush textBrush = new SolidBrush(Color.Black))
            {
                g.DrawString(text, font, textBrush, midX - textSize.Width / 2, midY - textSize.Height / 2);
            }
        }

        private void DrawRectangle(Graphics g, DrawingRectangle rect)
        {
            Color borderColor = rect.IsSelected ? Color.Orange : rect.BorderColor;
            float lineWidth = rect.IsSelected ? 3.0f : 2.0f;
            lineWidth /= _zoom;

            using (Pen pen = new Pen(borderColor, lineWidth))
            {
                g.DrawRectangle(pen, rect.Location.X, rect.Location.Y, rect.Width, rect.Height);
            }

            if (rect.IsSelected)
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(50, 255, 165, 0)))
                {
                    g.FillRectangle(brush, rect.Location.X, rect.Location.Y, rect.Width, rect.Height);
                }
            }

            DrawRectangleLabels(g, rect);
        }

        private void DrawRectangleLabels(Graphics g, DrawingRectangle rect)
        {
            Font font = new Font("Microsoft YaHei", 9.0f / _zoom);
            float padding = 3.0f / _zoom;

            string widthText = $"宽: {rect.WidthMM:F4} mm";
            string heightText = $"高: {rect.HeightMM:F4} mm";
            string diagonalText = $"对角线: {rect.DiagonalMM:F4} mm";

            SizeF widthSize = g.MeasureString(widthText, font);
            SizeF heightSize = g.MeasureString(heightText, font);
            SizeF diagonalSize = g.MeasureString(diagonalText, font);

            float maxWidth = Math.Max(widthSize.Width, Math.Max(heightSize.Width, diagonalSize.Width));
            float lineHeight = Math.Max(widthSize.Height, Math.Max(heightSize.Height, diagonalSize.Height));
            float labelWidth = maxWidth + padding * 2;
            float labelHeight = lineHeight * 3 + padding * 3;

            float labelX, labelY;
            bool drawOutside = rect.Width < labelWidth + 10 || rect.Height < labelHeight + 10;

            if (drawOutside)
            {
                labelX = rect.Location.X + rect.Width / 2 - maxWidth / 2;
                labelY = rect.Location.Y - labelHeight - 5;
            }
            else
            {
                labelX = rect.Location.X + rect.Width / 2 - maxWidth / 2;
                labelY = rect.Location.Y + rect.Height / 2 - lineHeight * 1.5f;
            }

            RectangleF bgRect = new RectangleF(
                labelX - padding,
                labelY - padding,
                labelWidth,
                labelHeight);

            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
            {
                g.FillRectangle(bgBrush, bgRect);
            }

            using (Pen borderPen = new Pen(Color.Gray, 1.0f / _zoom))
            {
                g.DrawRectangle(borderPen, bgRect.X, bgRect.Y, bgRect.Width, bgRect.Height);
            }

            using (SolidBrush textBrush = new SolidBrush(Color.Black))
            {
                g.DrawString(widthText, font, textBrush, labelX, labelY);
                g.DrawString(heightText, font, textBrush, labelX, labelY + lineHeight);
                g.DrawString(diagonalText, font, textBrush, labelX, labelY + lineHeight * 2);
            }
        }

        private void DrawPreviewRectangle(Graphics g)
        {
            float x = Math.Min(_drawStartPoint.X, _currentMousePoint.X);
            float y = Math.Min(_drawStartPoint.Y, _currentMousePoint.Y);
            float w = Math.Abs(_currentMousePoint.X - _drawStartPoint.X);
            float h = Math.Abs(_currentMousePoint.Y - _drawStartPoint.Y);

            if (w < 1 || h < 1) return;

            using (Pen pen = new Pen(Color.Green, 2.0f / _zoom))
            {
                pen.DashStyle = DashStyle.Dash;
                g.DrawRectangle(pen, x, y, w, h);
            }

            double widthMM = _converter.PixelsToMMX(w);
            double heightMM = _converter.PixelsToMMY(h);
            double diagonalMM = Math.Sqrt(widthMM * widthMM + heightMM * heightMM);

            Font font = new Font("Microsoft YaHei", 9.0f / _zoom);
            string text = $"{widthMM:F4} x {heightMM:F4} mm\n对角线: {diagonalMM:F4} mm";
            SizeF textSize = g.MeasureString(text, font);

            float labelX = x + w / 2 - textSize.Width / 2;
            float labelY = y + h / 2 - textSize.Height / 2;

            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(180, 255, 255, 255)))
            {
                g.FillRectangle(bgBrush, labelX - 4, labelY - 4, textSize.Width + 8, textSize.Height + 8);
            }

            using (SolidBrush textBrush = new SolidBrush(Color.Black))
            {
                g.DrawString(text, font, textBrush, labelX, labelY);
            }
        }

        private void DrawPreviewLine(Graphics g)
        {
            using (Pen pen = new Pen(Color.Blue, 2.0f / _zoom))
            {
                pen.DashStyle = DashStyle.Dash;
                g.DrawLine(pen, _lineStartPoint.X, _lineStartPoint.Y, 
                    _currentMousePoint.X, _currentMousePoint.Y);
            }

            double distance = _converter.CalculateDistanceMM(
                _lineStartPoint.X, _lineStartPoint.Y, 
                _currentMousePoint.X, _currentMousePoint.Y);

            Font font = new Font("Microsoft YaHei", 10.0f / _zoom);
            string text = $"{distance:F4} mm";
            SizeF textSize = g.MeasureString(text, font);

            float midX = (_lineStartPoint.X + _currentMousePoint.X) / 2;
            float midY = (_lineStartPoint.Y + _currentMousePoint.Y) / 2;

            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(180, 255, 255, 255)))
            {
                g.FillRectangle(bgBrush, midX - textSize.Width / 2 - 4, midY - textSize.Height / 2 - 4, 
                    textSize.Width + 8, textSize.Height + 8);
            }

            using (SolidBrush textBrush = new SolidBrush(Color.Black))
            {
                g.DrawString(text, font, textBrush, midX - textSize.Width / 2, midY - textSize.Height / 2);
            }
        }

        private PointF ScreenToImagePoint(Point screenPoint)
        {
            return new PointF(
                (screenPoint.X - _offset.X) / _zoom,
                (screenPoint.Y - _offset.Y) / _zoom);
        }

        private void DrawingPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (_image == null) return;

            PointF imagePoint = ScreenToImagePoint(e.Location);

            if (e.Button == MouseButtons.Left)
            {
                if (_drawMode == DrawMode.Line)
                {
                    if (!_hasLineStart)
                    {
                        _lineStartPoint = new DrawingPoint(imagePoint.X, imagePoint.Y);
                        _hasLineStart = true;
                        _isLineDrawing = true;
                        MainForm? form = this.FindForm() as MainForm;
                        form?.UpdateStatus($"起点: ({imagePoint.X:F1}, {imagePoint.Y:F1}) - 点击下一个点");
                    }
                    else
                    {
                        DrawingPoint endPoint = new DrawingPoint(imagePoint.X, imagePoint.Y);
                        LineSegment line = _drawingService.CreateLineSegment(_lineStartPoint!, endPoint);
                        _drawingService.AddLineSegment(line);
                        _lineStartPoint = endPoint;
                        MainForm? form = this.FindForm() as MainForm;
                        form?.UpdateStatus($"线段长度: {line.DistanceMM:F4} mm - 继续点击或右键结束");
                        this.Invalidate();
                    }
                }
                else if (_drawMode == DrawMode.Rectangle)
                {
                    _isDrawing = true;
                    _drawStartPoint = imagePoint;
                    _currentMousePoint = imagePoint;
                }
                else
                {
                    DrawingRectangle? hitRect = _drawingService.HitTestRectangle(imagePoint.X, imagePoint.Y);
                    if (hitRect != null)
                    {
                        _drawingService.DeselectAllRectangles();
                        hitRect.IsSelected = true;
                        _selectedRect = hitRect;
                        _isDragging = true;
                        _dragStartPoint = imagePoint;
                        _rectOriginalLocation = new PointF(hitRect.Location.X, hitRect.Location.Y);
                        MainForm? form = this.FindForm() as MainForm;
                        form?.UpdateStatus("已选中矩形 - 拖动鼠标移动位置");
                        this.Invalidate();
                    }
                    else
                    {
                        _drawingService.DeselectAllRectangles();
                        this.Invalidate();
                    }
                }
            }
        }

        private void DrawingPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_image == null) return;

            PointF imagePoint = ScreenToImagePoint(e.Location);
            _currentMousePoint = imagePoint;

            if (_isDragging && _selectedRect != null)
            {
                float deltaX = imagePoint.X - _dragStartPoint.X;
                float deltaY = imagePoint.Y - _dragStartPoint.Y;

                if (Math.Abs(deltaX) > 0.5f || Math.Abs(deltaY) > 0.5f)
                {
                    _drawingService.MoveRectangle(_selectedRect, deltaX, deltaY);
                    _dragStartPoint = imagePoint;
                    this.Invalidate();
                }
            }
            else if (_isDrawing && _drawMode == DrawMode.Rectangle)
            {
                this.Invalidate();
            }
            else if (_isLineDrawing && _hasLineStart)
            {
                this.Invalidate();
            }
        }

        private void DrawingPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (_image == null) return;

            if (e.Button == MouseButtons.Left)
            {
                if (_isDragging && _selectedRect != null)
                {
                    _isDragging = false;
                    MainForm? form = this.FindForm() as MainForm;
                    form?.UpdateStatus($"矩形已移动 - 宽: {_selectedRect.WidthMM:F4} mm, 高: {_selectedRect.HeightMM:F4} mm");
                }
                else if (_isDrawing && _drawMode == DrawMode.Rectangle)
                {
                    PointF imagePoint = ScreenToImagePoint(e.Location);
                    float x = Math.Min(_drawStartPoint.X, imagePoint.X);
                    float y = Math.Min(_drawStartPoint.Y, imagePoint.Y);
                    float w = Math.Abs(imagePoint.X - _drawStartPoint.X);
                    float h = Math.Abs(imagePoint.Y - _drawStartPoint.Y);

                    if (w > 5 && h > 5)
                    {
                        DrawingPoint location = new DrawingPoint(x, y);
                        DrawingRectangle rect = _drawingService.CreateRectangle(location, w, h);
                        _drawingService.AddRectangle(rect);
                        MainForm? form = this.FindForm() as MainForm;
                        form?.UpdateStatus($"矩形已绘制 - 宽: {rect.WidthMM:F4} mm, 高: {rect.HeightMM:F4} mm");
                    }

                    _isDrawing = false;
                    this.Invalidate();
                }
                else if (_drawMode == DrawMode.None)
                {
                    PointF imagePoint = ScreenToImagePoint(e.Location);
                    DrawingRectangle? hitRect = _drawingService.HitTestRectangle(imagePoint.X, imagePoint.Y);
                    if (hitRect == null)
                    {
                        _drawingService.DeselectAllRectangles();
                        this.Invalidate();
                    }
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (_isLineDrawing)
                {
                    _isLineDrawing = false;
                    _hasLineStart = false;
                    MainForm? form = this.FindForm() as MainForm;
                    form?.UpdateStatus("线段绘制完成 - 选择绘制模式继续");
                    this.Invalidate();
                }
            }
        }
    }
}
