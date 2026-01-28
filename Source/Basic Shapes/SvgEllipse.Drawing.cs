#if !NO_SDC
using SkiaSharp;

namespace Svg
{
    public partial class SvgEllipse : SvgPathBasedElement
    {
        private SKPath _path;

        /// <summary>
        /// Gets the <see cref="SKPath"/> for this element.
        /// </summary>
        public override SKPath Path(ISvgRenderer renderer)
        {
            if (this._path == null || this.IsPathDirty)
            {
                var center = SvgUnit.GetDevicePoint(this.CenterX, this.CenterY, renderer, this);
                var radiusX = this.RadiusX.ToDeviceValue(renderer, UnitRenderingType.Other, this);
                var radiusY = this.RadiusY.ToDeviceValue(renderer, UnitRenderingType.Other, this);

                this._path = new SKPath();
                _path.AddOval(new SKRect(center.X - radiusX, center.Y - radiusY, center.X + radiusX, center.Y + radiusY));
                this.IsPathDirty = false;
            }
            return _path;
        }

        protected override void Render(ISvgRenderer renderer)
        {
            if (this.RadiusX.Value > 0.0f && this.RadiusY.Value > 0.0f)
            {
                base.Render(renderer);
            }
        }
    }
}
#endif