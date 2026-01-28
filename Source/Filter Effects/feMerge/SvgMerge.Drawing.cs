#if !NO_SDC
using System.Linq;
using SkiaSharp;

namespace Svg.FilterEffects
{
    public partial class SvgMerge : SvgFilterPrimitive
    {
        public override void Process(ImageBuffer buffer)
        {
            var nodes = this.Children.OfType<SvgMergeNode>().ToList();
            if (!nodes.Any()) return;

            var inputImage = buffer[nodes.First().Input];
            if (inputImage == null) return;

            var result = new SKBitmap(inputImage.Width, inputImage.Height);
            using (var canvas = new SKCanvas(result))
            {
                foreach (var node in nodes)
                {
                    var bmp = buffer[node.Input];
                    if (bmp != null)
                    {
                        canvas.DrawBitmap(bmp, 0, 0);
                    }
                }
            }
            buffer[this.Result] = result;
        }
    }
}
#endif