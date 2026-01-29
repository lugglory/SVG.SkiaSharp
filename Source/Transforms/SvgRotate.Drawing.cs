using SkiaSharp;

namespace Svg.Transforms
{
    public sealed partial class SvgRotate : SvgTransform
    {
        public override SKMatrix Matrix
        {
            get
            {
                return SKMatrix.CreateRotationDegrees(this.Angle, this.CenterX, this.CenterY);
            }
        }
    }
}
