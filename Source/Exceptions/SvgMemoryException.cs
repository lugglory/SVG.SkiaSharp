using System;

namespace Svg.Exceptions
{
    [Serializable]
    public class SvgMemoryException : Exception
    {
        public SvgMemoryException() { }
        public SvgMemoryException(string message) : base(message) { }
        public SvgMemoryException(string message, Exception inner) : base(message, inner) { }
    }
}
