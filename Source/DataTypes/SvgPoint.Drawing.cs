using SkiaSharp;

namespace Svg
{
    public partial struct SvgPoint
    {
        public SKPoint ToDeviceValue(ISvgRenderer renderer, SvgElement owner)
        {
            return SvgUnit.GetDevicePoint(this.X, this.Y, renderer, owner);
        }
    }
}
