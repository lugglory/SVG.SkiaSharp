using SkiaSharp;

namespace Svg.Pathing
{
    public sealed partial class SvgArcSegment : SvgPathSegment
    {
        public override SKPoint AddToPath(SKPath graphicsPath, SKPoint start, SvgPathSegmentList parent)
        {
            var end = ToAbsolute(End, IsRelative, start);

            if (start == end)
            {
                return end;
            }

            if (RadiusX == 0.0f && RadiusY == 0.0f)
            {
                graphicsPath.LineTo(end);
                return end;
            }

            // Skia ArcTo(rx, ry, xAxisRotate, largeArc, sweep, x, y)
            graphicsPath.ArcTo(
                RadiusX, 
                RadiusY, 
                Angle, 
                Size == SvgArcSize.Large ? SKPathArcSize.Large : SKPathArcSize.Small, 
                Sweep == SvgArcSweep.Positive ? SKPathDirection.Clockwise : SKPathDirection.CounterClockwise, 
                end.X, 
                end.Y
            );

            return end;
        }
    }
}
