using System.Collections.Generic;
using SkiaSharp;
using Svg;

namespace SVGViewer
{
    class DebugRenderer : ISvgRenderer
    {
        private readonly Stack<ISvgBoundable> _boundables = new Stack<ISvgBoundable>();

        private SKRegion _clip = new SKRegion();
        private SKMatrix _transform = SKMatrix.CreateIdentity();

        public void SetBoundable(ISvgBoundable boundable)
        {
            _boundables.Push(boundable);
        }
        public ISvgBoundable GetBoundable()
        {
            return _boundables.Peek();
        }
        public ISvgBoundable PopBoundable()
        {
            return _boundables.Pop();
        }

        public float DpiY
        {
            get { return 96; }
        }

        public void DrawImage(SKImage image, SKRect destRect, SKRect srcRect, SKPaint paint = null)
        {
        }

        public void DrawImageUnscaled(SKImage image, SKPoint location)
        {
        }
        public void DrawPath(SKPath path, SKPaint paint)
        {
            using (var newPath = new SKPath(path))
            {
                newPath.Transform(_transform);
            }
        }
        public void FillPath(SKPath path, SKPaint paint)
        {
            using (var newPath = new SKPath(path))
            {
                newPath.Transform(_transform);
            }
        }
        public SKRegion GetClip()
        {
            return _clip;
        }
        public void RotateTransform(float fAngle)
        {
            _transform = _transform.PostConcat(SKMatrix.CreateRotationDegrees(fAngle));
        }
        public void ScaleTransform(float sx, float sy)
        {
            _transform = _transform.PostConcat(SKMatrix.CreateScale(sx, sy));
        }
        public void SetClip(SKRegion region)
        {
            _clip = region;
        }
        public void SetClip(SKPath path)
        {
        }
        public void SetClip(SKRect rect)
        {
        }
        public void TranslateTransform(float dx, float dy)
        {
            _transform = _transform.PostConcat(SKMatrix.CreateTranslation(dx, dy));
        }

        public bool SmoothingMode
        {
            get { return true; }
            set { /* Do Nothing */ }
        }

        public SKMatrix Transform
        {
            get { return _transform; }
            set { _transform = value; }
        }

        public void Save()
        {
        }

        public void Restore()
        {
        }

        public void Dispose()
        {
            _clip?.Dispose();
        }
    }
}