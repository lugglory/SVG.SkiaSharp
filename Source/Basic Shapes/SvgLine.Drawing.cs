using SkiaSharp;

namespace Svg
{
    public partial class SvgLine : SvgMarkerElement
    {
        private SKPath _path;

        public override SKPath Path(ISvgRenderer renderer)
        {
            if ((this._path == null || this.IsPathDirty) && base.StrokeWidth > 0)
            {
                var start = new SKPoint(this.StartX.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this),
                    this.StartY.ToDeviceValue(renderer, UnitRenderingType.Vertical, this));
                var end = new SKPoint(this.EndX.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this),
                    this.EndY.ToDeviceValue(renderer, UnitRenderingType.Vertical, this));

                this._path = new SKPath();

                if (renderer != null)
                {
                    this._path.MoveTo(start);
                    this._path.LineTo(end);
                    this.IsPathDirty = false;
                }
                else
                {
                    var radius = base.StrokeWidth / 2;
                    _path.AddCircle(start.X, start.Y, radius);
                    _path.AddCircle(end.X, end.Y, radius);
                }
            }
            return this._path;
        }
    }
}
