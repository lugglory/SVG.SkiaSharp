using System;
using SkiaSharp;

namespace Svg.Pathing
{
    public abstract partial class SvgPathSegment
    {
        protected static readonly SKPoint NaN = new SKPoint(float.NaN, float.NaN);

        public bool IsRelative { get; set; }

        public SKPoint End { get; set; }

        protected SvgPathSegment(bool isRelative)
        {
            IsRelative = isRelative;
        }

        protected SvgPathSegment(bool isRelative, SKPoint end)
            : this(isRelative)
        {
            End = end;
        }

        public SvgPathSegment Clone()
        {
            return MemberwiseClone() as SvgPathSegment;
        }

        [Obsolete("Will be removed.")]
        public SKPoint Start { get; set; }
    }
}