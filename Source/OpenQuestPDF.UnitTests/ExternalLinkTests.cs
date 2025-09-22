using NUnit.Framework;
using OpenQuestPDF.Elements;
using OpenQuestPDF.UnitTests.TestEngine;

namespace OpenQuestPDF.UnitTests
{
    [TestFixture]
    public class ExternalLinkTests
    {
        [Test]
        public void Measure() => SimpleContainerTests.Measure<Hyperlink>();
        
        // TODO: consider tests for the Draw method
    }
}