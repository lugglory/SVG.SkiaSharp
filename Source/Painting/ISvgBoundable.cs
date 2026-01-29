using SkiaSharp;

namespace Svg
{
    public interface ISvgBoundable
    {
        SKPoint Location
        {
            get;
        }

        SKSize Size
        {
            get;
        }

        SKRect Bounds
        {
            get;
        }
    }
}
