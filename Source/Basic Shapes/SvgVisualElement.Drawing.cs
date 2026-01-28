#if !NO_SDC
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SkiaSharp;
using Svg.ExtensionMethods;
using Svg.FilterEffects;

namespace Svg
{
    public abstract partial class SvgVisualElement : SvgElement, ISvgBoundable, ISvgStylable, ISvgClipable
    {
        /// <summary>
        /// Gets the <see cref="SKPath"/> for this element.
        /// </summary>
        public abstract SKPath Path(ISvgRenderer renderer);

        SKPoint ISvgBoundable.Location
        {
            get
            {
                return new SKPoint(Bounds.Left, Bounds.Top);
            }
        }

        SKSize ISvgBoundable.Size
        {
            get
            {
                return new SKSize(Bounds.Width, Bounds.Height);
            }
        }

        /// <summary>
        /// Gets the bounds of the element.
        /// </summary>
        /// <value>The bounds.</value>
        public abstract SKRect Bounds { get; }

        /// <summary>
        /// Renders the <see cref="SvgElement"/> and contents to the specified <see cref="ISvgRenderer"/> object.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> object to render to.</param>
        protected override void Render(ISvgRenderer renderer)
        {
            if (Visible && Displayable && (!Renderable || Path(renderer) != null))
                RenderInternal(renderer, true);
        }

        private void RenderInternal(ISvgRenderer renderer, bool renderFilter)
        {
            if (!(renderFilter && RenderFilter(renderer)))
            {
                var opacity = FixOpacityValue(Opacity);
                if (opacity == 1f)
                {
                    if (Renderable)
                        RenderInternal(renderer, RenderFillAndStroke);
                    else
                        RenderInternal(renderer, RenderChildren);
                }
                else
                {
                    // For opacity < 1, we usually render to a layer.
                    // Skia makes this easy with SaveLayer.
                    var paint = new SKPaint { Color = SKColors.White.WithAlpha((byte)(opacity * 255)) };
                    renderer.Save();
                    // We might want to pass the opacity-paint to a SaveLayer call if ISvgRenderer supported it,
                    // but since we are refactoring, let's keep it simple for now or use a temp canvas if needed.
                    // Actually, let's just use SaveLayer if the renderer's canvas is accessible.
                    if (renderer is SvgRenderer svgRenderer)
                    {
                        svgRenderer.Canvas.SaveLayer(paint);
                        if (Renderable)
                            RenderFillAndStroke(renderer);
                        else
                            RenderChildren(renderer);
                        svgRenderer.Canvas.Restore();
                    }
                    else
                    {
                        // Fallback if not SvgRenderer
                        if (Renderable)
                            RenderFillAndStroke(renderer);
                        else
                            RenderChildren(renderer);
                    }
                    renderer.Restore();
                }
            }
        }

        private void RenderInternal(ISvgRenderer renderer, Action<ISvgRenderer> renderMethod)
        {
            try
            {
                if (PushTransforms(renderer))
                {
                    SetClip(renderer);
                    renderMethod.Invoke(renderer);
                    // ResetClip is redundant if we use Save/Restore in Push/Pop
                }
            }
            finally
            {
                PopTransforms(renderer);
            }
        }

        private bool RenderFilter(ISvgRenderer renderer)
        {
            var filterPath = Filter.ReplaceWithNullIfNone();
            if (filterPath != null)
            {
                var element = OwnerDocument.IdManager.GetElementById(filterPath);
                if (element is SvgFilter filter)
                {
                    try
                    {
                        filter.ApplyFilter(this, renderer, r => RenderInternal(r, false));
                    }
                    catch (Exception ex)
                    {
                        Debug.Print(ex.ToString());
                    }
                    return true;
                }
            }
            return false;
        }

        protected internal virtual void RenderFillAndStroke(ISvgRenderer renderer)
        {
            var smoothingMode = renderer.SmoothingMode;
            try
            {
                if (RequiresSmoothRendering)
                    renderer.SmoothingMode = true;

                RenderFill(renderer);
                RenderStroke(renderer);
            }
            finally
            {
                renderer.SmoothingMode = smoothingMode;
            }
        }

