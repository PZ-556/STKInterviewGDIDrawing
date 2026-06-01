using System.Drawing;

namespace GDIDrawing.Model
{
    public class LineSegment
    {
        public DrawingPoint StartPoint { get; set; }
        public DrawingPoint EndPoint { get; set; }
        public double DistanceMM { get; set; }
        public Color LineColor { get; set; }

        public LineSegment()
        {
            StartPoint = new DrawingPoint();
            EndPoint = new DrawingPoint();
            LineColor = Color.Blue;
        }

        public LineSegment(DrawingPoint start, DrawingPoint end, double distance)
        {
            StartPoint = start;
            EndPoint = end;
            DistanceMM = distance;
            LineColor = Color.Blue;
        }
    }
}
