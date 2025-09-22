using NUnit.Framework;
using OpenQuestPDF.Drawing;
using OpenQuestPDF.Elements;
using OpenQuestPDF.Infrastructure;
using OpenQuestPDF.UnitTests.TestEngine;

namespace OpenQuestPDF.UnitTests
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