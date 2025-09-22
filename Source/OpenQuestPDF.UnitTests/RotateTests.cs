using NUnit.Framework;
using OpenQuestPDF.Elements;
using OpenQuestPDF.Infrastructure;
using OpenQuestPDF.UnitTests.TestEngine;

namespace OpenQuestPDF.UnitTests
{
    [TestFixture]
    public class RotateTests
    {
        [Test]
        public void Measure() => SimpleContainerTests.Measure<Rotate>();

        [Test]
        public void Draw()
        {
            TestPlan
                .For(x => new Rotate
                {
                    Child = x.CreateChild(),
                    Angle = 123
                })
                .DrawElement(new Size(400, 300))
                .ExpectCanvasRotate(123)
                .ExpectChildDraw(new Size(400, 300))
                .ExpectCanvasRotate(-123)
                .CheckDrawResult();
        } 
    }
}