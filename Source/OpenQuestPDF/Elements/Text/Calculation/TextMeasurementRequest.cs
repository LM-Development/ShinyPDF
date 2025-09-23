using OpenQuestPDF.Infrastructure;

namespace OpenQuestPDF.Elements.Text.Calculation
{
    internal class TextMeasurementRequest
    {
        public ICanvas Canvas { get; set; } = null!;
        public IPageContext PageContext { get; set; } = null!;
        
        public int StartIndex { get; set; }
        public float AvailableWidth { get; set; }
        
        public bool IsFirstElementInBlock { get; set; }
        public bool IsFirstElementInLine { get; set; }
    }
}