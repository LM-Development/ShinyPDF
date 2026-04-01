using ShinyPDF.Fluent;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Elements
{
    internal class Placeholder : IComponent
    {
        public string Text { get; set; } = string.Empty;
        private static readonly byte[] ImageData;

        static Placeholder()
        {
            ImageData = Helpers.Helpers.LoadEmbeddedResource("ShinyPDF.Resources.ImagePlaceholder.png");
        }

        public void Compose(IContainer container)
        {
            container
                .Background(Colors.Grey.Lighten2)
                .Padding(5)
                .AlignMiddle()
                .AlignCenter()
                .Element(x =>
                {
                    if (string.IsNullOrWhiteSpace(Text))
                        x.MaxHeight(32).Image(ImageData, ImageScaling.FitArea);
                    
                    else
                        x.Text(Text).FontSize(14);
                });
        }
    }
}