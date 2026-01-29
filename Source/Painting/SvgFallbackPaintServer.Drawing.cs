using System.Linq;
using SkiaSharp;

namespace Svg
{
    public partial class SvgFallbackPaintServer : SvgPaintServer
    {
        public override SKPaint GetPaint(SvgVisualElement styleOwner, ISvgRenderer renderer, float opacity, bool forStroke = false)
        {
            try
            {
                _primary.GetCallback = () => _fallbacks.FirstOrDefault();
                return _primary.GetPaint(styleOwner, renderer, opacity, forStroke);
            }
            finally
            {
                _primary.GetCallback = null;
            }
        }
    }
}
