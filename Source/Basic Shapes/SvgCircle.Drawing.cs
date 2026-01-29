using SkiaSharp;

namespace Svg
{
    public partial class SvgCircle : SvgPathBasedElement
    {
        private SKPath _path;

        /// <summary>
        /// Gets the <see cref="SKPath"/> representing this element.
        /// </summary>
        public override SKPath Path(ISvgRenderer renderer)
        {
            if (this._path == null || this.IsPathDirty)
            {
                _path = new SKPath();
                var center = this.Center.ToDeviceValue(renderer, this);
                var radius = this.Radius.ToDeviceValue(renderer, UnitRenderingType.Other, this);
                _path.AddCircle(center.X, center.Y, radius);
                this.IsPathDirty = false;
            }
            return _path;
        }

        /// <summary>
        /// Renders the circle using the specified <see cref="ISvgRenderer"/> object.
        /// </summary>
        /// <param name="renderer">The renderer object.</param>
        protected override void Render(ISvgRenderer renderer)
        {
            if (this.Radius.Value > 0.0f)
            {
                base.Render(renderer);
            }
        }
    }
}
