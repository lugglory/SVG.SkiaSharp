using System;
using SkiaSharp;
using System.Globalization;
using System.Text;
using System.ComponentModel;

namespace Svg
{
    /// <summary>
    /// Converts string representations of colours into <see cref="SKColor"/> objects.
    /// </summary>
    public class SvgColourConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string colour)
            {
                colour = colour.Trim();

                if (colour.StartsWith("rgb", StringComparison.InvariantCulture))
                {
                    try
                    {
                        int start = colour.IndexOf("(", StringComparison.InvariantCulture) + 1;
                        string[] values = colour.Substring(start, colour.IndexOf(")", StringComparison.InvariantCulture) - start).Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        int alphaValue = 255;
                        if (values.Length > 3)
                        {
                            var alphastring = values[3];
                            if (alphastring.StartsWith(".", StringComparison.InvariantCulture)) alphastring = "0" + alphastring;
                            var alphaDecimal = float.Parse(alphastring, CultureInfo.InvariantCulture);
                            alphaValue = (alphaDecimal <= 1) ? (int)Math.Round(alphaDecimal * 255) : (int)Math.Round(alphaDecimal);
                        }

                        if (values[0].Trim().EndsWith("%", StringComparison.InvariantCulture))
                        {
                            return new SKColor(
                                (byte)Math.Round(255 * float.Parse(values[0].Trim().TrimEnd('%'), NumberStyles.Any, CultureInfo.InvariantCulture) / 100f),
                                (byte)Math.Round(255 * float.Parse(values[1].Trim().TrimEnd('%'), NumberStyles.Any, CultureInfo.InvariantCulture) / 100f),
                                (byte)Math.Round(255 * float.Parse(values[2].Trim().TrimEnd('%'), NumberStyles.Any, CultureInfo.InvariantCulture) / 100f),
                                (byte)alphaValue);
                        }
                        else
                        {
                            return new SKColor(
                                byte.Parse(values[0], CultureInfo.InvariantCulture),
                                byte.Parse(values[1], CultureInfo.InvariantCulture),
                                byte.Parse(values[2], CultureInfo.InvariantCulture),
                                (byte)alphaValue);
                        }
                    }
                    catch
                    {
                        throw new SvgException("Colour is in an invalid format: '" + colour + "'");
                    }
                }
                else if (colour.StartsWith("hsl", StringComparison.InvariantCulture))
                {
                    try
                    {
                        int start = colour.IndexOf("(", StringComparison.InvariantCulture) + 1;
                        string[] values = colour.Substring(start, colour.IndexOf(")", StringComparison.InvariantCulture) - start).Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        float h = float.Parse(values[0], CultureInfo.InvariantCulture);
                        float s = float.Parse(values[1].TrimEnd('%'), CultureInfo.InvariantCulture) / 100f;
                        float l = float.Parse(values[2].TrimEnd('%'), CultureInfo.InvariantCulture) / 100f;
                        return SKColor.FromHsl(h, s * 100f, l * 100f);
                    }
                    catch
                    {
                        throw new SvgException("Colour is in an invalid format: '" + colour + "'");
                    }
                }
                else if (colour.StartsWith("#", StringComparison.InvariantCulture))
                {
                    if (SKColor.TryParse(colour, out var result)) return result;
                }
                
                // Named colors
                if (SKColor.TryParse(colour, out var namedResult)) return namedResult;

                // Handle 'currentColor', etc. via SvgPaintServer if needed, 
                // but this converter returns SKColor.
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is SKColor c)
            {
                return $"#{c.Red:X2}{c.Green:X2}{c.Blue:X2}";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}