        protected internal virtual void RenderFill(ISvgRenderer renderer)
        {
            if (Fill != null)
            {
                using (var paint = Fill.GetPaint(this, renderer, FixOpacityValue(FillOpacity)))
                {
                    if (paint != null)
                    {
                        var path = Path(renderer);
                        if (path != null)
                        {
                            path.FillType = FillRule == SvgFillRule.NonZero ? SKPathFillType.Winding : SKPathFillType.EvenOdd;
                            renderer.FillPath(path, paint);
                        }
                    }
                }
            }
        }

        protected internal virtual bool RenderStroke(ISvgRenderer renderer)
        {
            if (Stroke != null && Stroke != SvgPaintServer.None && StrokeWidth > 0f)
            {
                var strokeWidth = StrokeWidth.ToDeviceValue(renderer, UnitRenderingType.Other, this);
                using (var paint = Stroke.GetPaint(this, renderer, FixOpacityValue(StrokeOpacity), true))
                {
                    if (paint != null)
                    {
                        var path = Path(renderer);
                        if (path == null || path.PointCount < 1) return false;

                        paint.Style = SKPaintStyle.Stroke;
                        paint.StrokeWidth = strokeWidth;
                        paint.StrokeMiter = StrokeMiterLimit;

                        // Join
                        switch (StrokeLineJoin)
                        {
                            case SvgStrokeLineJoin.Bevel: paint.StrokeJoin = SKStrokeJoin.Bevel; break;
                            case SvgStrokeLineJoin.Round: paint.StrokeJoin = SKStrokeJoin.Round; break;
                            default: paint.StrokeJoin = SKStrokeJoin.Miter; break;
                        }

                        // Cap
                        switch (StrokeLineCap)
                        {
                            case SvgStrokeLineCap.Round: paint.StrokeCap = SKStrokeCap.Round; break;
                            case SvgStrokeLineCap.Square: paint.StrokeCap = SKStrokeCap.Square; break;
                            default: paint.StrokeCap = SKStrokeCap.Butt; break;
                        }

                        // Dash
                        if (StrokeDashArray != null && StrokeDashArray.Count > 0)
                        {
                            var dashPattern = StrokeDashArray.Select(u => u.ToDeviceValue(renderer, UnitRenderingType.Other, this)).ToArray();
                            float dashOffset = StrokeDashOffset.ToDeviceValue(renderer, UnitRenderingType.Other, this);
                            paint.PathEffect = SKPathEffect.CreateDash(dashPattern, dashOffset);
                        }

                        renderer.DrawPath(path, paint);
                        return true;
                    }
                }
            }
            return false;
        }

        protected internal virtual void SetClip(ISvgRenderer renderer)
        {
            var clipPath = this.ClipPath.ReplaceWithNullIfNone();
            var clip = this.Clip;
            if (clipPath != null || !string.IsNullOrEmpty(clip))
            {
                // We don't need _previousClip because of Save/Restore in Push/Pop
                if (clipPath != null)
                {
                    var element = this.OwnerDocument.GetElementById<SvgClipPath>(clipPath.ToString());
                    if (element != null)
                    {
                        var path = element.GetClipPath(this, renderer);
                        if (path != null) renderer.SetClip(path);
                    }
                }

                if (!string.IsNullOrEmpty(clip) && clip.StartsWith("rect("))
                {
                    clip = clip.Trim();
                    var offsets = (from o in clip.Substring(5, clip.Length - 6).Split(',')
                                   select float.Parse(o.Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture)).ToList();
                    var bounds = this.Bounds;
                    var clipRect = new SKRect(bounds.Left + offsets[3], bounds.Top + offsets[0],
                                              bounds.Right - offsets[1], bounds.Bottom - offsets[2]);
                    var path = new SKPath();
                    path.AddRect(clipRect);
                    renderer.SetClip(path);
                }
            }
        }

        protected internal virtual void ResetClip(ISvgRenderer renderer)
        {
            // No-op, handled by Restore()
        }

        void ISvgClipable.SetClip(ISvgRenderer renderer) { this.SetClip(renderer); }
        void ISvgClipable.ResetClip(ISvgRenderer renderer) { this.ResetClip(renderer); }
    }
}
#endif