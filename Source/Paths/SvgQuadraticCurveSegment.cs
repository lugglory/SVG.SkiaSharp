using SkiaSharp;

namespace Svg.Pathing
{
    public sealed partial class SvgQuadraticCurveSegment : SvgPathSegment
    {
        public SKPoint ControlPoint { get; set; }

        public SvgQuadraticCurveSegment(bool isRelative, SKPoint controlPoint, SKPoint end)
            : base(isRelative, end)
        {
            ControlPoint = controlPoint;
        }

        public SvgQuadraticCurveSegment(bool isRelative, SKPoint end)
            : this(isRelative, NaN, end)
        {
        }

        public override string ToString()
        {
            if (float.IsNaN(ControlPoint.X) || float.IsNaN(ControlPoint.Y))
                return (IsRelative ? "t" : "T") + End.ToSvgString();
            else
                return (IsRelative ? "q" : "Q") + ControlPoint.ToSvgString() + " " + End.ToSvgString();
        }

        [System.Obsolete("Use new constructor.")]
        public SvgQuadraticCurveSegment(SKPoint start, SKPoint controlPoint, SKPoint end)
            : this(false, controlPoint, end)
        {
            Start = start;
        }
    }
}