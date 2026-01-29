using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Svg
{
    public interface IFontDefn : IDisposable
    {
        float Size { get; }
        float SizeInPoints { get; }
        void AddStringToPath(ISvgRenderer renderer, SKPath path, string text, SKPoint location);
        float Ascent(ISvgRenderer renderer);
        IList<SKRect> MeasureCharacters(ISvgRenderer renderer, string text);
        SKSize MeasureString(ISvgRenderer renderer, string text);
    }
}
