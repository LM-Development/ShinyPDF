using ShinyPDF.Infrastructure;

namespace ShinyPDF.UnitTests.TestEngine.Operations
{
    internal record CanvasTranslateOperation : OperationBase
    {
        public Position Position { get; }

        public CanvasTranslateOperation(Position position)
        {
            Position = position;
        }
    }
}