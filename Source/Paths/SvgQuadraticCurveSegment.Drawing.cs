using SkiaSharp;

namespace Svg.Pathing
{
    public sealed partial class SvgQuadraticCurveSegment : SvgPathSegment
    {
        public override SKPoint AddToPath(SKPath graphicsPath, SKPoint start, SvgPathSegmentList parent)
        {
            var controlPoint = ControlPoint;
            if (float.IsNaN(controlPoint.X) || float.IsNaN(controlPoint.Y))
            {
                var prev = parent.IndexOf(this) - 1;
                if (prev >= 0 && parent[prev] is SvgQuadraticCurveSegment)
                {
                    // For Skia, we can get the control point of the last quadratic verb
                    // but it's easier to just use the logic from GDI+ if we want to be exact,
                    // OR better: in SVG, T follows Q. The control point is the reflection
                    // of the PREVIOUS Q's control point.
                    
                    // We need to find the control point used in the last QuadTo.
                    var pts = graphicsPath.Points;
                    if (pts.Length >= 2)
                    {
                        var lastCp = pts[pts.Length - 2];
                        controlPoint = Reflect(lastCp, start);
                    }
                    else
                    {
                        controlPoint = start;
                    }
                }
                else
                    controlPoint = start;
            }
            else
                controlPoint = ToAbsolute(controlPoint, IsRelative, start);

            var end = ToAbsolute(End, IsRelative, start);
            graphicsPath.QuadTo(controlPoint, end);
            return end;
        }
    }
}
