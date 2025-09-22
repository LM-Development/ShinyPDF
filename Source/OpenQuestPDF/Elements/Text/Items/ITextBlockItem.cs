using OpenQuestPDF.Elements.Text.Calculation;
using OpenQuestPDF.Infrastructure;

namespace OpenQuestPDF.Elements.Text.Items
{
    internal interface ITextBlockItem
    {
        TextMeasurementResult? Measure(TextMeasurementRequest request);
        void Draw(TextDrawingRequest request);
    }
}