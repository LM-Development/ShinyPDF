using NUnit.Framework;
using OpenQuestPDF.Examples.Engine;
using OpenQuestPDF.Fluent;
using OpenQuestPDF.Helpers;

namespace OpenQuestPDF.Examples
{
    public class AlignmentExamples
    {
        [Test]
        public void Example()
        {
            RenderingTest
                .Create()
                .PageSize(400, 200)
                .ProduceImages()
                .ShowResults()
                .Render(container =>
                {
                    container
                        .Padding(25)
                        .Border(1)
                        .AlignBottom()
                        .Background(Colors.Grey.Lighten1)
                        .Text("Test");
                });
        }
    }
}