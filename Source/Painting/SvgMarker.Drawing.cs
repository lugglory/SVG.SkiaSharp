#if !NO_SDC
using System;
using System.Linq;
using SkiaSharp;
using Svg.DataTypes;

namespace Svg
{
    public partial class SvgMarker : SvgPathBasedElement, ISvgViewPort
    {
        public override SKPath Path(ISvgRenderer renderer)
        {
            if (MarkerElement != null)
                return MarkerElement.Path(renderer);
            return null;
        }

        /// <summary>
        /// Render this marker using the slope of the given line segment
        /// </summary>
        public void RenderMarker(ISvgRenderer pRenderer, SvgVisualElement pOwner, SKPoint pRefPoint, SKPoint pMarkerPoint1, SKPoint pMarkerPoint2, bool isStartMarker)
        {
            float fAngle1 = 0f;
            if (Orient.IsAuto)
            {
                float xDiff = pMarkerPoint2.X - pMarkerPoint1.X;
                float yDiff = pMarkerPoint2.Y - pMarkerPoint1.Y;
                fAngle1 = (float)(Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI);

                if (isStartMarker && Orient.IsAutoStartReverse)
                {
                    fAngle1 += 180;
                }
            }

            RenderPart2(fAngle1, pRenderer, pOwner, pRefPoint);
        }

        /// <summary>
        /// Render this marker using the average of the slopes of the two given line segments
        /// </summary>
        public void RenderMarker(ISvgRenderer pRenderer, SvgVisualElement pOwner, SKPoint pRefPoint, SKPoint pMarkerPoint1, SKPoint pMarkerPoint2, SKPoint pMarkerPoint3)
        {
            float xDiff = pMarkerPoint2.X - pMarkerPoint1.X;
            float yDiff = pMarkerPoint2.Y - pMarkerPoint1.Y;
            float fAngle1 = (float)(Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI);

            xDiff = pMarkerPoint3.X - pMarkerPoint2.X;
            yDiff = pMarkerPoint3.Y - pMarkerPoint2.Y;
            float fAngle2 = (float)(Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI);

            RenderPart2((fAngle1 + fAngle2) / 2, pRenderer, pOwner, pRefPoint);
        }

        private void RenderPart2(float fAngle, ISvgRenderer pRenderer, SvgVisualElement pOwner, SKPoint pMarkerPoint)
        {
            using (var markerPathOriginal = Path(pRenderer))
            {
                if (markerPathOriginal == null) return;
                
                using (var markerPath = GetClone(pOwner, pRenderer))
                {
                    var transMatrix = SKMatrix.CreateTranslation(pMarkerPoint.X, pMarkerPoint.Y);
                    if (Orient.IsAuto)
                        transMatrix = transMatrix.PostConcat(SKMatrix.CreateRotationDegrees(fAngle));
                    else
                        transMatrix = transMatrix.PostConcat(SKMatrix.CreateRotationDegrees(Orient.Angle));
                    
                    switch (MarkerUnits)
                    {
                        case SvgMarkerUnits.StrokeWidth:
                            if (ViewBox.Width > 0 && ViewBox.Height > 0)
                            {
                                transMatrix = transMatrix.PostConcat(SKMatrix.CreateScale(MarkerWidth, MarkerHeight));
                                var strokeWidth = pOwner.StrokeWidth.ToDeviceValue(pRenderer, UnitRenderingType.Other, this);
                                transMatrix = transMatrix.PostConcat(SKMatrix.CreateTranslation(
                                    AdjustForViewBoxWidth(-RefX.ToDeviceValue(pRenderer, UnitRenderingType.Horizontal, this) * strokeWidth),
                                    AdjustForViewBoxHeight(-RefY.ToDeviceValue(pRenderer, UnitRenderingType.Vertical, this) * strokeWidth)));
                            }
                            else
                            {
                                transMatrix = transMatrix.PostConcat(SKMatrix.CreateTranslation(
                                    -RefX.ToDeviceValue(pRenderer, UnitRenderingType.Horizontal, this),
                                    -RefY.ToDeviceValue(pRenderer, UnitRenderingType.Vertical, this)));
                            }
                            break;
                        case SvgMarkerUnits.UserSpaceOnUse:
                            transMatrix = transMatrix.PostConcat(SKMatrix.CreateTranslation(
                                -RefX.ToDeviceValue(pRenderer, UnitRenderingType.Horizontal, this),
                                -RefY.ToDeviceValue(pRenderer, UnitRenderingType.Vertical, this)));
                            break;
                    }

                    if (MarkerElement != null && MarkerElement.Transforms != null)
                    {
                        var matrix = MarkerElement.Transforms.GetMatrix();
                        transMatrix = transMatrix.PostConcat(matrix);
                    }
                    
                    markerPath.Transform(transMatrix);

                    // Render Fill
                    var firstChild = this.Children.FirstOrDefault();
                    if (firstChild != null && firstChild.Fill != null)
                    {
                        using (var pBrush = firstChild.Fill.GetPaint(this, pRenderer, FixOpacityValue(FillOpacity)))
                        {
                            if (pBrush != null) pRenderer.FillPath(markerPath, pBrush);
                        }
                    }

                    // Render Stroke
                    using (var pRenderPaint = CreateStrokePaint(pOwner, pRenderer))
                    {
                        if (pRenderPaint != null) pRenderer.DrawPath(markerPath, pRenderPaint);
                    }
                }
            }
        }

        private SKPaint CreateStrokePaint(SvgVisualElement pPath, ISvgRenderer renderer)
        {
            if (this.Stroke == null) return null;
            var paint = this.Stroke.GetPaint(this, renderer, FixOpacityValue(Opacity), true);
            if (paint == null) return null;

            float width = 1f;
            switch (MarkerUnits)
            {
                case SvgMarkerUnits.StrokeWidth:
                    width = pPath.StrokeWidth.ToDeviceValue(renderer, UnitRenderingType.Other, this);
                    break;
                case SvgMarkerUnits.UserSpaceOnUse:
                    width = StrokeWidth.ToDeviceValue(renderer, UnitRenderingType.Other, this);
                    break;
            }
            paint.StrokeWidth = width;
            paint.Style = SKPaintStyle.Stroke;
            return paint;
        }

        private SKPath GetClone(SvgVisualElement pPath, ISvgRenderer renderer)
        {
            var pRet = new SKPath(Path(renderer));
            switch (MarkerUnits)
            {
                case SvgMarkerUnits.StrokeWidth:
                    var sw = pPath.StrokeWidth.ToDeviceValue(renderer, UnitRenderingType.Other, pPath);
                    var transMatrix = SKMatrix.CreateScale(AdjustForViewBoxWidth(sw), AdjustForViewBoxHeight(sw));
                    pRet.Transform(transMatrix);
                    break;
            }
            return pRet;
        }

        private float AdjustForViewBoxWidth(float fWidth)
        {
            return (ViewBox.Width <= 0 ? 1 : fWidth / ViewBox.Width);
        }

        private float AdjustForViewBoxHeight(float fHeight)
        {
            return (ViewBox.Height <= 0 ? 1 : fHeight / ViewBox.Height);
        }
    }
}
#endif