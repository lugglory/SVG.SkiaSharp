#if !NO_SDC
using SkiaSharp;

namespace Svg
{
    public partial class SvgDeferredPaintServer : SvgPaintServer
    {
        public override SKPaint GetPaint(SvgVisualElement styleOwner, ISvgRenderer renderer, float opacity, bool forStroke = false)
        {
            EnsureServer(styleOwner);
            return _concreteServer?.GetPaint(styleOwner, renderer, opacity, forStroke) ?? _fallbackServer?.GetPaint(styleOwner, renderer, opacity, forStroke) ?? NotSet?.GetPaint(styleOwner, renderer, opacity, forStroke);
        }
    }
}
#endif