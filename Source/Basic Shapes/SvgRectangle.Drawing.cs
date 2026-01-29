using System;
using SkiaSharp;

namespace Svg
{
    public partial class SvgRectangle : SvgPathBasedElement
    {
        private SKPath _path;

        /// <summary>
        /// Gets the <see cref="SKPath"/> for this element.
        /// </summary>
        public override SKPath Path(ISvgRenderer renderer)
        {
            if (_path == null || IsPathDirty)
            {
                var width = this.Width.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
                var height = this.Height.ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
                var location = this.Location.ToDeviceValue(renderer, this);
                var rx = CornerRadiusX.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
                var ry = CornerRadiusY.ToDeviceValue(renderer, UnitRenderingType.Vertical, this);

                // Ensure radius is not more than half the dimensions
                rx = Math.Min(rx, width / 2);
                ry = Math.Min(ry, height / 2);

                _path = new SKPath();
                var rect = new SKRect(location.X, location.Y, location.X + width, location.Y + height);
                
                if (rx > 0 || ry > 0)
                {
                    _path.AddRoundRect(rect, rx, ry);
                }
                else
                {
                    _path.AddRect(rect);
                }
                
                this.IsPathDirty = false;
            }
            return _path;
        }

        /// <summary>
        /// Renders the <see cref="SvgElement"/> and contents to the specified <see cref="ISvgRenderer"/> object.
        /// </summary>
        protected override void Render(ISvgRenderer renderer)
        {
            if (Width.Value > 0.0f && Height.Value > 0.0f)
            {
                base.Render(renderer);
            }
        }
    }
}
