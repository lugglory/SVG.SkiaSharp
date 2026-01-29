using System;
using SkiaSharp;

namespace Svg
{
    public interface ISvgRenderer : IDisposable
    {
        float DpiY { get; }
        void DrawImage(SKImage image, SKRect destRect, SKRect srcRect, SKPaint paint = null);
        void DrawImageUnscaled(SKImage image, SKPoint location);
        void DrawPath(SKPath path, SKPaint paint);
        void FillPath(SKPath path, SKPaint paint);
        ISvgBoundable GetBoundable();
        SKRegion GetClip();
        ISvgBoundable PopBoundable();
        void RotateTransform(float fAngle);
        void ScaleTransform(float sx, float sy);
        void SetBoundable(ISvgBoundable boundable);
        void SetClip(SKRegion region);
        void SetClip(SKPath path);
        void SetClip(SKRect rect);
        bool SmoothingMode { get; set; }
        SKMatrix Transform { get; set; }
        void TranslateTransform(float dx, float dy);
        void Save();
        void Restore();
    }
}
