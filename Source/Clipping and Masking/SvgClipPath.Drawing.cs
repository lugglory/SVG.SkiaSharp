using SkiaSharp;

namespace Svg
{
    public partial class SvgClipPath : SvgElement
    {
        private SKPath _path;

        /// <summary>
        /// Gets this <see cref="SvgClipPath"/>'s path to be used as a clipping path.
        /// </summary>
        public SKPath GetClipPath(SvgVisualElement owner, ISvgRenderer renderer)
        {
            if (_path == null || IsPathDirty)
            {
                _path = new SKPath();

                foreach (var element in Children)
                    CombinePaths(_path, element, renderer);

                IsPathDirty = false;
            }

            var result = _path;
            if (ClipPathUnits == SvgCoordinateUnits.ObjectBoundingBox)
            {
                result = new SKPath(_path);
                var bounds = owner.Bounds;
                var transform = SKMatrix.CreateScale(bounds.Width, bounds.Height);
                transform = transform.PostConcat(SKMatrix.CreateTranslation(bounds.Left, bounds.Top));
                result.Transform(transform);
            }

            return result;
        }

        private void CombinePaths(SKPath path, SvgElement element, ISvgRenderer renderer)
        {
            if (element is SvgVisualElement graphicsElement)
            {
                var childPath = graphicsElement.Path(renderer);
                if (childPath != null)
                {
                    path.FillType = graphicsElement.ClipRule == SvgClipRule.NonZero ? SKPathFillType.Winding : SKPathFillType.EvenOdd;

                    if (graphicsElement.Transforms != null)
                    {
                        var matrix = graphicsElement.Transforms.GetMatrix();
                        childPath.Transform(matrix);
                    }

                    if (childPath.PointCount > 0)
                        path.AddPath(childPath);
                }
            }

            foreach (var child in element.Children)
                CombinePaths(path, child, renderer);
        }

        protected override void Render(ISvgRenderer renderer)
        {
            // Do nothing
        }
    }
}
