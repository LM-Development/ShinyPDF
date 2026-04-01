using NUnit.Framework;
using ShinyPDF.Elements;
using ShinyPDF.UnitTests.TestEngine;

namespace ShinyPDF.UnitTests
{
    [TestFixture]
    public class ExternalLinkTests
    {
        [Test]
        public void Measure() => SimpleContainerTests.Measure<Hyperlink>();
        
        // TODO: consider tests for the Draw method
    }
}