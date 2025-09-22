using OpenQuestPDF.Drawing;
using OpenQuestPDF.Helpers;
using OpenQuestPDF.Infrastructure;

namespace OpenQuestPDF.Elements
{
    internal class Empty : Element
    {
        internal static Empty Instance { get; } = new Empty();
        
        internal override SpacePlan Measure(Size availableSpace)
        {
            return availableSpace.IsNegative() 
                ? SpacePlan.Wrap() 
                : SpacePlan.FullRender(0, 0);
        }

        internal override void Draw(Size availableSpace)
        {
            
        }
    }
}