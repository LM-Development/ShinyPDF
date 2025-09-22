using OpenQuestPDF.Infrastructure;

namespace OpenQuestPDF.UnitTests.TestEngine.Operations
{
    internal class CanvasDrawImageOperation : OperationBase
    {
        public Position Position { get; }
        public Size Size { get; }

        public CanvasDrawImageOperation(Position position, Size size)
        {
            Position = position;
            Size = size;
        }
    }
}