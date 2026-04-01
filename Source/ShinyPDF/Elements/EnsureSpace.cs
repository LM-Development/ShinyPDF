using ShinyPDF.Drawing;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Elements
{
    internal class EnsureSpace : ContainerElement, ICacheable
    {
        public const float DefaultMinHeight = 150;
        public float MinHeight { get; set; } = DefaultMinHeight;

        internal override SpacePlan Measure(Size availableSpace)
        {
            var measurement = base.Measure(availableSpace);

            if (measurement.Type == SpacePlanType.PartialRender && availableSpace.Height < MinHeight)
                return SpacePlan.Wrap();

            return measurement;
        }
    }
}