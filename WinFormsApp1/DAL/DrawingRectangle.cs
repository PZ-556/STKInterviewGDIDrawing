using System.Drawing;

namespace WinFormsApp1.DAL
{
    public class DrawingRectangle
    {
        public DrawingPoint Location { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public double WidthMM { get; set; }
        public double HeightMM { get; set; }
        public double DiagonalMM { get; set; }
        public Color BorderColor { get; set; }
        public bool IsSelected { get; set; }

        public DrawingRectangle()
        {
            Location = new DrawingPoint();
            BorderColor = Color.Green;
            IsSelected = false;
        }

        public DrawingRectangle(DrawingPoint location, float width, float height, double widthMM, double heightMM, double diagonalMM)
        {
            Location = location;
            Width = width;
            Height = height;
            WidthMM = widthMM;
            HeightMM = heightMM;
            DiagonalMM = diagonalMM;
            BorderColor = Color.Green;
            IsSelected = false;
        }

        public RectangleF GetRectangleF()
        {
            return new RectangleF(Location.X, Location.Y, Width, Height);
        }

        public bool ContainsPoint(float x, float y)
        {
            return x >= Location.X && x <= Location.X + Width &&
                   y >= Location.Y && y <= Location.Y + Height;
        }
    }
}
