#if !NO_SDC
using SkiaSharp;

namespace Svg
{
    public partial class SvgForeignObject : SvgVisualElement
    {
        /// <summary>
        /// Gets the <see cref="SKPath"/> for this element.
        /// </summary>
        public override SKPath Path(ISvgRenderer renderer)
        {
            return GetPaths(this, renderer);
        }

        /// <summary>
        /// Gets the bounds of the element.
        /// </summary>
        public override SKRect Bounds
        {
            get
            {
                var r = SKRect.Empty;
                foreach (var c in this.Children)
                {
                    if (c is SvgVisualElement visualElement)
                    {
                        var childBounds = visualElement.Bounds;
                        if (!childBounds.IsEmpty)
                        {
                            if (r.IsEmpty)
                            {
                                r = childBounds;
                            }
                            else
                            {
                                r = SKRect.Union(r, childBounds);
                            }
                        }
                    }
                }
                return TransformedBounds(r);
            }
        }
    }
}
#endif