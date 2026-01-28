#if !NO_SDC
using System;
using System.ComponentModel;
using SkiaSharp;
using System.Xml;
using Svg.Exceptions;

namespace Svg
{
    public partial class SvgDocument : SvgFragment, ITypeDescriptorContext
    {
        /// <summary>
        /// Skip check whether the SkiaSharp can be loaded.
        /// </summary>
        public static bool SkipGdiPlusCapabilityCheck { get; set; }

        internal SvgFontManager FontManager { get; private set; }

        /// <summary>
        /// Validate whether the system has SkiaSharp capabilities.
        /// </summary>
        public static bool SystemIsGdiPlusCapable()
        {
            return true;
        }

        public static void EnsureSystemIsGdiPlusCapable()
        {
            // No-op for SkiaSharp
        }

        public static SKBitmap OpenAsBitmap(string path)
        {
            var doc = Open(path);
            return doc?.Draw();
        }

        public static SKBitmap OpenAsBitmap(XmlDocument document)
        {
            var doc = Open(document);
            return doc?.Draw();
        }

        private void Draw(ISvgRenderer renderer, ISvgBoundable boundable)
        {
            // SvgFontManager might need SkiaSharp implementation later
            renderer.SetBoundable(boundable);
            Render(renderer);
        }

        /// <summary>
        /// Renders the <see cref="SvgDocument"/> to the specified <see cref="ISvgRenderer"/>.
        /// </summary>
        public void Draw(ISvgRenderer renderer)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            this.Draw(renderer, this);
        }

        /// <summary>
        /// Renders the <see cref="SvgDocument"/> to the specified <see cref="SKCanvas"/>.
        /// </summary>
        public void Draw(SKCanvas canvas)
        {
            if (canvas == null)
                throw new ArgumentNullException(nameof(canvas));

            using (var renderer = SvgRenderer.FromCanvas(canvas))
            {
                this.Draw(renderer, this);
            }
        }

        /// <summary>
        /// Renders the <see cref="SvgDocument"/> and returns the image as a <see cref="SKBitmap"/>.
        /// </summary>
        public virtual SKBitmap Draw()
        {
            var size = GetDimensions();
            if (size.Width <= 0 || size.Height <= 0)
                return null;

            SKBitmap bitmap = null;
            try
            {
                bitmap = new SKBitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));
                this.Draw(bitmap);
            }
            catch
            {
                bitmap?.Dispose();
                throw;
            }

            return bitmap;
        }

        /// <summary>
        /// Renders the <see cref="SvgDocument"/> into a given <see cref="SKBitmap"/>.
        /// </summary>
        public virtual void Draw(SKBitmap bitmap)
        {
            using (var renderer = SvgRenderer.FromImage(bitmap))
            {
                var boundable = new GenericBoundable(0, 0, bitmap.Width, bitmap.Height);
                this.Draw(renderer, boundable);
            }
        }

        /// <summary>
        /// Renders the <see cref="SvgDocument"/> in given size and returns the image as a <see cref="SKBitmap"/>.
        /// </summary>
        public virtual SKBitmap Draw(int rasterWidth, int rasterHeight)
        {
            var svgSize = GetDimensions();
            var imageSize = svgSize;
            this.RasterizeDimensions(ref imageSize, rasterWidth, rasterHeight);

            if (imageSize.Width <= 0 || imageSize.Height <= 0)
                return null;

            SKBitmap bitmap = null;
            try
            {
                bitmap = new SKBitmap((int)Math.Ceiling(imageSize.Width), (int)Math.Ceiling(imageSize.Height));
                using (var renderer = SvgRenderer.FromImage(bitmap))
                {
                    renderer.ScaleTransform(imageSize.Width / svgSize.Width, imageSize.Height / svgSize.Height);
                    var boundable = new GenericBoundable(0, 0, svgSize.Width, svgSize.Height);
                    this.Draw(renderer, boundable);
                }
            }
            catch
            {
                bitmap?.Dispose();
                throw;
            }

            return bitmap;
        }
    }
}
#endif