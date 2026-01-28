#if !NO_SDC
using System.Globalization;
using System.Linq;
using SkiaSharp;

namespace Svg
{
    public partial class SvgSwitch : SvgVisualElement
    {
        private readonly string _systemLanguageName = CultureInfo.CurrentCulture.Name.ToLower();
        private readonly string _systemLanguageShortName = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

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

        protected override void Render(ISvgRenderer renderer)
        {
            if (!Visible || !Displayable)
                return;

            try
            {
                if (!PushTransforms(renderer))
                    return;

                SetClip(renderer);
                foreach (var element in Children)
                {
                    if (element.CustomAttributes.ContainsKey("systemLanguage"))
                    {
                        var languages = element.CustomAttributes["systemLanguage"].Split(',');
                        if (!languages.Contains(_systemLanguageName) && !languages.Contains(_systemLanguageShortName))
                        {
                            continue;
                        }
                    }

                    element.RenderElement(renderer);
                    break;
                }
            }
            finally
            {
                PopTransforms(renderer);
            }
        }
    }
}
#endif