using System;
using System.ComponentModel;
using SkiaSharp;
using System.Globalization;

namespace Svg
{
    internal class SvgPaintServerFactory : TypeConverter
    {
        private static readonly SvgColourConverter _colourConverter;

        static SvgPaintServerFactory()
        {
            _colourConverter = new SvgColourConverter();
        }

        public static SvgPaintServer Create(string value, SvgDocument document)
        {
            if (value == null)
                return SvgPaintServer.NotSet;

            var colorValue = value.Trim();
            if (string.IsNullOrEmpty(colorValue))
                return SvgPaintServer.NotSet;
            else if (colorValue.Equals("none", StringComparison.OrdinalIgnoreCase))
                return SvgPaintServer.None;
            else if (colorValue.Equals("currentColor", StringComparison.OrdinalIgnoreCase))
                return new SvgDeferredPaintServer("currentColor");
            else if (colorValue.Equals("inherit", StringComparison.OrdinalIgnoreCase))
                return SvgPaintServer.Inherit;
            else if (colorValue.StartsWith("url(", StringComparison.OrdinalIgnoreCase))
            {
                var nextIndex = colorValue.IndexOf(')', 4) + 1;
                if (nextIndex == 0)
                    return new SvgDeferredPaintServer(colorValue + ")", null);

                var id = colorValue.Substring(0, nextIndex);
                colorValue = colorValue.Substring(nextIndex).Trim();
                var fallbackServer = string.IsNullOrEmpty(colorValue) ? null : Create(colorValue, document);

                return new SvgDeferredPaintServer(id, fallbackServer);
            }

            return new SvgColourServer((SKColor)_colourConverter.ConvertFrom(colorValue));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string s)
                return Create(s, (SvgDocument)context);

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value == SvgPaintServer.None || value == SvgPaintServer.Inherit || value == SvgPaintServer.NotSet)
                    return value.ToString();

                if (value is SvgColourServer colourServer)
                {
                    return new SvgColourConverter().ConvertTo(colourServer.Colour, typeof(string));
                }

                if (value is SvgDeferredPaintServer deferred)
                {
                    return deferred.ToString();
                }

                if (value != null)
                {
                    return string.Format(CultureInfo.InvariantCulture, "url(#{0})", ((SvgPaintServer)value).ID);
                }
                else
                {
                    return "none";
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}