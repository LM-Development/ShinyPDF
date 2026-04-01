using ShinyPDF.Drawing;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Elements
{
    internal class PageBreak : Element, IStateResettable
    {
        private bool IsRendered { get; set; }
        
        public void ResetState()
        {
            IsRendered = false;
        }

        internal override SpacePlan Measure(Size availableSpace)
        {
            if (availableSpace.IsNegative())
                return SpacePlan.Wrap();
            
            if (IsRendered)
                return SpacePlan.FullRender(0, 0);

            return SpacePlan.PartialRender(Size.Zero);
        }

        internal override void Draw(Size availableSpace)
        {
            IsRendered = true;
        }
    }
}