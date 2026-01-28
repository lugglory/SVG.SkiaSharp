using SkiaSharp;

namespace Svg.Pathing
{
    public sealed partial class SvgCubicCurveSegment : SvgPathSegment
    {
        public SKPoint FirstControlPoint { get; set; }
        public SKPoint SecondControlPoint { get; set; }

        public SvgCubicCurveSegment(bool isRelative, SKPoint firstControlPoint, SKPoint secondControlPoint, SKPoint end)
            : base(isRelative, end)
        {
            FirstControlPoint = firstControlPoint;
            SecondControlPoint = secondControlPoint;
        }

        public SvgCubicCurveSegment(bool isRelative, SKPoint secondControlPoint, SKPoint end)
            : this(isRelative, NaN, secondControlPoint, end)
        {
        }

        public override string ToString()
        {
            if (float.IsNaN(FirstControlPoint.X) || float.IsNaN(FirstControlPoint.Y))
                return (IsRelative ? "s" : "S") + SecondControlPoint.ToSvgString() + " " + End.ToSvgString();
            else
                return (IsRelative ? "c" : "C") + FirstControlPoint.ToSvgString() + " " + SecondControlPoint.ToSvgString() + " " + End.ToSvgString();
        }

        [System.Obsolete("Use new constructor.")]
        public SvgCubicCurveSegment(SKPoint start, SKPoint firstControlPoint, SKPoint secondControlPoint, SKPoint end)
            : this(false, firstControlPoint, secondControlPoint, end)
        {
            Start = start;
        }
    }
}