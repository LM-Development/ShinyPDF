using OpenQuestPDF.Drawing;
using OpenQuestPDF.Infrastructure;

namespace OpenQuestPDF.Elements
{
    internal class DefaultTextStyle : ContainerElement
    {
        public TextStyle TextStyle { get; set; } = TextStyle.Default;
    }
}