using ShinyPDF.Drawing;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;
using SkiaSharp;

namespace ShinyPDF.Elements
{
    internal class Image : Element, ICacheable
    {
        public SKImage? InternalImage { get; set; }

        ~Image()
        {
            InternalImage?.Dispose();
        }
        
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
            if (InternalImage == null)
                return;

            Canvas.DrawImage(InternalImage, Position.Zero, availableSpace);
        }
    }
}