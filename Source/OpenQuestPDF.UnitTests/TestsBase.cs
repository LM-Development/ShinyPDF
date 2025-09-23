using FluentAssertions;
using FluentAssertions.Equivalency;
using NUnit.Framework;

namespace OpenQuestPDF.UnitTests
{
    [SetUpFixture]
    public class TestsBase
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            // FluentAssertions configuration removed as the API has changed in newer versions
            // The default configuration should be sufficient for our test needs
        }
    }
}