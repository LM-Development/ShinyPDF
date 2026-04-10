using ShinyPDF.Drawing;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Elements
{
    internal class DefaultTextStyle : ContainerElement
    {
        public TextStyle TextStyle { get; set; } = TextStyle.Default;
    }
}