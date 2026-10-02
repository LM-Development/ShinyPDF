using ShinyPDF.Infrastructure;

namespace ShinyPDF.UnitTests.TestEngine.Operations
{
    internal record CanvasDrawRectangleOperation : OperationBase
    {
        public Position Position { get; } 
        public Size Size { get; }
        public string Color { get; }

        public CanvasDrawRectangleOperation(Position position, Size size, string color)
        {
            Position = position;
            Size = size;
            Color = color;
        }
    }
}