#if !NO_SDC
using System;
using System.Globalization;
using System.Linq;
using SkiaSharp;

namespace Svg.FilterEffects
{
    public partial class SvgColourMatrix : SvgFilterPrimitive
    {
        public override void Process(ImageBuffer buffer)
        {
            var inputImage = buffer[this.Input];
            if (inputImage == null) return;

            float[] matrix = new float[20];
            float value;
            switch (this.Type)
            {
                case SvgColourMatrixType.HueRotate:
                    value = (string.IsNullOrEmpty(this.Values) ? 0 : float.Parse(this.Values, NumberStyles.Any, CultureInfo.InvariantCulture));
                    var cosV = (float)Math.Cos(value);
                    var sinV = (float)Math.Sin(value);
                    matrix = new float[] {
                        0.213f + cosV * 0.787f - sinV * 0.213f, 0.715f - cosV * 0.715f - sinV * 0.715f, 0.072f - cosV * 0.072f + sinV * 0.928f, 0, 0,
                        0.213f - cosV * 0.213f + sinV * 0.143f, 0.715f + cosV * 0.285f + sinV * 0.140f, 0.072f - cosV * 0.072f - sinV * 0.283f, 0, 0,
                        0.213f - cosV * 0.213f - sinV * 0.787f, 0.715f - cosV * 0.715f + sinV * 0.715f, 0.072f + cosV * 0.928f + sinV * 0.072f, 0, 0,
                        0, 0, 0, 1, 0
                    };
                    break;
                case SvgColourMatrixType.LuminanceToAlpha:
                    matrix = new float[] {
                        0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0,
                        0.2125f, 0.7154f, 0.0721f, 0, 0
                    };
                    break;
                case SvgColourMatrixType.Saturate:
                    value = (string.IsNullOrEmpty(this.Values) ? 1 : float.Parse(this.Values, NumberStyles.Any, CultureInfo.InvariantCulture));
                    matrix = new float[] {
                        0.213f + 0.787f * value, 0.715f - 0.715f * value, 0.072f - 0.072f * value, 0, 0,
                        0.213f - 0.213f * value, 0.715f + 0.285f * value, 0.072f - 0.072f * value, 0, 0,
                        0.213f - 0.213f * value, 0.715f - 0.715f * value, 0.072f + 0.928f * value, 0, 0,
                        0, 0, 0, 1, 0
                    };
                    break;
                default: // Matrix
                    var parts = this.Values.Split(new char[] { ' ', '\t', '\n', '\r', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < 20; i++)
                    {
                        if (i < parts.Length)
                            matrix[i] = float.Parse(parts[i], NumberStyles.Any, CultureInfo.InvariantCulture);
                    }
                    break;
            }

            var result = new SKBitmap(inputImage.Width, inputImage.Height);
            using (var canvas = new SKCanvas(result))
            using (var paint = new SKPaint())
            {
                paint.ColorFilter = SKColorFilter.CreateColorMatrix(matrix);
                canvas.DrawBitmap(inputImage, 0, 0, paint);
            }
            buffer[this.Result] = result;
        }
    }
}
#endif