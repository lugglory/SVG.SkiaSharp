#if !NO_SDC
using SkiaSharp;

namespace Svg.Transforms
{
    public sealed partial class SvgMatrix : SvgTransform
    {
        public override SKMatrix Matrix
        {
            get
            {
                // SKMatrix(scaleX, skewX, transX, skewY, scaleY, transY, pers0, pers1, pers2)
                return new SKMatrix(
                    this.Points[0], this.Points[2], this.Points[4],
                    this.Points[1], this.Points[3], this.Points[5],
                    0, 0, 1);
            }
        }
    }
}
#endif