using OpenQuestPDF.Elements.Text.Items;

namespace OpenQuestPDF.Elements.Text.Calculation
{
    internal class TextLineElement
    {
        public required ITextBlockItem Item { get; set; }
        public required TextMeasurementResult Measurement { get; set; }
    }
}