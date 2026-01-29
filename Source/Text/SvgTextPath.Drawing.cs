using SkiaSharp;

namespace Svg
{
    public partial class SvgTextPath : SvgTextBase
    {
        protected override SKPath GetBaselinePath(ISvgRenderer renderer)
        {
            if (this.OwnerDocument.IdManager.GetElementById(this.ReferencedPath) is SvgVisualElement path)
            {
                var pathData = new SKPath(path.Path(renderer));
                if (path.Transforms != null && path.Transforms.Count > 0)
                {
                    var matrix = path.Transforms.GetMatrix();
                    pathData.Transform(matrix);
                }
                return pathData;
            }
            return null;
        }

        protected override float GetAuthorPathLength()
        {
            if (this.OwnerDocument.IdManager.GetElementById(this.ReferencedPath) is SvgPath path)
            {
                return path.PathLength;
            }
            return 0;
        }
    }
}
