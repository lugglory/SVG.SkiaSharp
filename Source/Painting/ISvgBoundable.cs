using SkiaSharp;

namespace Svg
{
    public interface ISvgBoundable
    {
#if !NO_SDC
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
#endif
    }
}
