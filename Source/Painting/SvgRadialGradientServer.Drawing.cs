#if !NO_SDC
using System;
using System.Linq;
using SkiaSharp;

namespace Svg
{
    public partial class SvgRadialGradientServer : SvgGradientServer
    {
        protected override SKPaint CreatePaint(SvgVisualElement renderingElement, ISvgRenderer renderer, float opacity, bool forStroke)
        {
            try
            {
                if (this.GradientUnits == SvgCoordinateUnits.ObjectBoundingBox) renderer.SetBoundable(renderingElement);

                var center = SvgUnit.GetDevicePoint(NormalizeUnit(this.CenterX), NormalizeUnit(this.CenterY), renderer, this);
                var focal = SvgUnit.GetDevicePoint(NormalizeUnit(this.FocalX), NormalizeUnit(this.FocalY), renderer, this);
                var radius = NormalizeUnit(this.Radius).ToDeviceValue(renderer, UnitRenderingType.Other, this);
                // Note: SVG also has 'fr' (focal radius), but this library might not have it yet. Defaulting to 0.
                float focalRadius = 0f;

                var bounds = renderer.GetBoundable().Bounds;
                var transform = EffectiveGradientTransform;

                var preTransform = SKMatrix.CreateTranslation(bounds.Left, bounds.Top);
                if (this.GradientUnits == SvgCoordinateUnits.ObjectBoundingBox)
                {
                    preTransform = preTransform.PostConcat(SKMatrix.CreateScale(bounds.Width, bounds.Height));
                }
                transform = preTransform.PostConcat(transform);

                var blend = GetColorBlend(renderer, opacity, true);

                var paint = new SKPaint();
                paint.IsAntialias = true;
                paint.Shader = SKShader.CreateTwoPointConicalGradient(
                    focal, focalRadius,
                    center, radius,
                    blend.Colors, blend.Offsets,
                    GetSKTileMode(),
                    transform
                );

                return paint;
            }
            finally
            {
                if (this.GradientUnits == SvgCoordinateUnits.ObjectBoundingBox) renderer.PopBoundable();
            }
        }
    }
}
#endif