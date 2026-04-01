using ShinyPDF.Infrastructure;

namespace ShinyPDF.Elements.Text.Calculation
{
    internal class TextMeasurementRequest
    {
        public required ICanvas Canvas { get; set; }
        public required IPageContext PageContext { get; set; }
        
        public int StartIndex { get; set; }
        public float AvailableWidth { get; set; }
        
        public bool IsFirstElementInBlock { get; set; }
        public bool IsFirstElementInLine { get; set; }
    }
}