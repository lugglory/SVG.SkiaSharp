using NUnit.Framework;
using SkiaSharp;

namespace Svg.UnitTests
{
    /// <summary>
    /// Test Class of rendering SVGs.
    /// Based on Issue 210.
    /// </summary>
    /// <remarks>
    /// Test use the following embedded resources:
    ///   - Issue210_Metafile\3DSceneSnapshotBIG.svg
    /// </remarks>
    [TestFixture]
    public class BigSceneRenderingTest : SvgTestHelper
    {
        protected override string TestResource { get { return GetFullResourceString("Issue210_Metafile.3DSceneSnapshotBIG.svg"); } }
        protected override int ExpectedSize { get { return 12000; } }

        [Test]
        public void TestBigSceneRendering()
        {
            LoadSvg(GetXMLDocFromResource());
        }
    }
}