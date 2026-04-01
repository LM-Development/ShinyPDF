using System;
using OpenQuestPDF.Drawing;
using OpenQuestPDF.Helpers;
using OpenQuestPDF.Infrastructure;
using SkiaSharp;

namespace OpenQuestPDF.Elements
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