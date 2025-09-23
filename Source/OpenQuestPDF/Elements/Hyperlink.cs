using OpenQuestPDF.Drawing;
using OpenQuestPDF.Infrastructure;

namespace OpenQuestPDF.Elements
{
    internal class Hyperlink : ContainerElement
    {
        public string Url { get; set; } = string.Empty;
        
        internal override void Draw(Size availableSpace)
        {
            var targetSize = base.Measure(availableSpace);

            if (targetSize.Type == SpacePlanType.Wrap)
                return;

            Canvas.DrawHyperlink(Url, targetSize);
            base.Draw(availableSpace);
        }
    }
}