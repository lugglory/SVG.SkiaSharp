#if !NO_SDC
using System;
using System.Linq;
using SkiaSharp;

namespace Svg.FilterEffects
{
    public partial class SvgFilter : SvgElement
    {
        protected override void Render(ISvgRenderer renderer)
        {
            RenderChildren(renderer);
        }

        private SKMatrix GetTransform(SvgVisualElement element)
        {
            var transformMatrix = SKMatrix.CreateIdentity();
            if (element.Transforms != null)
            {
                var matrix = element.Transforms.GetMatrix();
                transformMatrix = transformMatrix.PostConcat(matrix);
            }
            return transformMatrix;
        }

        private SKRect GetPathBounds(SvgVisualElement element, ISvgRenderer renderer, SKMatrix transform)
        {
            var bounds = (element is SvgGroup) ? element.Path(renderer).Bounds : element.Bounds;
            return transform.MapRect(bounds);
        }

        public void ApplyFilter(SvgVisualElement element, ISvgRenderer renderer, Action<ISvgRenderer> renderMethod)
        {
            var transform = GetTransform(element);
            var bounds = GetPathBounds(element, renderer, transform);
            if (bounds.Width == 0f || bounds.Height == 0f)
                return;

            var inflate = 0.5f;
            using (var buffer = new ImageBuffer(bounds, inflate, renderer, renderMethod) { Transform = transform })
            {
                foreach (var primitive in Children.OfType<SvgFilterPrimitive>())
                    primitive.Process(buffer);

                var bufferImg = buffer.Buffer;
                if (bufferImg == null) return;

                var imgDraw = bounds;
                imgDraw.Inflate(inflate * bounds.Width, inflate * bounds.Height);

                renderer.Save();
                try
                {
                    renderer.SetClip(imgDraw);
                    using (var skImage = SKImage.FromBitmap(bufferImg))
                    {
                        renderer.DrawImage(skImage, imgDraw, new SKRect(bounds.Left, bounds.Top, bounds.Left + imgDraw.Width, bounds.Top + imgDraw.Height));
                    }
                }
                finally
                {
                    renderer.Restore();
                }
            }
        }
    }
}
#endif