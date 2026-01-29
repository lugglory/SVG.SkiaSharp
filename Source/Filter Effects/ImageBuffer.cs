using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Svg.FilterEffects
{
    public class ImageBuffer : Dictionary<string, SKBitmap>, IDisposable
    {
        private const string BufferKey = "__!!BUFFER";

        private readonly SKRect _bounds;
        private readonly ISvgRenderer _renderer;
        private readonly Action<ISvgRenderer> _renderMethod;
        private readonly float _inflate;

        public SKMatrix Transform { get; set; } = SKMatrix.CreateIdentity();

        public SKBitmap Buffer
        {
            get { return this[BufferKey]; }
        }

        public new SKBitmap this[string key]
        {
            get { return ProcessResult(ProcessKey(key), base.ContainsKey(ProcessKey(key)) ? base[ProcessKey(key)] : null); }
            set 
            { 
                var processedKey = ProcessKey(key);
                if (base.TryGetValue(processedKey, out var oldBitmap))
                {
                    oldBitmap?.Dispose();
                }
                base[processedKey] = value; 
            }
        }

        public ImageBuffer(SKRect bounds, float inflate, ISvgRenderer renderer, Action<ISvgRenderer> renderMethod)
        {
            _bounds = bounds;
            _inflate = inflate;
            _renderer = renderer;
            _renderMethod = renderMethod;

            this[SvgFilterPrimitive.SourceGraphic] = null;
            this[SvgFilterPrimitive.SourceAlpha] = null;
            this[SvgFilterPrimitive.BackgroundImage] = null;
            this[SvgFilterPrimitive.BackgroundAlpha] = null;
            this[SvgFilterPrimitive.FillPaint] = null;
            this[SvgFilterPrimitive.StrokePaint] = null;
        }

        public new void Add(string key, SKBitmap value)
        {
            base.Add(ProcessKey(key), value);
        }

        public new bool ContainsKey(string key)
        {
            return base.ContainsKey(ProcessKey(key));
        }

        public new void Clear()
        {
            foreach (var kvp in this) kvp.Value?.Dispose();
            base.Clear();
        }

        public new bool Remove(string key)
        {
            var k = ProcessKey(key);
            switch (k)
            {
                case SvgFilterPrimitive.SourceGraphic:
                case SvgFilterPrimitive.SourceAlpha:
                case SvgFilterPrimitive.BackgroundImage:
                case SvgFilterPrimitive.BackgroundAlpha:
                case SvgFilterPrimitive.FillPaint:
                case SvgFilterPrimitive.StrokePaint:
                    return false;
                default:
                    if (base.TryGetValue(k, out var bmp))
                    {
                        bmp?.Dispose();
                        return base.Remove(k);
                    }
                    return false;
            }
        }

        public new bool TryGetValue(string key, out SKBitmap value)
        {
            if (base.TryGetValue(ProcessKey(key), out value))
            {
                value = ProcessResult(ProcessKey(key), value);
                return true;
            }
            return false;
        }

        private SKBitmap ProcessResult(string key, SKBitmap curr)
        {
            if (curr == null)
            {
                switch (key)
                {
                    case SvgFilterPrimitive.SourceGraphic:
                        this[key] = CreateSourceGraphic();
                        return this[key];
                    case SvgFilterPrimitive.SourceAlpha:
                        this[key] = CreateSourceAlpha();
                        return this[key];
                }
            }
            return curr;
        }

        private string ProcessKey(string key)
        {
            return string.IsNullOrEmpty(key) ? (ContainsKey(BufferKey) ? BufferKey : SvgFilterPrimitive.SourceGraphic) : key;
        }

        private SKBitmap CreateSourceGraphic()
        {
            int width = (int)Math.Ceiling(_bounds.Width + 2 * _inflate * _bounds.Width + _bounds.Left);
            int height = (int)Math.Ceiling(_bounds.Height + 2 * _inflate * _bounds.Height + _bounds.Top);
            
            var graphic = new SKBitmap(width, height);
            using (var canvas = new SKCanvas(graphic))
            using (var renderer = SvgRenderer.FromCanvas(canvas))
            {
                renderer.SetBoundable(_renderer.GetBoundable());
                var transform = SKMatrix.CreateTranslation(_bounds.Width * _inflate, _bounds.Height * _inflate);
                renderer.Transform = transform;
                _renderMethod.Invoke(renderer);
            }
            return graphic;
        }

        private SKBitmap CreateSourceAlpha()
        {
            var source = this[SvgFilterPrimitive.SourceGraphic];
            if (source == null) return null;

            var sourceAlpha = new SKBitmap(source.Width, source.Height);
            using (var canvas = new SKCanvas(sourceAlpha))
            using (var paint = new SKPaint())
            {
                // Color matrix to extract alpha: 
                // [ 0 0 0 0 0 ]
                // [ 0 0 0 0 0 ]
                // [ 0 0 0 0 0 ]
                // [ 0 0 0 1 0 ]
                // [ 0 0 0 0 1 ]
                float[] matrix = {
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 1, 0
                };
                paint.ColorFilter = SKColorFilter.CreateColorMatrix(matrix);
                canvas.DrawBitmap(source, 0, 0, paint);
            }
            return sourceAlpha;
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
