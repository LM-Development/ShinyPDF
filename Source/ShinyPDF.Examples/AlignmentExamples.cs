using NUnit.Framework;
using ShinyPDF.Examples.Engine;
using ShinyPDF.Fluent;
using ShinyPDF.Helpers;

namespace ShinyPDF.Examples
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