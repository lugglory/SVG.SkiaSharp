#if !NO_SDC
using SkiaSharp;

namespace Svg.Pathing
{
    public sealed partial class SvgMoveToSegment : SvgPathSegment
    {
        public override SKPoint AddToPath(SKPath graphicsPath, SKPoint start, SvgPathSegmentList parent)
        {
            var end = ToAbsolute(End, IsRelative, start);
            graphicsPath.MoveTo(end);
            return end;
        }
    }
}
#endif