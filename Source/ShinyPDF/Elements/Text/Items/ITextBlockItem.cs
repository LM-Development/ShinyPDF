using ShinyPDF.Elements.Text.Calculation;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Elements.Text.Items
{
    internal interface ITextBlockItem
    {
        TextMeasurementResult? Measure(TextMeasurementRequest request);
        void Draw(TextDrawingRequest request);
    }
}