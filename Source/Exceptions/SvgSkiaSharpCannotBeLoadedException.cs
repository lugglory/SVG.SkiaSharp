using System;
#if !NET8_0_OR_GREATER
using System.Runtime.Serialization;
#endif

namespace Svg
{
    [Serializable]
    public class SvgSkiaSharpCannotBeLoadedException : Exception
    {
const string skiaErrorMsg = "Cannot initialize SkiaSharp libraries. This is likely to be caused by missing native SkiaSharp libraries. Please refer to the documentation for more details.";

        public SvgSkiaSharpCannotBeLoadedException() : base(skiaErrorMsg) { }
        public SvgSkiaSharpCannotBeLoadedException(string message) : base(message) { }
        public SvgSkiaSharpCannotBeLoadedException(Exception inner) : base(skiaErrorMsg, inner) {}
        public SvgSkiaSharpCannotBeLoadedException(string message, Exception inner) : base(message, inner) { }

#if !NET8_0_OR_GREATER
        protected SvgSkiaSharpCannotBeLoadedException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
#endif
    }
}
