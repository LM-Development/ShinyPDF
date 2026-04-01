using NUnit.Framework;
using OpenQuestPDF.Examples.Engine;
using OpenQuestPDF.Fluent;
using OpenQuestPDF.Helpers;
using OpenQuestPDF.Infrastructure;

namespace OpenQuestPDF.Examples
{
    public class StopPaging
    {
        [Test]
        public void Example()
        {
            RenderingTest
                .Create()
                .PageSize(300, 250)
                .ProduceImages()
                .ShowResults()
                .Render(container => 
                {
                    container
                        .Padding(25)
                        .DefaultTextStyle(TextStyle.Default.FontSize(14))
                        .Decoration(decoration =>
                        {
                            decoration
                                .Before()
                                .Text(text =>
                                {
                                    text.DefaultTextStyle(TextStyle.Default.SemiBold().FontColor(Colors.Blue.Medium));
                                    
                                    text.Span("Page ");
                                    text.CurrentPageNumber();
                                });
                            
                            decoration
                                .Content()
                                .Column(column =>
                                {
                                    column.Spacing(25);
                                    column.Item().StopPaging().Text(Placeholders.LoremIpsum());
                                    column.Item().ExtendHorizontal().Height(75).Background(Colors.Grey.Lighten2);
                                });
                        });
                });
        }
    }
}