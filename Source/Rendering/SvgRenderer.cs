#if !NO_SDC
using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Svg
{
    /// <summary>
    /// Convenience wrapper around a graphics object
    /// </summary>
    public sealed class SvgRenderer : ISvgRenderer
    {
        private readonly SKCanvas _canvas;
        private readonly bool _disposable;
        private readonly SKBitmap _bitmap; // Keep reference if we created it to dispose it? No, usually caller manages.
        // But if FromImage creates a canvas from bitmap, the canvas is disposable.

        private readonly Stack<ISvgBoundable> _boundables = new Stack<ISvgBoundable>();

        public void SetBoundable(ISvgBoundable boundable)
        {
            _boundables.Push(boundable);
        }
        public ISvgBoundable GetBoundable()
        {
            return _boundables.Count > 0 ? _boundables.Peek() : null;
        }
        public ISvgBoundable PopBoundable()
        {
            return _boundables.Pop();
        }

        public float DpiY
        {
            get { return 96.0f; } // Skia is pixel-based. Default to 96.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ISvgRenderer"/> class.
        /// </summary>
        private SvgRenderer(SKCanvas canvas, bool disposable = true)
        {
            _canvas = canvas;
            _disposable = disposable;
        }
        
        // Internal usage mostly
        public SKCanvas Canvas => _canvas;

        public void DrawImage(SKImage image, SKRect destRect, SKRect srcRect, SKPaint paint = null)
        {
            // Skia DrawImage takes dest rect. Src rect logic needs DrawImageRect.
            // DrawImageRect(SKImage image, SKRect src, SKRect dest, SKPaint paint)
            _canvas.DrawImage(image, srcRect, destRect, paint);
        }

        public void DrawImageUnscaled(SKImage image, SKPoint location)
        {
            _canvas.DrawImage(image, location);
        }

        public void DrawPath(SKPath path, SKPaint paint)
        {
            // Ensure paint is set to Stroke
            var oldStyle = paint.Style;
            paint.Style = SKPaintStyle.Stroke;
            _canvas.DrawPath(path, paint);
            paint.Style = oldStyle; // Restore? SvgElement usually manages separate paints, but let's be safe.
        }

        public void FillPath(SKPath path, SKPaint paint)
        {
            var oldStyle = paint.Style;
            paint.Style = SKPaintStyle.Fill;
            _canvas.DrawPath(path, paint);
            paint.Style = oldStyle;
        }

        public SKRegion GetClip()
        {
            var region = new SKRegion();
            region.SetRect(SKRectI.Round(_canvas.LocalClipBounds));
            return region;
        }

        public void RotateTransform(float fAngle)
        {
            _canvas.RotateDegrees(fAngle);
        }

        public void ScaleTransform(float sx, float sy)
        {
            _canvas.Scale(sx, sy);
        }

        public void SetClip(SKRegion region)
        {
            _canvas.ClipRegion(region);
        }

        public void SetClip(SKPath path)
        {
            _canvas.ClipPath(path);
        }

        public void SetClip(SKRect rect)
        {
            _canvas.ClipRect(rect);
        }

        public void TranslateTransform(float dx, float dy)
        {
            _canvas.Translate(dx, dy);
        }

        public bool SmoothingMode
        {
            get { return true; } // Always assume high quality in Skia or manage via Paint
            set { 
                // No-op. Antialiasing is per-paint in Skia.
            }
        }

        public SKMatrix Transform
        {
            get { return _canvas.TotalMatrix; }
            set { _canvas.SetMatrix(value); }
        }

        public void Save()
        {
            _canvas.Save();
        }

        public void Restore()
        {
            _canvas.Restore();
        }

        public void Dispose()
        {
            if (_disposable)
                _canvas.Dispose();
        }

        public static ISvgRenderer FromImage(SKBitmap bitmap)
        {
            var canvas = new SKCanvas(bitmap);
            return new SvgRenderer(canvas, true);
        }
        
        public static ISvgRenderer FromCanvas(SKCanvas canvas)
        {
            return new SvgRenderer(canvas, false);
        }
    }
}
#endif