#if !NO_SDC
using SkiaSharp;

namespace Svg.Pathing
{
    public sealed partial class SvgClosePathSegment : SvgPathSegment
    {
        public override SKPoint AddToPath(SKPath graphicsPath, SKPoint start, SvgPathSegmentList parent)
        {
            graphicsPath.Close();

            if (graphicsPath.PointCount == 0)
                return start;

            // Fallback: return the first point of the path.
            // In many cases this is the start of the current subpath.
            return graphicsPath.Points[0];
        }
    }
}
#endif