using System;
using ShinyPDF.Drawing;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.UnitTests.TestEngine
{
    internal class ElementMock : Element
    {
        public string Id { get; set; }
        public Func<Size, SpacePlan> MeasureFunc { get; set; }
        public Action<Size> DrawFunc { get; set; }

        internal override SpacePlan Measure(Size availableSpace) => MeasureFunc(availableSpace);
        internal override void Draw(Size availableSpace) => DrawFunc(availableSpace);
    }
}