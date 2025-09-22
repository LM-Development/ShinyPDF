using OpenQuestPDF.Drawing;
using OpenQuestPDF.Infrastructure;

namespace OpenQuestPDF.Elements
{
    internal class ShowEntire : ContainerElement, ICacheable
    {
        internal override SpacePlan Measure(Size availableSpace)
        {
            var childMeasurement = base.Measure(availableSpace);

            if (childMeasurement.Type == SpacePlanType.FullRender)
                return childMeasurement;

            return SpacePlan.Wrap();
        }
    }
}