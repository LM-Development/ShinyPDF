using System.IO;
using NUnit.Framework;
using OpenQuestPDF.Drawing;
using OpenQuestPDF.Examples.Engine;
using OpenQuestPDF.Fluent;
using OpenQuestPDF.Helpers;
using OpenQuestPDF.Infrastructure;

namespace OpenQuestPDF.Examples
{
    public class BarcodeExamples
    {
        [Test]
        public void Example()
        {
            FontManager.RegisterFont(File.OpenRead("LibreBarcode39-Regular.ttf"));
            
            RenderingTest
                .Create()
                .PageSize(400, 200)
                .ShowResults()
                .Render(container =>
                {
                    container
                        .Background(Colors.White)
                        .AlignCenter()
                        .AlignMiddle()
                        .Text("*QuestPDF*")
                        .FontFamily("Libre Barcode 39")
                        .FontSize(64);
                });
        }
    }
}