#if !NO_SDC
using System;
using System.Linq;
using SkiaSharp;

namespace Svg
{
    public partial struct SvgUnit
    {
        /// <summary>
        /// Converts the current unit to one that can be used at render time.
        /// </summary>
        /// <returns>The representation of the current unit in a device value (usually pixels).</returns>
        public float ToDeviceValue(ISvgRenderer renderer, UnitRenderingType renderType, SvgElement owner)
        {
            // If it's already been calculated
            if (this._deviceValue.HasValue)
            {
                return this._deviceValue.Value;
            }

            if (this._value == 0.0f)
            {
                this._deviceValue = 0.0f;
                return this._deviceValue.Value;
            }

            const float cmInInch = 2.54f;
            var ppi = owner?.OwnerDocument?.Ppi ?? SvgDocument.PointsPerInch;

            var type = this.Type;
            var value = this.Value;

            float points;

            switch (type)
            {
                case SvgUnitType.Em:
                    // Font manager logic simplified for now
                    using (var currFont = GetFont(renderer, owner, null))
                    {
                        if (currFont == null)
                        {
                            points = (float)(value * 9);
                            _deviceValue = (points / 72.0f) * ppi;
                        }
                        else
                        {
                            _deviceValue = value * (currFont.SizeInPoints / 72.0f) * ppi;
                        }
                    }
                    break;
                case SvgUnitType.Ex:
                    using (var currFont = GetFont(renderer, owner, null))
                    {
                        if (currFont == null)
                        {
                            points = (float)(value * 9);
                            _deviceValue = (points * 0.5f / 72.0f) * ppi;
                        }
                        else
                        {
                            _deviceValue = value * 0.5f * (currFont.SizeInPoints / 72.0f) * ppi;
                        }
                    }
                    break;
                case SvgUnitType.Centimeter:
                    _deviceValue = (float)((value / cmInInch) * ppi);
                    break;
                case SvgUnitType.Inch:
                    _deviceValue = value * ppi;
                    break;
                case SvgUnitType.Millimeter:
                    _deviceValue = (float)((value / 10) / cmInInch) * ppi;
                    break;
                case SvgUnitType.Pica:
                    _deviceValue = ((value * 12) / 72) * ppi;
                    break;
                case SvgUnitType.Point:
                    _deviceValue = (value / 72) * ppi;
                    break;
                case SvgUnitType.Pixel:
                    _deviceValue = value;
                    break;
                case SvgUnitType.User:
                    _deviceValue = value;
                    break;
                case SvgUnitType.Percentage:
                    var boundable = (renderer == null
                        ? (owner == null ? null : owner.OwnerDocument)
                        : renderer.GetBoundable());
                    if (boundable == null)
                    {
                        _deviceValue = value;
                        break;
                    }

                    SKSize size = boundable.Size;

                    switch (renderType)
                    {
                        case UnitRenderingType.Horizontal:
                            _deviceValue = (float)(size.Width / 100.0 * value);
                            break;
                        case UnitRenderingType.HorizontalOffset:
                            _deviceValue = (float)(size.Width / 100.0 * value) + boundable.Location.X;
                            break;
                        case UnitRenderingType.Vertical:
                            _deviceValue = (float)(size.Height / 100.0 * value);
                            break;
                        case UnitRenderingType.VerticalOffset:
                            _deviceValue = (float)(size.Height / 100.0 * value) + boundable.Location.Y;
                            break;
                        case UnitRenderingType.Other:
                            if (owner.OwnerDocument != null && owner.OwnerDocument.ViewBox.Width != 0 &&
                                owner.OwnerDocument.ViewBox.Height != 0)
                            {
                                _deviceValue =
                                    (float)(Math.Sqrt(Math.Pow(owner.OwnerDocument.ViewBox.Width, 2) +
                                                      Math.Pow(owner.OwnerDocument.ViewBox.Height, 2)) / Math.Sqrt(2) *
                                        value / 100.0);
                            }
                            else
                                _deviceValue = (float)(Math.Sqrt(Math.Pow(size.Width, 2) + Math.Pow(size.Height, 2)) /
                                    Math.Sqrt(2) * value / 100.0);
                            break;
                    }
                    break;
                default:
                    _deviceValue = value;
                    break;
            }

            return this._deviceValue.HasValue ? this._deviceValue.Value : 0f;
        }

        public static implicit operator float(SvgUnit value)
        {
            return value.ToDeviceValue(null, UnitRenderingType.Other, null);
        }

        private IFontDefn GetFont(ISvgRenderer renderer, SvgElement owner, SvgFontManager fontManager)
        {
            var visual = owner?.ParentsAndSelf.OfType<SvgVisualElement>().FirstOrDefault();
            // In a real refactor, SvgFontManager might be needed or passed differently
            // but let's assume we can get it from document or it's not strictly needed here if we simplify
            return null; // Placeholder, need to check SvgVisualElement.GetFont
        }

        public static SKPoint GetDevicePoint(SvgUnit x, SvgUnit y, ISvgRenderer renderer, SvgElement owner)
        {
            return new SKPoint(x.ToDeviceValue(renderer, UnitRenderingType.Horizontal, owner),
                y.ToDeviceValue(renderer, UnitRenderingType.Vertical, owner));
        }
        public static SKPoint GetDevicePointOffset(SvgUnit x, SvgUnit y, ISvgRenderer renderer, SvgElement owner)
        {
            return new SKPoint(x.ToDeviceValue(renderer, UnitRenderingType.HorizontalOffset, owner),
                y.ToDeviceValue(renderer, UnitRenderingType.VerticalOffset, owner));
        }

        public static SKSize GetDeviceSize(SvgUnit width, SvgUnit height, ISvgRenderer renderer, SvgElement owner)
        {
            return new SKSize(width.ToDeviceValue(renderer, UnitRenderingType.Horizontal, owner),
                height.ToDeviceValue(renderer, UnitRenderingType.Vertical, owner));
        }
    }
}
#endif