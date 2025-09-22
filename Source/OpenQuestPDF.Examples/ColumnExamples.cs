using System;
using System.Linq;
using NUnit.Framework;
using OpenQuestPDF.Examples.Engine;
using OpenQuestPDF.Fluent;
using OpenQuestPDF.Helpers;
using OpenQuestPDF.Infrastructure;

namespace OpenQuestPDF.Examples
{
    public class ColumnExamples
    {
        [Test]
        public void Column()
        {
            RenderingTest
                .Create()
                .PageSize(PageSizes.A4)
                .ShowResults()
                .ProducePdf()
                .Render(container =>
                {
                    container.Column(column =>
                    {
                        foreach (var i in Enumerable.Range(0, 10))
                            column.Item().Element(Block);

                        static void Block(IContainer container)
                        {
                            container
                                .Width(72)
                                .Height(3.5f, Unit.Inch)
                                .Height(1.5f, Unit.Inch)
                                .Background(Placeholders.BackgroundColor());
                        }
                    });
                });
        }
        
        [Test]
        public void Stability_NoItems()
        {
            RenderingTest
                .Create()
                .ProducePdf()
                .MaxPages(100)
                .PageSize(250, 150)
                .Render(container =>
                {
                    container
                        .Padding(25)
                        .Column(column => { });
                });
        }
    }
}