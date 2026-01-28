#if !NO_SDC
using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Svg
{
    public class SvgFontDefn : IFontDefn
    {
        private SvgFont _font;
        private float _emScale;
        private float _ppi;
        private float _size;
        private Dictionary<string, SvgGlyph> _glyphs;
        private Dictionary<string, SvgKern> _kerning;

        public float Size => _size;
        public float SizeInPoints => _size * 72.0f / _ppi;

        public SvgFontDefn(SvgFont font, float size, float ppi)
        {
            _font = font;
            _size = size;
            _ppi = ppi;
            var face = _font.Children.OfType<SvgFontFace>().First();
            _emScale = _size / face.UnitsPerEm;
        }

        public float Ascent(ISvgRenderer renderer)
        {
            float ascent = _font.Descendants().OfType<SvgFontFace>().First().Ascent;
            float baselineOffset = this.SizeInPoints * (_emScale / _size) * ascent;
            return _ppi / 72f * baselineOffset;
        }

        public IList<SKRect> MeasureCharacters(ISvgRenderer renderer, string text)
        {
            var result = new List<SKRect>();
            using (var path = GetPath(renderer, text, result, false)) { }
            return result;
        }

        public SKSize MeasureString(ISvgRenderer renderer, string text)
        {
            var result = new List<SKRect>();
            using (_ = GetPath(renderer, text, result, true)) { }

            float? firstLeft = null;
            float? lastRight = null;
            foreach (var rect in result.Where(r => !r.IsEmpty))
            {
                firstLeft ??= rect.Left;
                lastRight = rect.Right;
            }

            if (firstLeft == null) return SKSize.Empty;
            return new SKSize(lastRight.Value - firstLeft.Value, Ascent(renderer));
        }

        public void AddStringToPath(ISvgRenderer renderer, SKPath path, string text, SKPoint location)
        {
            var textPath = GetPath(renderer, text, null, false);
            if (textPath.PointCount > 0)
            {
                var translate = SKMatrix.CreateTranslation(location.X, location.Y);
                textPath.Transform(translate);
                path.AddPath(textPath);
            }
        }

        private SKPath GetPath(ISvgRenderer renderer, string text, IList<SKRect> ranges, bool measureSpaces)
        {
            EnsureDictionaries();

            SKRect bounds;
            SvgGlyph glyph;
            SvgKern kern;
            SKPath path;
            SvgGlyph prevGlyph = null;
            float xPos = 0;

            var ascent = Ascent(renderer);
            var result = new SKPath();
            if (string.IsNullOrEmpty(text)) return result;

            for (int i = 0; i < text.Length; i++)
            {
                var charStr = text.Substring(i, 1);
                if (!_glyphs.TryGetValue(charStr, out glyph)) 
                    glyph = _font.Descendants().OfType<SvgMissingGlyph>().FirstOrDefault() ?? new SvgMissingGlyph();
                
                if (prevGlyph != null && _kerning.TryGetValue(prevGlyph.GlyphName + "|" + glyph.GlyphName, out kern))
                {
                    xPos -= kern.Kerning * _emScale;
                }
                
                path = glyph.Path(renderer);
                if (path != null)
                {
                    using (var clonedPath = new SKPath(path))
                    {
                        var scaleMatrix = SKMatrix.CreateScale(_emScale, -1 * _emScale);
                        scaleMatrix = scaleMatrix.PostConcat(SKMatrix.CreateTranslation(xPos, ascent));
                        clonedPath.Transform(scaleMatrix);

                        bounds = clonedPath.Bounds;
                        if (ranges != null)
                        {
                            if (measureSpaces && clonedPath.PointCount == 0)
                            {
                                ranges.Add(new SKRect(xPos, 0, xPos + glyph.HorizAdvX * _emScale, ascent));
                            }
                            else
                            {
                                ranges.Add(bounds);
                            }
                        }
                        if (clonedPath.PointCount > 0) result.AddPath(clonedPath);
                    }
                }

                xPos += glyph.HorizAdvX * _emScale;
                prevGlyph = glyph;
            }

            return result;
        }

        private void EnsureDictionaries()
        {
            if (_glyphs == null) _glyphs = _font.Descendants().OfType<SvgGlyph>().ToDictionary(g => g.Unicode ?? g.GlyphName ?? g.ID ?? "");
            if (_kerning == null) _kerning = _font.Descendants().OfType<SvgKern>().ToDictionary(k => k.Glyph1 + "|" + k.Glyph2);
        }

        public void Dispose()
        {
            _glyphs = null;
            _kerning = null;
        }
    }
}
#endif