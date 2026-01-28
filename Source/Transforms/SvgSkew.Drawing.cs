#if !NO_SDC
using System;
using SkiaSharp;

namespace Svg.Transforms
{
    public sealed partial class SvgSkew : SvgTransform
    {
        public override SKMatrix Matrix
        {
            get
            {
                var tanX = (float)Math.Tan(this.AngleX / 180.0 * Math.PI);
                var tanY = (float)Math.Tan(this.AngleY / 180.0 * Math.PI);
                return SKMatrix.CreateSkew(tanX, tanY);
            }
        }
    }
}
#endif