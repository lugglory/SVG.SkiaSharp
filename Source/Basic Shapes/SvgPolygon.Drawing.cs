using System.Diagnostics;
using SkiaSharp;

namespace Svg
{
    public partial class SvgPolygon : SvgMarkerElement
    {
        private SKPath _path;

        public override SKPath Path(ISvgRenderer renderer)
        {
            if (this._path == null || this.IsPathDirty)
            {
                this._path = new SKPath();

                try
                {
                    var points = this.Points;
                    for (int i = 0; (i + 1) < points.Count; i += 2)
                    {
                        var endPoint = SvgUnit.GetDevicePoint(points[i], points[i + 1], renderer, this);

                        if (renderer == null)
                        {
                            var radius = base.StrokeWidth * 2;
                            _path.AddCircle(endPoint.X, endPoint.Y, radius);
                            continue;
                        }

                        if (i == 0)
                        {
                            _path.MoveTo(endPoint);
                        }
                        else
                        {
                            _path.LineTo(endPoint);
                        }
                    }
                }
                catch
                {
                    Trace.TraceError("Error parsing points");
                }

                this._path.Close();
                if (renderer != null)
                    this.IsPathDirty = false;
            }
            return this._path;
        }
    }
}
