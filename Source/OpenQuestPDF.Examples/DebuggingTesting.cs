using NUnit.Framework;
using OpenQuestPDF.Drawing.Exceptions;
using OpenQuestPDF.Examples.Engine;
using OpenQuestPDF.Fluent;
using OpenQuestPDF.Helpers;

namespace OpenQuestPDF.Examples
{
    public class DebuggingTesting
    {
        [Test]
        public void Column()
        {
            Assert.Throws<DocumentLayoutException>(() =>
            {
                RenderingTest
                    .Create()
                    .PageSize(500, 360)
                    .Render(container =>
                    {
                        container
                            .Padding(10)
                            .Width(100)
                            .Background(Colors.Grey.Lighten3)
                            .DebugPointer("Example debug pointer")
                            .Column(x =>
                            {
                                x.Item().Text("Test");
                                x.Item().Width(150);
                            });
                    });
            });
        }

        [Test]
        public void Simple()
        {
            Assert.Throws<DocumentLayoutException>(() =>
            {
                RenderingTest
                    .Create()
                    .PageSize(500, 360)
                    .Render(container =>
                    {
                        container
                            .Padding(10)
                            .Width(100)
                            .Background(Colors.Grey.Lighten3)
                            .Width(150)
                            .Text("Test");
                    });
            });
        }

        [Test]
        public void DebugPointer()
        {
            Assert.Throws<DocumentLayoutException>(() =>
            {
                RenderingTest
                    .Create()
                    .PageSize(500, 360)
                    .Render(container =>
                    {
                        container
                            .Background(Colors.Grey.Lighten3)
                            .Padding(10)
                            .Width(100)
                            .DebugPointer("Example debug pointer")
                            .Width(150)
                            .Text("Example");
                    });
            });
        }
    }
}