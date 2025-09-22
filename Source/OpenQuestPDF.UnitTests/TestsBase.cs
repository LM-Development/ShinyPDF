using FluentAssertions;
using NUnit.Framework;

namespace OpenQuestPDF.UnitTests
{
    [SetUpFixture]
    public class TestsBase
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            AssertionOptions.AssertEquivalencyUsing(options => options
                .IncludingNestedObjects()
                .IncludingInternalProperties()
                .IncludingInternalFields()
                .AllowingInfiniteRecursion()
                .RespectingRuntimeTypes()
                .WithTracing()
                .WithStrictOrdering());
        }
    }
}