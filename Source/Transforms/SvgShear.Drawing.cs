using SkiaSharp;

namespace Svg.Transforms
{
    public partial class SvgShear
    {
        public override SKMatrix Matrix
        {
            get
            {
                return SKMatrix.CreateSkew(this.X, this.Y);
            }
        }
    }
}
