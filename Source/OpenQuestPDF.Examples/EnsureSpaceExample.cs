using NUnit.Framework;
using OpenQuestPDF.Examples.Engine;
using OpenQuestPDF.Fluent;
using OpenQuestPDF.Helpers;
using OpenQuestPDF.Infrastructure;

namespace OpenQuestPDF.Examples
{
    public class EnsureSpaceExample
    {
        [Test]
        public void EnsureSpaceWith()
        {
            RenderingTest
                .Create()
                .ProduceImages()
                .ShowResults()
                .RenderDocument(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(20);
                        page.Size(PageSizes.A7.Landscape());
                        page.PageColor(Colors.White);
                        
                        page.Header().Text("With ensure space").SemiBold();
                        
                        page.Content().Column(column =>
                        {
                            column
                                .Item()
                                .ExtendHorizontal()
                                .Height(75)
                                .Background(Colors.Grey.Lighten2);
                            
                            column
                                .Item()
                                .EnsureSpace(100)
                                .Text(Placeholders.LoremIpsum());
                        });
                        
                        page.Footer().Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" out of ");
                            text.TotalPages();
                        });
                    });
                });
        }
    }
}