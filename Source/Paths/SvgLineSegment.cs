using SkiaSharp;

namespace Svg.Pathing
{
    public sealed partial class SvgLineSegment : SvgPathSegment
    {
        public SvgLineSegment(bool isRelative, SKPoint end)
            : base(isRelative, end)
        {
        }

        public override string ToString()
        {
            if (float.IsNaN(End.Y))
                return (IsRelative ? "h" : "H") + End.X.ToSvgString();
            else if (float.IsNaN(End.X))
                return (IsRelative ? "v" : "V") + End.Y.ToSvgString();
            else
                return (IsRelative ? "l" : "L") + End.ToSvgString();
        }

        [System.Obsolete("Use new constructor.")]
        public SvgLineSegment(SKPoint start, SKPoint end)
            : this(false, end)
        {
            Start = start;
        }
    }
}