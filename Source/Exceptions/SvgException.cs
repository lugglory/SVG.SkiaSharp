using System;

namespace Svg
{
    [Serializable]
    public class SvgException : FormatException
    {
        public SvgException() { }
        public SvgException(string message) : base(message) { }
        public SvgException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class SvgIDException : FormatException
    {
        public SvgIDException() { }
        public SvgIDException(string message) : base(message) { }
        public SvgIDException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class SvgIDExistsException : SvgIDException
    {
        public SvgIDExistsException() { }
        public SvgIDExistsException(string message) : base(message) { }
        public SvgIDExistsException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class SvgIDWrongFormatException : SvgIDException
    {
        public SvgIDWrongFormatException() { }
        public SvgIDWrongFormatException(string message) : base(message) { }
        public SvgIDWrongFormatException(string message, Exception inner) : base(message, inner) { }
    }
}
