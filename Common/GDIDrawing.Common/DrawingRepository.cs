using System.Collections.Generic;
using GDIDrawing.Model;

namespace GDIDrawing.Common
{
    public class DrawingRepository
    {
        private List<LineSegment> _lineSegments;
        private List<DrawingRectangle> _rectangles;

        public List<LineSegment> LineSegments => _lineSegments;
        public List<DrawingRectangle> Rectangles => _rectangles;

        public DrawingRepository()
        {
            _lineSegments = new List<LineSegment>();
            _rectangles = new List<DrawingRectangle>();
        }

        public void AddLineSegment(LineSegment line)
        {
            _lineSegments.Add(line);
        }

        public void AddRectangle(DrawingRectangle rect)
        {
            _rectangles.Add(rect);
        }

        public void ClearAll()
        {
            _lineSegments.Clear();
            _rectangles.Clear();
        }

        public void ClearLines()
        {
            _lineSegments.Clear();
        }

        public void ClearRectangles()
        {
            _rectangles.Clear();
        }

        public DrawingRectangle GetSelectedRectangle()
        {
            return _rectangles.Find(r => r.IsSelected);
        }

        public void DeselectAllRectangles()
        {
            foreach (var rect in _rectangles)
            {
                rect.IsSelected = false;
            }
        }
    }
}
