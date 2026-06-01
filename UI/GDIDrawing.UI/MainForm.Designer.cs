using System.Drawing;

namespace GDIDrawing.UI
{
    partial class MainForm
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

        private void InitializeComponent()
        {
            panelToolbar = new Panel();
            btnLoadImage = CreateButton("加载图片", 10, 8, 100);
            btnLineMode = CreateButton("线段绘制", 120, 8, 100);
            btnRectMode = CreateButton("矩形绘制", 230, 8, 100);
            btnExitMode = CreateButton("退出绘制", 340, 8, 100);
            btnClearAll = CreateButton("清除全部", 450, 8, 100);
            btnZoomOut = CreateButton("缩小 (-)", 570, 8, 80);
            btnZoomIn = CreateButton("放大 (+)", 660, 8, 80);
            lblZoomLevel = new Label();
            lblStatus = new Label();
            drawingPanel = new DrawingPanel(_repository, _drawingService, _converter);
            panelToolbar.SuspendLayout();
            SuspendLayout();
            // 
            // panelToolbar
            // 
            panelToolbar.BackColor = Color.FromArgb(240, 240, 240);
            panelToolbar.Controls.Add(btnLoadImage);
            panelToolbar.Controls.Add(btnLineMode);
            panelToolbar.Controls.Add(btnRectMode);
            panelToolbar.Controls.Add(btnExitMode);
            panelToolbar.Controls.Add(btnClearAll);
            panelToolbar.Controls.Add(btnZoomIn);
            panelToolbar.Controls.Add(btnZoomOut);
            panelToolbar.Controls.Add(lblZoomLevel);
            panelToolbar.Controls.Add(lblStatus);
            panelToolbar.Dock = DockStyle.Top;
            panelToolbar.Height = 70;
            panelToolbar.Name = "panelToolbar";
            // 
            // btnLoadImage
            // 
            btnLoadImage.Name = "btnLoadImage";
            btnLoadImage.Click += BtnLoadImage_Click;
            // 
            // btnLineMode
            // 
            btnLineMode.Enabled = false;
            btnLineMode.Name = "btnLineMode";
            btnLineMode.Click += BtnLineMode_Click;
            // 
            // btnRectMode
            // 
            btnRectMode.Enabled = false;
            btnRectMode.Name = "btnRectMode";
            btnRectMode.Click += BtnRectMode_Click;
            // 
            // btnExitMode
            // 
            btnExitMode.Name = "btnExitMode";
            btnExitMode.Click += BtnExitMode_Click;
            // 
            // btnClearAll
            // 
            btnClearAll.Enabled = false;
            btnClearAll.Name = "btnClearAll";
            btnClearAll.Click += BtnClearAll_Click;
            // 
            // btnZoomOut
            // 
            btnZoomOut.Enabled = false;
            btnZoomOut.Name = "btnZoomOut";
            btnZoomOut.Click += BtnZoomOut_Click;
            // 
            // btnZoomIn
            // 
            btnZoomIn.Enabled = false;
            btnZoomIn.Name = "btnZoomIn";
            btnZoomIn.Click += BtnZoomIn_Click;
            // 
            // lblZoomLevel
            // 
            lblZoomLevel.AutoSize = true;
            lblZoomLevel.Font = new Font("Microsoft YaHei", 10F, FontStyle.Bold);
            lblZoomLevel.Location = new Point(750, 12);
            lblZoomLevel.Name = "lblZoomLevel";
            lblZoomLevel.Text = "100%";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Microsoft YaHei", 9F);
            lblStatus.Location = new Point(10, 42);
            lblStatus.Name = "lblStatus";
            lblStatus.Text = "就绪 - 请先加载图片";
            // 
            // drawingPanel
            // 
            drawingPanel.BackColor = Color.White;
            drawingPanel.Dock = DockStyle.Fill;
            drawingPanel.Name = "drawingPanel";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 800);
            Controls.Add(drawingPanel);
            Controls.Add(panelToolbar);
            MinimumSize = new Size(800, 600);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "GDI+ 绘图测量工具";
            panelToolbar.ResumeLayout(false);
            panelToolbar.PerformLayout();
            ResumeLayout(false);
            UpdateModeButtons();
        }

        private Button CreateButton(string text, int x, int y, int width)
        {
            return new Button
            {
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei", 9F),
                Location = new Point(x, y),
                Size = new Size(width, 30),
                Text = text
            };
        }
    }
}
