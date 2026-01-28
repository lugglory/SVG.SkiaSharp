#if !NO_SDC
using System;
using SkiaSharp;

namespace Svg
{
    public partial class SvgUse : SvgVisualElement
    {
        /// <summary>
        /// Applies the required transforms to <see cref="ISvgRenderer"/>.
        /// </summary>
        protected internal override bool PushTransforms(ISvgRenderer renderer)
        {
            if (!base.PushTransforms(renderer))
                return false;
            
            var dx = X.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
            var dy = Y.ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
            
            // Apply translation to current transform
            var current = renderer.Transform;
            renderer.Transform = current.PostConcat(SKMatrix.CreateTranslation(dx, dy));
            
            return true;
        }

        public override SKPath Path(ISvgRenderer renderer)
        {
            if (this.OwnerDocument.IdManager.GetElementById(this.ReferencedElement) is SvgVisualElement element && !this.HasRecursiveReference())
            {
                return element.Path(renderer);
            }
            return null;
        }

        /// <summary>
        /// Gets the bounds of the element.
        /// </summary>
        public override SKRect Bounds
        {
            get
            {
                var ew = this.Width.ToDeviceValue(null, UnitRenderingType.Horizontal, this);
                var eh = this.Height.ToDeviceValue(null, UnitRenderingType.Vertical, this);
                if (ew > 0 && eh > 0)
                {
                    var location = this.Location.ToDeviceValue(null, this);
                    return TransformedBounds(new SKRect(location.X, location.Y, location.X + ew, location.Y + eh));
                }
                
                if (this.OwnerDocument.IdManager.GetElementById(this.ReferencedElement) is SvgVisualElement element)
                {
                    return element.Bounds;
                }

                return SKRect.Empty;
            }
        }

        protected override void RenderChildren(ISvgRenderer renderer)
        {
            if (ReferencedElement != null && !HasRecursiveReference())
            {
                if (OwnerDocument.IdManager.GetElementById(ReferencedElement) is SvgVisualElement element)
                {
                    var ew = Width.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
                    var eh = Height.ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
                    if (ew > 0 && eh > 0)
                    {
                        var viewBox = element.Attributes.GetAttribute<SvgViewBox>("viewBox");
                        if (viewBox != SvgViewBox.Empty && Math.Abs(ew - viewBox.Width) > float.Epsilon && Math.Abs(eh - viewBox.Height) > float.Epsilon)
                        {
                            var sw = ew / viewBox.Width;
                            var sh = eh / viewBox.Height;
                            // Apply scale to current transform
                            var current = renderer.Transform;
                            renderer.Transform = current.PostConcat(SKMatrix.CreateScale(sw, sh));
                        }
                    }

                    var origParent = element.Parent;
                    element._parent = this;
                    element.InvalidateChildPaths();
                    element.RenderElement(renderer);
                    element._parent = origParent;
                }
            }
        }
    }
}
#endif