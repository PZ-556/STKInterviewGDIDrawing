using System.Drawing;
using WinFormsApp1.DAL;

namespace WinFormsApp1.BLL
{
    public class DrawingService
    {
        private DrawingRepository _repository;
        private MeasurementService _measurementService;

        public DrawingService(DrawingRepository repository, MeasurementService measurementService)
        {
            _repository = repository;
            _measurementService = measurementService;
        }

        public LineSegment CreateLineSegment(DrawingPoint start, DrawingPoint end)
        {
            double distance = _measurementService.CalculateDistance(start, end);
            return new LineSegment(start, end, distance);
        }

        public DrawingRectangle CreateRectangle(DrawingPoint location, float width, float height)
        {
            double widthMM = _measurementService.CalculateRectangleWidthMM(width);
            double heightMM = _measurementService.CalculateRectangleHeightMM(height);
            double diagonalMM = _measurementService.CalculateDiagonalMM(width, height);
            return new DrawingRectangle(location, width, height, widthMM, heightMM, diagonalMM);
        }

        public void AddLineSegment(LineSegment line)
        {
            _repository.AddLineSegment(line);
        }

        public void AddRectangle(DrawingRectangle rect)
        {
            _repository.AddRectangle(rect);
        }

        public DrawingRectangle? HitTestRectangle(float x, float y)
        {
            for (int i = _repository.Rectangles.Count - 1; i >= 0; i--)
            {
                if (_repository.Rectangles[i].ContainsPoint(x, y))
                {
                    return _repository.Rectangles[i];
                }
            }
            return null;
        }

        public void DeselectAllRectangles()
        {
            _repository.DeselectAllRectangles();
        }

        public void MoveRectangle(DrawingRectangle rect, float deltaX, float deltaY)
        {
            rect.Location.X += deltaX;
            rect.Location.Y += deltaY;
        }
    }
}
