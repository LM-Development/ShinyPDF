using System;
using OpenQuestPDF.Drawing;
using OpenQuestPDF.Infrastructure;

namespace OpenQuestPDF.Elements
{
    internal class StopPaging : ContainerElement
    {
        internal override SpacePlan Measure(Size availableSpace)
        {
            if (Child == null)
                return SpacePlan.FullRender(Size.Zero);

            var measurement = Child.Measure(availableSpace);

            return measurement.Type switch
            {
                SpacePlanType.Wrap => SpacePlan.FullRender(Size.Zero),
                SpacePlanType.PartialRender => SpacePlan.FullRender(measurement),
                SpacePlanType.FullRender => measurement,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}