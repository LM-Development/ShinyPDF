using ShinyPDF.Drawing;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Elements
{
    internal class SectionLink : ContainerElement
    {
        public string? SectionName { get; set; }
        
        internal override void Draw(Size availableSpace)
        {
            if (Canvas == null)
                return;
            var targetSize = base.Measure(availableSpace);

            if (targetSize.Type == SpacePlanType.Wrap)
                return;

            if (SectionName == null)
                return;

            Canvas.DrawSectionLink(SectionName, targetSize);
            base.Draw(availableSpace);
        }
    }
}