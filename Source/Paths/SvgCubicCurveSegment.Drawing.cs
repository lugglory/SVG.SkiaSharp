using SkiaSharp;

namespace Svg.Pathing
{
    public sealed partial class SvgCubicCurveSegment : SvgPathSegment
    {
        public override SKPoint AddToPath(SKPath graphicsPath, SKPoint start, SvgPathSegmentList parent)
        {
            var firstControlPoint = FirstControlPoint;
            if (float.IsNaN(firstControlPoint.X) || float.IsNaN(firstControlPoint.Y))
            {
                var prev = parent.IndexOf(this) - 1;
                if (prev >= 0 && parent[prev] is SvgCubicCurveSegment)
                {
                    // In SkiaSharp, we can't directly access path points by index as easily as in some other libraries.
                    // We can track it or look at the last verb points.
                    // But usually SVG.NET stores the last segment's control point if needed.
                    // Actually, the easiest is to get the last point of the path.
                    // But we need the SECOND control point of the PREVIOUS segment.
                    // Let's assume we can get it from the previous segment object.
                    var prevSeg = parent[prev] as SvgCubicCurveSegment;
                    var prevEnd = (prev == 0) ? SKPoint.Empty : ToAbsolute(parent[prev-1].End, parent[prev-1].IsRelative, SKPoint.Empty); // This is getting complex.
                    // Simplify: Reflect previous segment's SecondControlPoint across start.
                    var prevCp2 = ToAbsolute(prevSeg.SecondControlPoint, prevSeg.IsRelative, (prev == 0) ? SKPoint.Empty : ToAbsolute(parent[prev-1].End, parent[prev-1].IsRelative, SKPoint.Empty));
                    // Wait, SVG.NET implementation was using graphicsPath.PathPoints[graphicsPath.PointCount - 2].
                    // In Skia, we can get the points of the last verb.
                    var pts = graphicsPath.Points;
                    if (pts.Length >= 2)
                    {
                        var lastCp = pts[pts.Length - 2];
                        firstControlPoint = Reflect(lastCp, start);
                    }
                    else
                    {
                        firstControlPoint = start;
                    }
                }
                else
                    firstControlPoint = start;
            }
            else
                firstControlPoint = ToAbsolute(firstControlPoint, IsRelative, start);

            var end = ToAbsolute(End, IsRelative, start);
            var secondControlPoint = ToAbsolute(SecondControlPoint, IsRelative, start);
            graphicsPath.CubicTo(firstControlPoint, secondControlPoint, end);
            return end;
        }
    }
}
