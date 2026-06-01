using WinFormsApp1.DAL;

namespace WinFormsApp1.BLL
{
    public class MeasurementService
    {
        private CoordinateConverter _converter;

        public MeasurementService(CoordinateConverter converter)
        {
            _converter = converter;
        }

        public double CalculateDistance(DrawingPoint p1, DrawingPoint p2)
        {
            return _converter.CalculateDistanceMM(p1.X, p1.Y, p2.X, p2.Y);
        }

        public double CalculateRectangleWidthMM(float widthPx)
        {
            return _converter.PixelsToMMX(widthPx);
        }

        public double CalculateRectangleHeightMM(float heightPx)
        {
            return _converter.PixelsToMMY(heightPx);
        }

        public double CalculateDiagonalMM(float widthPx, float heightPx)
        {
            double widthMM = CalculateRectangleWidthMM(widthPx);
            double heightMM = CalculateRectangleHeightMM(heightPx);
            return Math.Sqrt(widthMM * widthMM + heightMM * heightMM);
        }
    }
}
