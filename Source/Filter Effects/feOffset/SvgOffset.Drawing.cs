using SkiaSharp;

namespace Svg.FilterEffects
{
    public partial class SvgOffset : SvgFilterPrimitive
    {
        public override void Process(ImageBuffer buffer)
        {
            var inputImage = buffer[this.Input];
            if (inputImage == null) return;

            var dx = this.Dx.ToDeviceValue(null, UnitRenderingType.Horizontal, this);
            var dy = this.Dy.ToDeviceValue(null, UnitRenderingType.Vertical, this);
            
            var vector = buffer.Transform.MapVector(new SKPoint(dx, dy));

            var result = new SKBitmap(inputImage.Width, inputImage.Height);
            using (var canvas = new SKCanvas(result))
            using (var paint = new SKPaint())
            {
                paint.ImageFilter = SKImageFilter.CreateOffset(vector.X, vector.Y);
                canvas.DrawBitmap(inputImage, 0, 0, paint);
            }
            buffer[this.Result] = result;
        }
    }
}
