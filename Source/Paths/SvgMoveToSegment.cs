using SkiaSharp;

namespace Svg.Pathing
{
    public sealed partial class SvgMoveToSegment : SvgPathSegment
    {
        public SvgMoveToSegment(bool isRelative, SKPoint moveTo)
            : base(isRelative, moveTo)
        {
        }

        public override string ToString()
        {
            return (IsRelative ? "m" : "M") + End.ToSvgString();
        }

        [System.Obsolete("Use new constructor.")]
        public SvgMoveToSegment(SKPoint moveTo)
            : this(false, moveTo)
        {
            Start = moveTo;
        }
    }
}