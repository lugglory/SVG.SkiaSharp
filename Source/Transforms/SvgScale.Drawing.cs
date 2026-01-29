using SkiaSharp;

namespace Svg.Transforms
{
    public sealed partial class SvgScale : SvgTransform
    {
        public override SKMatrix Matrix
        {
            get
            {
                return SKMatrix.CreateScale(this.X, this.Y);
            }
        }
    }
}
