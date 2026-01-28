#if !NO_SDC
using System;
using SkiaSharp;

namespace Svg
{
    public partial class SvgColourServer : SvgPaintServer
    {
        public override SKPaint GetPaint(SvgVisualElement styleOwner, ISvgRenderer renderer, float opacity, bool forStroke = false)
        {
            var paint = new SKPaint();
            paint.IsAntialias = true;

            // is none?
            if (this == None)
            {
                paint.Color = SKColors.Transparent;
                return paint;
            }

            // default fill color is black, default stroke color is none
            if (this == NotSet && forStroke)
            {
                paint.Color = SKColors.Transparent;
                return paint;
            }

            float finalOpacity = opacity * (this.Colour.Alpha / 255.0f);
            paint.Color = this.Colour.WithAlpha((byte)Math.Round(finalOpacity * 255));

            return paint;
        }
    }
}
#endif