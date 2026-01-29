using System;
using SkiaSharp;

namespace Svg.Pathing
{
    public abstract partial class SvgPathSegment
    {
        protected static SKPoint Reflect(SKPoint point, SKPoint mirror)
        {
            var dx = Math.Abs(mirror.X - point.X);
            var dy = Math.Abs(mirror.Y - point.Y);

            var x = mirror.X + (mirror.X >= point.X ? dx : -dx);
            var y = mirror.Y + (mirror.Y >= point.Y ? dy : -dy);

            return new SKPoint(x, y);
        }

        protected static SKPoint ToAbsolute(SKPoint point, bool isRelative, SKPoint start)
        {
            if (float.IsNaN(point.X))
                point.X = start.X;
            else if (isRelative)
                point.X += start.X;

            if (float.IsNaN(point.Y))
                point.Y = start.Y;
            else if (isRelative)
                point.Y += start.Y;

            return point;
        }

        public abstract SKPoint AddToPath(SKPath graphicsPath, SKPoint start, SvgPathSegmentList parent);
    }
}
