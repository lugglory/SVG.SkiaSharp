using System;
using SkiaSharp;
using System.IO;
using Svg;

namespace SvgRuntimeUpdates
{
    class Program
    {
        static void Main(string[] args)
        {
            var sampleDoc = SvgDocument.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../sample.svg"));
            sampleDoc.GetElementById<SvgUse>("Commonwealth_Star").Fill = new SvgColourServer(SKColors.Black);
            
            var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../sample.png");
            using (var bitmap = sampleDoc.Draw())
            {
                if (bitmap != null)
                {
                    using (var image = SKImage.FromBitmap(bitmap))
                    using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                    using (var stream = File.OpenWrite(outputPath))
                    {
                        data.SaveTo(stream);
                    }
                }
            }
        }
    }
}