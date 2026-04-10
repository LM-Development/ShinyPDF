using System;
using ShinyPDF.Drawing;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;
using SkiaSharp;

namespace ShinyPDF.Elements
{
    internal class DynamicImage : Element
    {
        public Func<Size, byte[]>? Source { get; set; }
        
        internal override SpacePlan Measure(Size availableSpace)
        {
            return availableSpace.IsNegative() 
                ? SpacePlan.Wrap() 
                : SpacePlan.FullRender(availableSpace);
        }

        internal override void Draw(Size availableSpace)
        {
            if (Canvas == null)
                return;
            var imageData = Source?.Invoke(availableSpace);
            
            if (imageData == null)
                return;

            using var image = SKImage.FromEncodedData(imageData);
            Canvas.DrawImage(image, Position.Zero, availableSpace);
        }
    }
}