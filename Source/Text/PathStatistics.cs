using System;
using SkiaSharp;

namespace Svg
{
    public class PathStatistics : IDisposable
    {
        private readonly SKPathMeasure _measure;
        private readonly float _totalLength;

        public double TotalLength => _totalLength;

        public PathStatistics(SKPath path)
        {
            _measure = new SKPathMeasure(path);
            _totalLength = _measure.Length;
        }

        public void LocationAngleAtOffset(double offset, out SKPoint point, out float angle)
        {
            if (_measure.GetPositionAndTangent((float)offset, out point, out var tangent))
            {
                angle = (float)(Math.Atan2(tangent.Y, tangent.X) * 180.0 / Math.PI);
            }
            else
            {
                angle = 0;
            }
        }

        public bool OffsetOnPath(double offset)
        {
            return offset >= 0 && offset <= _totalLength;
        }

        public void Dispose()
        {
            _measure?.Dispose();
        }
    }
}
