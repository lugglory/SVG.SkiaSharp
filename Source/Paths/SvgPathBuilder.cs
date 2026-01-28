using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using SkiaSharp;
using Svg.Pathing;

namespace Svg
{
    public static class PointFExtensions
    {
        public static string ToSvgString(this float value)
        {
            return value.ToString("G7", CultureInfo.InvariantCulture);
        }

        public static string ToSvgString(this SKPoint p)
        {
            return $"{p.X.ToSvgString()} {p.Y.ToSvgString()}";
        }
    }

    public class SvgPathBuilder : TypeConverter
    {
        public static SvgPathSegmentList Parse(ReadOnlySpan<char> path)
        {
            var segments = new SvgPathSegmentList();

            try
            {
                var pathTrimmed = path.TrimEnd();
                var commandStart = 0;
                var pathLength = pathTrimmed.Length;

                for (var i = 0; i < pathLength; ++i)
                {
                    var currentChar = pathTrimmed[i];
                    if (char.IsLetter(currentChar) && currentChar != 'e' && currentChar != 'E')
                    {
                        var start = commandStart;
                        var length = i - commandStart;
                        var command = pathTrimmed.Slice(start, length).Trim();
                        commandStart = i;

                        if (command.Length > 0)
                        {
                            var commandSetTrimmed = pathTrimmed.Slice(start, length).Trim();
                            var state = new CoordinateParserState(ref commandSetTrimmed);
                            CreatePathSegment(commandSetTrimmed[0], segments, ref state, commandSetTrimmed);
                        }

                        if (pathLength == i + 1)
                        {
                            var commandSetTrimmed = pathTrimmed.Slice(i, 1).Trim();
                            var state = new CoordinateParserState(ref commandSetTrimmed);
                            CreatePathSegment(commandSetTrimmed[0], segments, ref state, commandSetTrimmed);
                        }
                    }
                    else if (pathLength == i + 1)
                    {
                        var start = commandStart;
                        var length = i - commandStart + 1;
                        var command = pathTrimmed.Slice(start, length).Trim();

                        if (command.Length > 0)
                        {
                            var commandSetTrimmed = pathTrimmed.Slice(start, length).Trim();
                            var state = new CoordinateParserState(ref commandSetTrimmed);
                            CreatePathSegment(commandSetTrimmed[0], segments, ref state, commandSetTrimmed);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Trace.TraceError("Error parsing path \"{0}\": {1}", path.ToString(), exc.Message);
            }

            return segments;
        }

        private static void CreatePathSegment(char command, SvgPathSegmentList segments, ref CoordinateParserState state, ReadOnlySpan<char> chars)
        {
            var isRelative = char.IsLower(command);

            switch (command)
            {
                case 'M':
                case 'm':
                    {
                        if (CoordinateParser.TryGetFloat(out var coords0, chars, ref state) 
                         && CoordinateParser.TryGetFloat(out var coords1, chars, ref state))
                        {
                            segments.Add(new SvgMoveToSegment(isRelative, new SKPoint(coords0, coords1)));
                        }
                        while (CoordinateParser.TryGetFloat(out coords0, chars, ref state) 
                            && CoordinateParser.TryGetFloat(out coords1, chars, ref state))
                        {
                            segments.Add(new SvgLineSegment(isRelative, new SKPoint(coords0, coords1)));
                        }
                    }
                    break;
                case 'A':
                case 'a':
                    {
                        while (CoordinateParser.TryGetFloat(out var coords0, chars, ref state) 
                            && CoordinateParser.TryGetFloat(out var coords1, chars, ref state) 
                            && CoordinateParser.TryGetFloat(out var coords2, chars, ref state) 
                            && CoordinateParser.TryGetBool(out var size, chars, ref state) 
                            && CoordinateParser.TryGetBool(out var sweep, chars, ref state) 
                            && CoordinateParser.TryGetFloat(out var coords3, chars, ref state) 
                            && CoordinateParser.TryGetFloat(out var coords4, chars, ref state))
                        {
                            segments.Add(new SvgArcSegment(coords0, coords1, coords2,
                                    size ? SvgArcSize.Large : SvgArcSize.Small,
                                    sweep ? SvgArcSweep.Positive : SvgArcSweep.Negative,
                                    isRelative, new SKPoint(coords3, coords4)));
                        }
                    }
                    break;
                case 'L':
                case 'l':
                    {
                        while (CoordinateParser.TryGetFloat(out var coords0, chars, ref state) 
                            && CoordinateParser.TryGetFloat(out var coords1, chars, ref state))
                        {
                            segments.Add(new SvgLineSegment(isRelative, new SKPoint(coords0, coords1)));
                        }
                    }
                    break;
                case 'H':
                case 'h':
                    {
                        while (CoordinateParser.TryGetFloat(out var coords0, chars, ref state))
                        {
                            segments.Add(new SvgLineSegment(isRelative, new SKPoint(coords0, float.NaN)));
                        }
                    }
                    break;
                case 'V':
                case 'v':
                    {
                        while (CoordinateParser.TryGetFloat(out var coords0, chars, ref state))
                        {
                            segments.Add(new SvgLineSegment(isRelative, new SKPoint(float.NaN, coords0)));
                        }
                    }
                    break;
                case 'Q':
                case 'q':
                    {
                        while (CoordinateParser.TryGetFloat(out var coords0, chars, ref state) 
                            && CoordinateParser.TryGetFloat(out var coords1, chars, ref state) 
                            && CoordinateParser.TryGetFloat(out var coords2, chars, ref state) 
                            && CoordinateParser.TryGetFloat(out var coords3, chars, ref state))
                        {
                            segments.Add(new SvgQuadraticCurveSegment(isRelative,
                                    new SKPoint(coords0, coords1), new SKPoint(coords2, coords3)));
                        }
                    }
                    break;
                case 'T':
                case 't':
                    {
                        while (CoordinateParser.TryGetFloat(out var coords0, chars, ref state) 
                            && CoordinateParser.TryGetFloat(out var coords1, chars, ref state))
                        {
                            segments.Add(new SvgQuadraticCurveSegment(isRelative, new SKPoint(coords0, coords1)));
                        }
                    }
                    break;
                case 'C':
                case 'c':
                    {
                        while (CoordinateParser.TryGetFloat(out var coords0, chars, ref state) 
                            && CoordinateParser.TryGetFloat(out var coords1, chars, ref state) 
                            && CoordinateParser.TryGetFloat(out var coords2, chars, ref state) 
                            && CoordinateParser.TryGetFloat(out var coords3, chars, ref state) 
                            && CoordinateParser.TryGetFloat(out var coords4, chars, ref state) 
                            && CoordinateParser.TryGetFloat(out var coords5, chars, ref state))
                        {
                            segments.Add(new SvgCubicCurveSegment(isRelative,
                                    new SKPoint(coords0, coords1), new SKPoint(coords2, coords3), new SKPoint(coords4, coords5)));
                        }
                    }
                    break;
                case 'S':
                case 's':
                    {
                        while (CoordinateParser.TryGetFloat(out var coords0, chars, ref state) 
                            && CoordinateParser.TryGetFloat(out var coords1, chars, ref state) 
                            && CoordinateParser.TryGetFloat(out var coords2, chars, ref state) 
                            && CoordinateParser.TryGetFloat(out var coords3, chars, ref state))
                        {
                            segments.Add(new SvgCubicCurveSegment(isRelative,
                                    new SKPoint(coords0, coords1), new SKPoint(coords2, coords3)));
                        }
                    }
                    break;
                case 'Z':
                case 'z':
                    {
                        segments.Add(new SvgClosePathSegment(isRelative));
                    }
                    break;
            }
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string s)
                return Parse(s.AsSpan());

            return base.ConvertFrom(context, culture, value);
        }
    }
}