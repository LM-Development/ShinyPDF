
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Elements
{
    internal class Translate : ContainerElement
    {
        public float TranslateX { get; set; } = 0;
        public float TranslateY { get; set; } = 0;

        internal override void Draw(Size availableSpace)
        {
            if (Canvas == null)
                return;
            var translate = new Position(TranslateX, TranslateY);
            
            Canvas.Translate(translate);
            base.Draw(availableSpace);
            Canvas.Translate(translate.Reverse());
        }
    }
}