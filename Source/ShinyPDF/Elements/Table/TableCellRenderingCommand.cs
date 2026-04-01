using ShinyPDF.Drawing;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Elements.Table
{
    internal class TableCellRenderingCommand
    {
        public required TableCell Cell { get; set; }
        public SpacePlan Measurement { get; set; }
        public Size Size { get; set; }
        public Position Offset { get; set; }
    }
}