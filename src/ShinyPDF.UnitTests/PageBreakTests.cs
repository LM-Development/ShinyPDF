using NUnit.Framework;
using ShinyPDF.Drawing;
using ShinyPDF.Elements;
using ShinyPDF.Infrastructure;
using ShinyPDF.UnitTests.TestEngine;

namespace ShinyPDF.UnitTests
{
    [TestFixture]
    public class PageBreakTests
    {
        [Test]
        public void Measure()
        {
            TestPlan
                .For(x => new PageBreak())
                
                .MeasureElement(new Size(400, 300))
                .CheckMeasureResult(SpacePlan.PartialRender(Size.Zero))
                
                .DrawElement(new Size(400, 300))
                .CheckDrawResult()
                
                .MeasureElement(new Size(500, 400))
                .CheckMeasureResult(SpacePlan.FullRender(0, 0));
        }
    }
}