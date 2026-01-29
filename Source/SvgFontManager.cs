using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Svg
{
    /// <summary>
    /// Manages access to system fonts and any privately loaded fonts using SkiaSharp.
    /// </summary>
    public class SvgFontManager : IDisposable
    {
        private static readonly string[][] defaultLocalizedFamilyNames = new string[][]
        {
            // Japanese
            new string[]{ "Meiryo", "?°„Ç§?™„Ç™", },
            new string[]{ "MS Gothic", "Ôº?º≥ ?¥„Ç∑?É„ÇØ", },
            new string[]{ "MS Mincho", "Ôº?º≥ ?éÊúù", },
        };

        public static List<string[]> LocalizedFamilyNames { get; private set; } = new List<string[]>();
        public static List<string> PrivateFontPathList { get; private set; } = new List<string>();
        public static List<byte[]> PrivateFontDataList { get; private set; } = new List<byte[]>();

        private readonly List<SKTypeface> _privateTypefaces = new List<SKTypeface>();
        private readonly List<string[]> _localizedFamilyNames = new List<string[]>();

        internal SvgFontManager()
        {
            foreach (var path in PrivateFontPathList)
            {
                var tf = SKTypeface.FromFile(path);
                if (tf != null) _privateTypefaces.Add(tf);
            }

            foreach (var data in PrivateFontDataList)
            {
                using (var ms = new System.IO.MemoryStream(data))
                {
                    var tf = SKTypeface.FromStream(ms);
                    if (tf != null) _privateTypefaces.Add(tf);
                }
            }

            _localizedFamilyNames.AddRange(LocalizedFamilyNames);
            _localizedFamilyNames.AddRange(defaultLocalizedFamilyNames);
        }

        public SKTypeface FindFont(string name, SKFontStyleWeight weight = SKFontStyleWeight.Normal, SKFontStyleWidth width = SKFontStyleWidth.Normal, SKFontStyleSlant slant = SKFontStyleSlant.Upright)
        {
            if (name == null) return null;

            var familyNames = _localizedFamilyNames.Find(f => f.Contains(name, StringComparer.CurrentCultureIgnoreCase))
                              ?? Enumerable.Repeat(name, 1);

            foreach (var familyName in familyNames)
            {
                // Check private fonts first
                var tf = _privateTypefaces.Find(f => f.FamilyName.Equals(familyName, StringComparison.CurrentCultureIgnoreCase));
                if (tf != null) return tf;

                // Check system fonts
                tf = SKTypeface.FromFamilyName(familyName, weight, width, slant);
                if (tf != null && !tf.FamilyName.Equals("Default", StringComparison.OrdinalIgnoreCase))
                    return tf;
            }

            // Fallbacks
            switch (name.ToLower())
            {
                case "serif":
                    return SKTypeface.FromFamilyName("serif", weight, width, slant);
                case "sans-serif":
                    return SKTypeface.FromFamilyName("sans-serif", weight, width, slant);
                case "monospace":
                    return SKTypeface.FromFamilyName("monospace", weight, width, slant);
            }

            return SKTypeface.Default;
        }

        public void Dispose()
        {
            foreach (var tf in _privateTypefaces) tf.Dispose();
            _privateTypefaces.Clear();
        }
    }
}
