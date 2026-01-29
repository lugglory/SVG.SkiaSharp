using SkiaSharp;

namespace Svg
{
    public abstract partial class SvgPaintServer : SvgElement
    {
        /// <summary>
        /// Renders the <see cref="SvgElement"/> and contents to the specified <see cref="ISvgRenderer"/> object.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> object to render to.</param>
        protected override void Render(ISvgRenderer renderer)
        {
            // Never render paint servers or their children
        }

        /// <summary>
        /// Gets a <see cref="SKPaint"/> representing the current paint server.
        /// </summary>
        /// <param name="styleOwner">The owner <see cref="SvgVisualElement"/>.</param>
        /// <param name="renderer">The renderer object.</param>
        /// <param name="opacity">The opacity of the paint.</param>
        /// <param name="forStroke">True if the paint is for a stroke.</param>
        public abstract SKPaint GetPaint(SvgVisualElement styleOwner, ISvgRenderer renderer, float opacity, bool forStroke = false);
    }
}
