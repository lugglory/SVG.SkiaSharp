using System;
using SkiaSharp;
using System.Diagnostics;

namespace Svg
{
    public partial class SvgPolyline : SvgPolygon
    {
        private SKPath _path;

        public override SKPath Path(ISvgRenderer renderer)
        {
            if (_path == null || this.IsPathDirty)
            {
                _path = new SKPath();

                try
                {
                    for (int i = 0; (i + 1) < Points.Count; i += 2)
                    {
                        var endPoint = new SKPoint(Points[i].ToDeviceValue(renderer, UnitRenderingType.Horizontal, this),
                            Points[i + 1].ToDeviceValue(renderer, UnitRenderingType.Vertical, this));

                        if (renderer == null)
                        {
                            var radius = base.StrokeWidth / 2;
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
                catch (Exception exc)
                {
                    Trace.TraceError("Error rendering points: " + exc.Message);
                }
                if (renderer != null)
                    this.IsPathDirty = false;
            }
            return _path;
        }
    }
}
