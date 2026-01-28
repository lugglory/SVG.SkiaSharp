#if !NO_SDC
using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Svg
{
    public partial class SvgPatternServer : SvgPaintServer, ISvgViewPort
    {
        private SKMatrix EffectivePatternTransform
        {
            get
            {
                var transform = SKMatrix.CreateIdentity();
                if (PatternTransform != null)
                {
                    var matrix = PatternTransform.GetMatrix();
                    transform = transform.PostConcat(matrix);
                }

                return transform;
            }
        }

        public override SKPaint GetPaint(SvgVisualElement renderingElement, ISvgRenderer renderer, float opacity, bool forStroke = false)
        {
            var chain = new List<SvgPatternServer>();

            var curr = this;
            do
            {
                chain.Add(curr);
                curr = SvgDeferredPaintServer.TryGet<SvgPatternServer>(curr.InheritGradient, renderingElement);
            } while (curr != null);

            var firstChildren = chain.Find(p => p.Children.Count > 0);
            if (firstChildren == null)
                return null;
            var firstX = chain.Find(p => p.X != SvgUnit.None);
            var firstY = chain.Find(p => p.Y != SvgUnit.None);
            var firstWidth = chain.Find(p => p.Width != SvgUnit.None);
            var firstHeight = chain.Find(p => p.Height != SvgUnit.None);
            if (firstWidth == null || firstHeight == null)
                return null;
            var firstPatternUnit = chain.Find(p => p._patternUnits.HasValue);
            var firstPatternContentUnit = chain.Find(p => p._patternContentUnits.HasValue);
            var firstViewBox = chain.Find(p => p.ViewBox != SvgViewBox.Empty);

            var xUnit = firstX == null ? new SvgUnit(0f) : firstX.X;
            var yUnit = firstY == null ? new SvgUnit(0f) : firstY.Y;
            var widthUnit = firstWidth.Width;
            var heightUnit = firstHeight.Height;

            var patternUnits = firstPatternUnit == null ? SvgCoordinateUnits.ObjectBoundingBox : firstPatternUnit.PatternUnits;
            var patternContentUnits = firstPatternContentUnit == null ? SvgCoordinateUnits.UserSpaceOnUse : firstPatternContentUnit.PatternContentUnits;
            var viewBox = firstViewBox == null ? SvgViewBox.Empty : firstViewBox.ViewBox;

            var isPatternObjectBoundingBox = patternUnits == SvgCoordinateUnits.ObjectBoundingBox;
            try
            {
                if (isPatternObjectBoundingBox)
                    renderer.SetBoundable(renderingElement);

                var x = xUnit.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
                var y = yUnit.ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
                var width = widthUnit.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
                var height = heightUnit.ToDeviceValue(renderer, UnitRenderingType.Vertical, this);

                if (isPatternObjectBoundingBox)
                {
                    var bounds = renderer.GetBoundable().Bounds;

                    if (xUnit.Type != SvgUnitType.Percentage)
                        x *= bounds.Width;
                    if (yUnit.Type != SvgUnitType.Percentage)
                        y *= bounds.Height;
                    if (widthUnit.Type != SvgUnitType.Percentage)
                        width *= bounds.Width;
                    if (heightUnit.Type != SvgUnitType.Percentage)
                        height *= bounds.Height;
                    x += bounds.Left;
                    y += bounds.Top;
                }

                if (width <= 0f || height <= 0f)
                    return null;

                var imgInfo = new SKImageInfo((int)Math.Ceiling(width), (int)Math.Ceiling(height));
                using (var surface = SKSurface.Create(imgInfo))
                {
                    if (surface == null) return null;
                    var canvas = surface.Canvas;
                    using (var tileRenderer = SvgRenderer.FromCanvas(canvas))
                    {
                        tileRenderer.SetBoundable(renderingElement);
                        if (viewBox != SvgViewBox.Empty)
                        {
                            tileRenderer.ScaleTransform(width / viewBox.Width, height / viewBox.Height);
                        }
                        else if (patternContentUnits == SvgCoordinateUnits.ObjectBoundingBox)
                        {
                            var bounds = tileRenderer.GetBoundable().Bounds;
                            tileRenderer.ScaleTransform(bounds.Width, bounds.Height);
                        }

                        foreach (var child in firstChildren.Children)
                            child.RenderElement(tileRenderer);
                    }

                    using (var image = surface.Snapshot())
                    {
                        var transform = EffectivePatternTransform;
                        transform = transform.PostConcat(SKMatrix.CreateTranslation(x, y));
                        
                        var paint = new SKPaint();
                        paint.IsAntialias = true;
                        paint.Shader = image.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat, transform);
                        
                        // Handle opacity if needed (texture opacity)
                        if (opacity < 1.0f)
                        {
                            paint.Color = paint.Color.WithAlpha((byte)(opacity * 255));
                        }

                        return paint;
                    }
                }
            }
            finally
            {
                if (isPatternObjectBoundingBox)
                    renderer.PopBoundable();
            }
        }
    }
}
#endif