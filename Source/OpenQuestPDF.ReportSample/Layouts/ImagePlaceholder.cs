using OpenQuestPDF.Fluent;
using OpenQuestPDF.Helpers;
using OpenQuestPDF.Infrastructure;

namespace OpenQuestPDF.ReportSample.Layouts
{
    public class ImagePlaceholder : IComponent
    {
        public static bool Solid { get; set; } = false;
        
        public void Compose(IContainer container)
        {
            if (Solid)
                container.Background(Placeholders.Color());
            
            else
                container.Image(Placeholders.Image);
        }
    }
}