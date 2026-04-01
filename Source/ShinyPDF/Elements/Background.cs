using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Elements
{
    internal class Background : ContainerElement
    {
        public string Color { get; set; } = Colors.Black;
        
        internal override void Draw(Size availableSpace)
        {
            if (Canvas == null)
                return;
            Canvas.DrawRectangle(Position.Zero, availableSpace, Color);
            base.Draw(availableSpace);
        }
    }
}