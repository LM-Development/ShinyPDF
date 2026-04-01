using NUnit.Framework;
using ShinyPDF.Examples.Engine;
using ShinyPDF.Fluent;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;
using SkiaSharp;
using Svg.Skia;

namespace ShinyPDF.Examples
{
    public class SvgImageExample
    {
        [Test]
        public void BorderRadius()
        {
            RenderingTest
                .Create()
                .PageSize(175, 100)
                .ProduceImages()
                .ShowResults()
                .Render(container =>
                {
                    container
                        .Background(Colors.Grey.Lighten2)
                        .Padding(25)
                        .Canvas((canvas, space) =>
                        {
                            using var svg = new SKSvg();
                            svg.Load("pdf-icon.svg");
                            
                            canvas.DrawPicture(svg.Picture);
                        });
                });
        }
    }
}