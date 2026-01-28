using System;
using SkiaSharp;
using Moq;
using NUnit.Framework;

namespace Svg.UnitTests
{
    public class SvgVisualElementTests
    {
        [Test]
        public void TestSmoothingModeRestoreForGraphics()
        {
            var visualElementMock = new Mock<SvgVisualElement>();
            visualElementMock.Setup(_ => _.Attributes).CallBase();

            var visualElement = visualElementMock.Object;
            visualElement.ShapeRendering = SvgShapeRendering.Auto;

            using (var bitmap = new SKBitmap(1, 1))
            using (var canvas = new SKCanvas(bitmap))
            using (var renderer = SvgRenderer.FromCanvas(canvas))
            {
                renderer.SmoothingMode = true;

                visualElement.RenderElement(renderer);

                Assert.That(renderer.SmoothingMode, Is.True);
            }
        }

        [Test]
        public void TestSmoothingModeRestoreForCustomRenderer()
        {
            var visualElementMock = new Mock<SvgVisualElement>();
            visualElementMock.Setup(_ => _.Attributes).CallBase();

            var visualElement = visualElementMock.Object;
            visualElement.ShapeRendering = SvgShapeRendering.Auto;

            var renderer = Mock.Of<ISvgRenderer>(_ => _.Transform == SKMatrix.CreateIdentity());

            renderer.SmoothingMode = true;

            visualElement.RenderElement(renderer);

            Assert.That(renderer.SmoothingMode, Is.True);
        }
    }
}