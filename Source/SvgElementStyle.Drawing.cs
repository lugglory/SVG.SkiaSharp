using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Svg
{
    public partial class SvgElement
    {
        internal IFontDefn GetFont(ISvgRenderer renderer, SvgFontManager fontManager)
        {
            float fontSize;
            var fontSizeUnit = this.FontSize;
            if (fontSizeUnit == SvgUnit.None || fontSizeUnit == SvgUnit.Empty)
            {
                fontSize = new SvgUnit(SvgUnitType.Em, 1.0f).ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
            }
            else
            {
                fontSize = fontSizeUnit.ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
            }

            var manager = fontManager ?? this.OwnerDocument?.FontManager ?? new SvgFontManager();
            var familyResult = ValidateFontFamily(this.FontFamily, this.OwnerDocument, manager);
            var sFaces = familyResult as IEnumerable<SvgFontFace>;
            var ppi = this.OwnerDocument?.Ppi ?? SvgDocument.PointsPerInch;

            if (sFaces == null)
            {
                var weight = SKFontStyleWeight.Normal;
                switch (this.FontWeight)
                {
                    case SvgFontWeight.Bold:
                    case SvgFontWeight.W600:
                    case SvgFontWeight.W700:
                    case SvgFontWeight.W800:
                    case SvgFontWeight.W900:
                        weight = SKFontStyleWeight.Bold;
                        break;
                    case SvgFontWeight.Bolder:
                        weight = SKFontStyleWeight.ExtraBold;
                        break;
                    case SvgFontWeight.Lighter:
                        weight = SKFontStyleWeight.Light;
                        break;
                }

                var slant = SKFontStyleSlant.Upright;
                switch (this.FontStyle)
                {
                    case SvgFontStyle.Italic:
                    case SvgFontStyle.Oblique:
                        slant = SKFontStyleSlant.Italic;
                        break;
                }

                SKTypeface tf = null;
                if (familyResult is SKTypeface resultTf)
                {
                    // If we already found a typeface, we might want to try to get one with matching style
                    tf = SKTypeface.FromFamilyName(resultTf.FamilyName, weight, SKFontStyleWidth.Normal, slant);
                }
                else if (familyResult is string familyName)
                {
                    tf = manager.FindFont(familyName, weight, SKFontStyleWidth.Normal, slant);
                }

                if (tf == null) tf = SKTypeface.Default;

                return new SkiaFontDefn(tf, fontSize, ppi);
            }
            else
            {
                var font = sFaces.First().Parent as SvgFont;
                if (font == null)
                {
                    var uri = sFaces.First().Descendants().OfType<SvgFontFaceUri>().First().ReferencedElement;
                    font = OwnerDocument.IdManager.GetElementById(uri) as SvgFont;
                }
                return new SvgFontDefn(font, fontSize, ppi);
            }
        }

        public static object ValidateFontFamily(string fontFamilyList, SvgDocument doc, SvgFontManager fontManager)
        {
            var fontParts = (fontFamilyList ?? string.Empty).Split(new char[] { ',' }).Select(fontName => fontName.Trim(new char[] { '"', ' ', '\'' }));

            foreach (var f in fontParts)
            {
                if (doc != null && doc.FontDefns().TryGetValue(f, out var fontFaces))
                    return fontFaces;

                var tf = fontManager.FindFont(f);
                if (tf != null && !tf.FamilyName.Equals("Default", StringComparison.OrdinalIgnoreCase))
                    return tf;
            }

            return SKTypeface.Default;
        }
    }
}
