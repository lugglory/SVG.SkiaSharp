using SkiaSharp;

namespace Svg.FilterEffects
{
    public partial class SvgGaussianBlur : SvgFilterPrimitive
    {
        public override void Process(ImageBuffer buffer)
        {
            var inputImage = buffer[this.Input];
            if (inputImage == null) return;

            float stdX = 0f;
            float stdY = 0f;
            if (StdDeviation.Count == 1)
            {
                stdX = StdDeviation[0];
                stdY = stdX;
            }
            else if (StdDeviation.Count == 2)
            {
                stdX = StdDeviation[0];
                stdY = StdDeviation[1];
            }

            if (stdX <= 0f && stdY <= 0f)
            {
                buffer[this.Result] = inputImage;
                return;
            }

            var result = new SKBitmap(inputImage.Width, inputImage.Height);
            using (var canvas = new SKCanvas(result))
            using (var paint = new SKPaint())
            {
                paint.ImageFilter = SKImageFilter.CreateBlur(stdX, stdY);
                canvas.DrawBitmap(inputImage, 0, 0, paint);
            }

            buffer[this.Result] = result;
        }
    }
}
