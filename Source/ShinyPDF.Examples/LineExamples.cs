using System.IO;
using NUnit.Framework;
using ShinyPDF.Examples.Engine;
using ShinyPDF.Fluent;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Examples
{
    public class LineExamples
    {
        [Test]
        public void LineHorizontal()
        {
            RenderingTest
                .Create()
                .PageSize(PageSizes.A5)
                .ProduceImages()
                .ShowResults()
                .Render(container => 
                {
                    container
                        .Padding(15)
                        .MinimalBox()
                        .DefaultTextStyle(TextStyle.Default.FontSize(16))
                        .Column(column =>
                        {
                            column.Item().Text("Above text");
                            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                            column.Item().Text("Below text");
                        });
                });
        }
        
        [Test]
        public void LineVertical()
        {
            RenderingTest
                .Create()
                .PageSize(PageSizes.A5)
                .ProduceImages()
                .ShowResults()
                .Render(container => 
                {
                    container
                        .Padding(15)
                        .DefaultTextStyle(TextStyle.Default.FontSize(16))
                        .Row(row =>
                        {
                            row.AutoItem().Text("Left text");
                            row.AutoItem().PaddingHorizontal(10).LineVertical(1).LineColor(Colors.Grey.Medium);
                            row.AutoItem().Text("Right text");
                        });
                });
        }
    }
}