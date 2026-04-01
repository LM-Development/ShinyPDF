using OpenQuestPDF.Drawing;
using OpenQuestPDF.Infrastructure;

namespace OpenQuestPDF.Elements.Text.Calculation
{
    internal class TextDrawingRequest
    {
        public required ICanvas Canvas { get; set; }
        public required IPageContext PageContext { get; set; }
        
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        
        public float TotalAscent { get; set; }
        public Size TextSize { get; set; }
    }
}