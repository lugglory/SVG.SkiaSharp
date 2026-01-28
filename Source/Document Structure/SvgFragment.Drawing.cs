#if !NO_SDC
using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Svg
{
    public partial class SvgFragment : SvgElement, ISvgBoundable
    {
        SKPoint ISvgBoundable.Location
        {
            get { return SKPoint.Empty; }
        }

        SKSize ISvgBoundable.Size
        {
            get
            {
                if (Width.Type == SvgUnitType.Percentage || Height.Type == SvgUnitType.Percentage)
                    return SKSize.Empty;
                return GetDimensions();
            }
        }

        SKRect ISvgBoundable.Bounds
        {
            get { 
                var size = ((ISvgBoundable)this).Size;
                return new SKRect(0, 0, size.Width, size.Height); 
            }
        }

        protected internal override bool PushTransforms(ISvgRenderer renderer)
        {
            if (!base.PushTransforms(renderer))
                return false;
            ViewBox.AddViewBoxTransform(AspectRatio, renderer, this);
            return true;
        }

        protected override void Render(ISvgRenderer renderer)
        {
            switch (Overflow)
            {
                case SvgOverflow.Auto:
                case SvgOverflow.Visible:
                case SvgOverflow.Inherit:
                    base.Render(renderer);
                    break;
                default:
                    renderer.Save();
                    try
                    {
                        var size = this is SvgDocument ? renderer.GetBoundable().Bounds.Size : GetDimensions(renderer);
                        var clip = new SKRect(X.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this),
                            Y.ToDeviceValue(renderer, UnitRenderingType.Vertical, this),
                            X.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this) + size.Width, 
                            Y.ToDeviceValue(renderer, UnitRenderingType.Vertical, this) + size.Height);
                        renderer.SetClip(clip);
                        try
                        {
                            renderer.SetBoundable(new GenericBoundable(clip));
                            base.Render(renderer);
                        }
                        finally
                        {
                            renderer.PopBoundable();
                        }
                    }
                    finally
                    {
                        renderer.Restore();
                    }
                    break;
            }
        }

        public SKPath Path
        {
            get
            {
                var path = new SKPath();
                AddPaths(this, path);
                return path;
            }
        }

        public SKRect Bounds
        {
            get
            {
                var bounds = SKRect.Empty;
                foreach (var child in Children)
                {
                    SKRect childBounds = SKRect.Empty;
                    if (child is SvgFragment fragment)
                    {
                        childBounds = fragment.Bounds;
                        childBounds.Offset(fragment.X, fragment.Y);
                    }
                    else if (child is SvgVisualElement visualElement)
                    {
                        childBounds = visualElement.Bounds;
                    }

                    if (!childBounds.IsEmpty)
                    {
                        if (bounds.IsEmpty)
                        {
                            bounds = childBounds;
                        }
                        else
                        {
                            bounds = SKRect.Union(bounds, childBounds);
                        }
                    }
                }

                return TransformedBounds(bounds);
            }
        }

        public SKSize GetDimensions()
        {
            return GetDimensions(null);
        }

        internal SKSize GetDimensions(ISvgRenderer renderer)
        {
            float w, h;
            var isWidthperc = Width.Type == SvgUnitType.Percentage;
            var isHeightperc = Height.Type == SvgUnitType.Percentage;

            var bounds = SKRect.Empty;
            if (isWidthperc || isHeightperc)
            {
                if (ViewBox.Width > 0 && ViewBox.Height > 0)
                {
                    bounds = new SKRect(ViewBox.MinX, ViewBox.MinY, ViewBox.MinX + ViewBox.Width, ViewBox.MinY + ViewBox.Height);
                }
                else
                {
                    bounds = Bounds;
                }
            }

            if (isWidthperc && this is SvgDocument)
            {
                w = (bounds.Width + bounds.Left) * (Width.Value * 0.01f);
            }
            else
            {
                w = Width.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
            }
            
            if (isHeightperc && this is SvgDocument)
            {
                h = (bounds.Height + bounds.Top) * (Height.Value * 0.01f);
            }
            else
            {
                h = Height.ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
            }

            return new SKSize(w, h);
        }
    }
}
#endif