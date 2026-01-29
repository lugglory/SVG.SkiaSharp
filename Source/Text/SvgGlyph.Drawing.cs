using SkiaSharp;
using Svg.Pathing;

namespace Svg
{
    public partial class SvgGlyph : SvgPathBasedElement
    {
        private SKPath _path;

        /// <summary>
        /// Gets the <see cref="SKPath"/> for this element.
        /// </summary>
        public override SKPath Path(ISvgRenderer renderer)
        {
            if (_path == null || IsPathDirty)
            {
                _path = new SKPath();

                if (PathData != null)
                {
                    var start = SKPoint.Empty;
                    foreach (var segment in PathData)
                        start = segment.AddToPath(_path, start, PathData);
                }

                IsPathDirty = false;
            }
            return _path;
        }
    }
}
