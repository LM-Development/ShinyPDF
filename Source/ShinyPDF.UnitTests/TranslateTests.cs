using NUnit.Framework;
using ShinyPDF.Elements;
using ShinyPDF.Infrastructure;
using ShinyPDF.UnitTests.TestEngine;

namespace ShinyPDF.UnitTests
{
    [TestFixture]
    public class TranslateTests
    {
        [Test]
        public void Measure() => SimpleContainerTests.Measure<Translate>();
        
        [Test]
        public void Draw()
        {
            TestPlan
                .For(x => new Translate
                {
                    Child = x.CreateChild(),
                    TranslateX = 50,
                    TranslateY = 75
                })
                .DrawElement(new Size(400, 300))
                .ExpectCanvasTranslate(50, 75)
                .ExpectChildDraw(new Size(400, 300))
                .ExpectCanvasTranslate(-50, -75)
                .CheckDrawResult();
        }
    }
}