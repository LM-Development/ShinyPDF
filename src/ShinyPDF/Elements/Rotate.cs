using ShinyPDF.Infrastructure;

namespace ShinyPDF.Elements
{
    internal class Rotate : ContainerElement
    {
        public float Angle { get; set; } = 0;

        internal override void Draw(Size availableSpace)
        {
            if (Canvas == null)
                return;
            Canvas.Rotate(Angle);
            Child?.Draw(availableSpace);
            Canvas.Rotate(-Angle);
        }
    }
}