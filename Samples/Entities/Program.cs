using System;
using System.Collections.Generic;
using Svg;
using System.IO;
using SkiaSharp;

namespace Entities
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../sample.svg");

            var sampleDoc = SvgDocument.Open<SvgDocument>(filePath,  new Dictionary<string, string> 
                {
                    {"entity1", "fill:red" },
                    {"entity2", "fill:yellow" }
                });

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