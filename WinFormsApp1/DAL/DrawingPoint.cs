namespace WinFormsApp1.DAL
{
    public class DrawingPoint
    {
        public float X { get; set; }
        public float Y { get; set; }

        public DrawingPoint() { }

        public DrawingPoint(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}
