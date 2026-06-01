namespace GDIDrawing.Common
{
    public class CoordinateConverter
    {
        private float _imageWidthPx;
        private float _imageHeightPx;
        private float _imageWidthMM;
        private float _imageHeightMM;
        private float _pixelsPerMMX;
        private float _pixelsPerMMY;

        public float ImageWidthPx => _imageWidthPx;
        public float ImageHeightPx => _imageHeightPx;
        public float ImageWidthMM => _imageWidthMM;
        public float ImageHeightMM => _imageHeightMM;
        public float PixelsPerMMX => _pixelsPerMMX;
        public float PixelsPerMMY => _pixelsPerMMY;

        public CoordinateConverter(float imageWidthPx, float imageHeightPx, float imageWidthMM, float imageHeightMM)
        {
            _imageWidthPx = imageWidthPx;
            _imageHeightPx = imageHeightPx;
            _imageWidthMM = imageWidthMM;
            _imageHeightMM = imageHeightMM;
            _pixelsPerMMX = imageWidthPx / imageWidthMM;
            _pixelsPerMMY = imageHeightPx / imageHeightMM;
        }

        public double PixelsToMMX(float pixels)
        {
            return pixels / _pixelsPerMMX;
        }

        public double PixelsToMMY(float pixels)
        {
            return pixels / _pixelsPerMMY;
        }

        public float MMToPixelsX(double mm)
        {
            return (float)(mm * _pixelsPerMMX);
        }

        public float MMToPixelsY(double mm)
        {
            return (float)(mm * _pixelsPerMMY);
        }

        public double CalculateDistanceMM(float x1, float y1, float x2, float y2)
        {
            double dx = PixelsToMMX(x2 - x1);
            double dy = PixelsToMMY(y2 - y1);
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
