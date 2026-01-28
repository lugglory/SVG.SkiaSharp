#if !NO_SDC
using SkiaSharp;
using Svg.Pathing;

namespace Svg
{
    public partial class SvgPath : SvgMarkerElement
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

                var pathData = PathData;
                if (pathData != null && pathData.Count > 0 && pathData.First is SvgMoveToSegment)
                {
                    var start = SKPoint.Empty;
                    foreach (var segment in pathData)
                        start = segment.AddToPath(_path, start, pathData);

                    if (_path.PointCount == 0)
                    {
                        if (pathData.Count > 0)
                        {
                            var segment = pathData.Last;
                            _path.MoveTo(segment.End);
                            _path.LineTo(segment.End);
                            Fill = SvgPaintServer.None;
                            Stroke = SvgPaintServer.None;
                        }
                        else
                            _path = null;
                    }
                }

                if (renderer != null)
                    IsPathDirty = false;
            }
            return _path;
        }
    }
}
#endif