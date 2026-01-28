#if !NO_SDC
using SkiaSharp;

namespace Svg
{
    public abstract partial class SvgPathBasedElement : SvgVisualElement
    {
        public override SKRect Bounds
        {
            get
            {
                var path = Path(null);
                if (path == null)
                    return SKRect.Empty;
                if (Transforms == null || Transforms.Count == 0)
                    return path.Bounds;

                using (var clonedPath = new SKPath(path))
                {
                    var matrix = Transforms.GetMatrix();
                    clonedPath.Transform(matrix);
                    return clonedPath.Bounds;
                }
            }
        }
    }
}
#endif