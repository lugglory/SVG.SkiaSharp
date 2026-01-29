using SkiaSharp;

namespace Svg
{
    public partial class SvgSymbol : SvgVisualElement
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

        protected internal override bool PushTransforms(ISvgRenderer renderer)
        {
            if (!base.PushTransforms(renderer))
                return false;
            ViewBox.AddViewBoxTransform(AspectRatio, renderer, null);
            return true;
        }

        protected override void Render(ISvgRenderer renderer)
        {
            if (_parent is SvgUse) base.Render(renderer);
        }
    }
}
