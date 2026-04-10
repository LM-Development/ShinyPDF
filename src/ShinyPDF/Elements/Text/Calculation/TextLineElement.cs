using ShinyPDF.Elements.Text.Items;

namespace ShinyPDF.Elements.Text.Calculation
{
    internal class TextLineElement
    {
        public required ITextBlockItem Item { get; set; }
        public required TextMeasurementResult Measurement { get; set; }
    }
}