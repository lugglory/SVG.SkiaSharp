using System;
using SkiaSharp;

namespace Svg
{
    public partial class SvgColourServer : SvgPaintServer
    {
        public SvgColourServer()
            : this(SKColors.Black)
        {
        }

        public SvgColourServer(SKColor colour)
        {
            this._colour = colour;
        }

        private SKColor _colour;

        public SKColor Colour
        {
            get { return this._colour; }
            set { this._colour = value; }
        }

        public override string ToString()
        {
            if (this == None)
                return "none";
            else if (this == NotSet)
                return string.Empty;
            else if (this == Inherit)
                return "inherit";

            SKColor c = this.Colour;
            
            // Return the hex value
            return String.Format("#{0:X2}{1:X2}{2:X2}", c.Red, c.Green, c.Blue);
        }

        public override SvgElement DeepCopy()
        {
            return DeepCopy<SvgColourServer>();
        }

        public override SvgElement DeepCopy<T>()
        {
            if (this == None || this == Inherit || this == NotSet)
                return this;

            var newObj = base.DeepCopy<T>() as SvgColourServer;

            newObj.Colour = Colour;
            return newObj;
        }

        public override bool Equals(object obj)
        {
            var objColor = obj as SvgColourServer;
            if (objColor == null)
                return false;

            if ((this == None && obj != None) || (this != None && obj == None) ||
                (this == NotSet && obj != NotSet) || (this != NotSet && obj == NotSet) ||
                (this == Inherit && obj != Inherit) || (this != Inherit && obj == Inherit))
                return false;

            return this.GetHashCode() == objColor.GetHashCode();
        }

        public override int GetHashCode()
        {
            return _colour.GetHashCode();
        }
    }
}