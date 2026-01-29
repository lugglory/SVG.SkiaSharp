using System;
using System.Linq;
using SkiaSharp;

namespace Svg
{
    public abstract partial class SvgGradientServer : SvgPaintServer
    {
        public override SKPaint GetPaint(SvgVisualElement styleOwner, ISvgRenderer renderer, float opacity, bool forStroke = false)
        {
            LoadStops(styleOwner);

            if (Stops.Count == 0)
                return null;
            else if (Stops.Count == 1)
            {
                var stopColor = Stops[0].GetColor(styleOwner);
                var paint = new SKPaint();
                paint.IsAntialias = true;
                float finalOpacity = opacity * (stopColor.Alpha / 255.0f);
                paint.Color = stopColor.WithAlpha((byte)Math.Round(finalOpacity * 255));
                return paint;
            }

            return CreatePaint(styleOwner, renderer, opacity, forStroke);
        }

        protected abstract SKPaint CreatePaint(SvgVisualElement renderingElement, ISvgRenderer renderer, float opacity, bool forStroke);

        protected SKMatrix EffectiveGradientTransform
        {
            get
            {
                var transform = SKMatrix.CreateIdentity();

                if (GradientTransform != null)
                {
                    var matrix = GradientTransform.GetMatrix();
                    transform = transform.PostConcat(matrix);
                }

                return transform;
            }
        }

        protected (SKColor[] Colors, float[] Offsets) GetColorBlend(ISvgRenderer renderer, float opacity, bool radial)
        {
            var colourBlends = Stops.Count;
            var insertStart = false;
            var insertEnd = false;

            // If the first stop doesn't start at zero
            if (Stops[0].Offset.Value > 0f)
            {
                colourBlends++;
                if (radial) insertEnd = true;
                else insertStart = true;
            }

            // If the last stop doesn't end at 100%
            var lastValue = Stops[Stops.Count - 1].Offset.Value;
            if (lastValue < 100f)
            {
                colourBlends++;
                if (radial) insertStart = true;
                else insertEnd = true;
            }

            var colors = new SKColor[colourBlends];
            var positions = new float[colourBlends];

            var actualStops = 0;
            for (var i = 0; i < colourBlends; i++)
            {
                var currentStop = Stops[radial ? Stops.Count - 1 - actualStops : actualStops];
                
                // Note: ToDeviceValue might depend on bounds. 
                // SVG gradient offsets are usually 0..1 relative to the object or the coordinate system.
                // SvgUnit.ToDeviceValue for percentages uses the provided dimension.
                // For gradients, we usually want the 0..1 value.
                float offsetValue = currentStop.Offset.Value / 100f; // SvgUnit.Offset is converted to percentage in SvgGradientStop setter.

                var mergedOpacity = opacity * currentStop.StopOpacity;
                var position = radial ? 1.0f - offsetValue : offsetValue;
                position = Math.Min(Math.Max(position, 0f), 1f);
                
                var stopColor = currentStop.GetColor(this);
                var color = stopColor.WithAlpha((byte)Math.Round(mergedOpacity * (stopColor.Alpha / 255.0f) * 255));

                actualStops++;

                if (insertStart && i == 0)
                {
                    positions[i] = 0.0f;
                    colors[i] = color;
                    i++;
                }

                positions[i] = position;
                colors[i] = color;

                if (insertEnd && i == colourBlends - 2)
                {
                    i++;
                    positions[i] = 1.0f;
                    colors[i] = color;
                }
            }

            return (colors, positions);
        }

        protected SKShaderTileMode GetSKTileMode()
        {
            switch (SpreadMethod)
            {
                case SvgGradientSpreadMethod.Reflect:
                    return SKShaderTileMode.Mirror;
                case SvgGradientSpreadMethod.Repeat:
                    return SKShaderTileMode.Repeat;
                default:
                    return SKShaderTileMode.Clamp;
            }
        }

        protected SvgUnit NormalizeUnit(SvgUnit orig)
        {
            return orig.Type == SvgUnitType.Percentage && GradientUnits == SvgCoordinateUnits.ObjectBoundingBox ?
                new SvgUnit(SvgUnitType.User, orig.Value / 100f) : orig;
        }
    }
}
