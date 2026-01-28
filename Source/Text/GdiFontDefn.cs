#if !NO_SDC
using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Svg
{
    public class SkiaFontDefn : IFontDefn
    {
        private readonly SKTypeface _typeface;
        private readonly float _fontSize;
        private readonly float _ppi;

        public float Size => _fontSize;
        public float SizeInPoints => _fontSize * 72f / _ppi;

        public SkiaFontDefn(SKTypeface typeface, float fontSize, float ppi)
        {
            _typeface = typeface;
            _fontSize = fontSize;
            _ppi = ppi;
        }

        public void AddStringToPath(ISvgRenderer renderer, SKPath path, string text, SKPoint location)
        {
            using (var paint = new SKPaint { Typeface = _typeface, TextSize = _fontSize })
            {
                using (var textPath = paint.GetTextPath(text, location.X, location.Y))
                {
                    path.AddPath(textPath);
                }
            }
        }

        public float Ascent(ISvgRenderer renderer)
        {
            using (var paint = new SKPaint { Typeface = _typeface, TextSize = _fontSize })
            {
                return -paint.FontMetrics.Ascent;
            }
        }

        public IList<SKRect> MeasureCharacters(ISvgRenderer renderer, string text)
        {
            var results = new List<SKRect>();
            using (var paint = new SKPaint { Typeface = _typeface, TextSize = _fontSize })
            {
                float x = 0;
                foreach (var ch in text)
                {
                    var s = ch.ToString();
                    var width = paint.MeasureText(s);
                    results.Add(new SKRect(x, -paint.FontMetrics.Ascent, x + width, -paint.FontMetrics.Descent));
                    x += width;
                }
            }
            return results;
        }

        public SKSize MeasureString(ISvgRenderer renderer, string text)
        {
            using (var paint = new SKPaint { Typeface = _typeface, TextSize = _fontSize })
            {
                var width = paint.MeasureText(text);
                var metrics = paint.FontMetrics;
                return new SKSize(width, metrics.Descent - metrics.Ascent);
            }
        }

        public void Dispose()
        {
            _typeface?.Dispose();
        }
    }
}
#endif