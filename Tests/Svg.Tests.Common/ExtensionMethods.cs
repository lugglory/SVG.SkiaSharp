using System;
using SkiaSharp;

namespace Svg.Tests.Common
{
    /// <summary>
    /// Taken from https://web.archive.org/web/20130111215043/http://www.switchonthecode.com/tutorials/csharp-tutorial-convert-a-color-image-to-grayscale
    /// and slightly modified.
    /// Image width and height, default threshold and handling of alpha values have been adapted.
    /// </summary>
    public static class ExtensionMethods
    {
        private static readonly int ImageWidth = 64;
        private static readonly int ImageHeight = 64;

        public static float PercentageDifference(this SKBitmap img1, SKBitmap img2, byte threshold = 10)
        {
            byte[,] differences = img1.GetDifferences(img2);

            int diffPixels = 0;

            foreach (byte b in differences)
            {
                if (b > threshold) { diffPixels++; }
            }

            return diffPixels / (float)(differences.GetLength(0) * differences.GetLength(1));
        }

        public static SKBitmap Resize(this SKBitmap originalImage, int newWidth, int newHeight)
        {
            if (originalImage.Width > originalImage.Height)
                newWidth = originalImage.Width * newHeight / originalImage.Height;
            else
                newHeight = originalImage.Height * newWidth / originalImage.Width;

            var res = new SKBitmap(newWidth, newHeight);
            originalImage.ScalePixels(res, SKFilterQuality.High);
            return res;
        }

        public static byte[,] GetGrayScaleValues(this SKBitmap img)
        {
            byte[,] grayScale = new byte[img.Width, img.Height];

            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    var color = img.GetPixel(x, y);
                    var alpha = color.Alpha;
                    var gray = (byte)(0.3f * color.Red + 0.59f * color.Green + 0.11f * color.Blue);
                    grayScale[x, y] = (byte)Math.Abs(gray * alpha / 255);
                }
            }
            return grayScale;
        }

        public static byte[,] GetDifferences(this SKBitmap img1, SKBitmap img2)
        {
            using (var resizedThisOne = img1.Resize(ImageWidth, ImageHeight))
            using (var resizedTheOtherOne = img2.Resize(ImageWidth, ImageHeight))
            {
                byte[,] differences = new byte[resizedThisOne.Width, resizedThisOne.Height];
                byte[,] firstGray = resizedThisOne.GetGrayScaleValues();
                byte[,] secondGray = resizedTheOtherOne.GetGrayScaleValues();

                for (int y = 0; y < differences.GetLength(1); y++)
                {
                    for (int x = 0; x < differences.GetLength(0); x++)
                    {
                        differences[x, y] = (byte)Math.Abs(firstGray[x, y] - secondGray[x, y]);
                    }
                }
                return differences;
            }
        }
    }
}