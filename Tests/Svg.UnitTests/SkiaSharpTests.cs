using NUnit.Framework;

namespace Svg.UnitTests
{

    /// <summary>
    /// Simple testing of the SkiaSharp capabilities, just to ensure whether the checks are implemented correctly
    /// </summary>
    [TestFixture]
    public class SkiaSharpTests : SvgTestHelper
    {
        [Test]
        public void SkiaSharp_QueryCapability_YieldsTrue()
        {
            Assert.True(SvgDocument.SystemIsSkiaSharpCapable(), "The SkiaSharp check should yield true, please validate SkiaSharp capabilities");
        }

        [Test]
        public void SkiaSharp_EnsureCapability_YieldsNoError()
        {
            SvgDocument.EnsureSystemIsSkiaSharpCapable(); //This call is a void, if everything works as expected, we won't get an exception and the test will finish.
        }
    }
}
