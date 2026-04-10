using NUnit.Framework;
using ShinyPDF.Examples.Engine;
using ShinyPDF.Fluent;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Examples
{
    public class DefaultTextStyleExample
    {
        [Test]
        public void DefaultTextStyle()
        {
            RenderingTest
                .Create()
                .ProducePdf()
                .ShowResults()
                .RenderDocument(container =>
                {
                    container.Page(page =>
                    {
                        // all text in this set of pages has size 20
                        page.DefaultTextStyle(TextStyle.Default.FontSize(20));
                    
                        page.Margin(20);
                        page.Size(PageSizes.A4);
                        page.PageColor(Colors.White);
        
                        page.Content().Column(column =>
                        {
                            column.Item().Text(Placeholders.Sentence());
                        
                            column.Item().Text(text =>
                            {
                                // text in this block is additionally semibold
                                text.DefaultTextStyle(TextStyle.Default.SemiBold());
        
                                text.Line(Placeholders.Sentence());
                            
                                // this text has size 20 but also semibold and red
                                text.Span(Placeholders.Sentence()).FontColor(Colors.Red.Medium);
                            });
                        });
                    });
                });
        }
    }
}