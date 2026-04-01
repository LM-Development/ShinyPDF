using NUnit.Framework;
using ShinyPDF.Elements;
using ShinyPDF.UnitTests.TestEngine;

namespace ShinyPDF.UnitTests
{
    [TestFixture]
    public class InternalLocationTests
    {
        [Test]
        public void Measure() => SimpleContainerTests.Measure<SectionLink>();
        
        // TODO: consider tests for the Draw method
    }
}