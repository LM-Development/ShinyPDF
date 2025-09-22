using OpenQuestPDF.Elements.Text.Items;

namespace OpenQuestPDF.Elements.Text.Calculation
{
    internal class TextLineElement
    {
        public ITextBlockItem Item { get; set; }
        public TextMeasurementResult Measurement { get; set; }
    }
}