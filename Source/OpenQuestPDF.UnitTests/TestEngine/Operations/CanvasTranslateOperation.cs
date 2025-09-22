using OpenQuestPDF.Infrastructure;

namespace OpenQuestPDF.UnitTests.TestEngine.Operations
{
    internal class CanvasTranslateOperation : OperationBase
    {
        public Position Position { get; }

        public CanvasTranslateOperation(Position position)
        {
            Position = position;
        }
    }
}