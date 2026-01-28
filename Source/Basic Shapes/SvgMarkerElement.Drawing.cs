#if !NO_SDC
using System.Linq;
using SkiaSharp;
using Svg.ExtensionMethods;

namespace Svg
{
    public abstract partial class SvgMarkerElement : SvgPathBasedElement
    {
        protected internal override bool RenderStroke(ISvgRenderer renderer)
        {
            var result = base.RenderStroke(renderer);
            var path = Path(renderer);
            if (path == null || path.PointCount == 0) return result;

            var points = path.Points;
            var pathLength = points.Length;

            var markerStart = MarkerStart.ReplaceWithNullIfNone();
            if (markerStart != null)
            {
                var refPoint1 = points[0];
                var index = 1;
                while (index < pathLength && points[index] == refPoint1)
                {
                    ++index;
                }
                if (index < pathLength)
                {
                    var refPoint2 = points[index];
                    var marker = OwnerDocument.GetElementById<SvgMarker>(markerStart.ToString());
                    if (marker != null) marker.RenderMarker(renderer, this, refPoint1, refPoint1, refPoint2, true);
                }
            }

            var markerMid = MarkerMid.ReplaceWithNullIfNone();
            if (markerMid != null)
            {
                // TODO: Implement marker mid-points using SkiaSharp-compatible path iteration
            }

            var markerEnd = MarkerEnd.ReplaceWithNullIfNone();
            if (markerEnd != null)
            {
                var index = pathLength - 1;
                var refPoint1 = points[index];
                --index;
                while (index > 0 && points[index] == refPoint1)
                {
                    --index;
                }
                var refPoint2 = points[index];
                var marker = OwnerDocument.GetElementById<SvgMarker>(markerEnd.ToString());
                if (marker != null) marker.RenderMarker(renderer, this, refPoint1, refPoint2, points[pathLength - 1], false);
            }

            return result;
        }
    }
}
#endif