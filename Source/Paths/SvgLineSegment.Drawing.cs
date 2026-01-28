#if !NO_SDC
using SkiaSharp;

namespace Svg.Pathing
{
    public sealed partial class SvgLineSegment : SvgPathSegment
    {
        public override SKPoint AddToPath(SKPath graphicsPath, SKPoint start, SvgPathSegmentList parent)
        {
            var end = ToAbsolute(End, IsRelative, start);
            graphicsPath.LineTo(end);
            return end;
        }
    }
}
#endif