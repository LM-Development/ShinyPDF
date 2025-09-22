using NUnit.Framework;
using OpenQuestPDF.Elements;
using OpenQuestPDF.UnitTests.TestEngine;

namespace OpenQuestPDF.UnitTests
{
    [TestFixture]
    public class InternalLocationTests
    {
        [Test]
        public void Measure() => SimpleContainerTests.Measure<SectionLink>();
        
        // TODO: consider tests for the Draw method
    }
}