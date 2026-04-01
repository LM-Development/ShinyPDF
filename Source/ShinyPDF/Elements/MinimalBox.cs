using ShinyPDF.Drawing;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Elements
{
    internal class MinimalBox : ContainerElement, IContentDirectionAware
    {
        public ContentDirection ContentDirection { get; set; }
        
        internal override void Draw(Size availableSpace)
        {
            if (Canvas == null)
                return;
            var targetSize = base.Measure(availableSpace);
            
            if (targetSize.Type == SpacePlanType.Wrap)
                return;
            
            var translate = ContentDirection == ContentDirection.RightToLeft
                ? new Position(availableSpace.Width - targetSize.Width, 0)
                : Position.Zero;
            
            Canvas.Translate(translate);
            base.Draw(targetSize);
            Canvas.Translate(translate.Reverse());
        }
    }
}