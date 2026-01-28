using SkiaSharp;

namespace Svg
{
    internal class GenericBoundable : ISvgBoundable
    {
        private SKRect _rect;

        public GenericBoundable(SKRect rect)
        {
            _rect = rect;
        }
        public GenericBoundable(float x, float y, float width, float height)
        {
            _rect = new SKRect(x, y, x + width, y + height);
        }

        public SKPoint Location
        {
            get { return new SKPoint(_rect.Left, _rect.Top); }
        }

        public SKSize Size
        {
            get { return new SKSize(_rect.Width, _rect.Height); }
        }

        public SKRect Bounds
        {
            get { return _rect; }
        }
    }
}
