#if !NO_SDC
using SkiaSharp;

namespace Svg.Transforms
{
    public sealed partial class SvgTranslate : SvgTransform
    {
        public override SKMatrix Matrix
        {
            get
            {
                return SKMatrix.CreateTranslation(this.X, this.Y);
            }
        }
    }
}
#endif