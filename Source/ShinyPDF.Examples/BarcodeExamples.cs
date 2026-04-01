using System.IO;
using NUnit.Framework;
using ShinyPDF.Drawing;
using ShinyPDF.Examples.Engine;
using ShinyPDF.Fluent;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Examples
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
                        .Text("*ShinyPDF*")
                        .FontFamily("Libre Barcode 39")
                        .FontSize(64);
                });
        }
    }
}