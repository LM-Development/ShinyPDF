namespace ShinyPDF.UnitTests.TestEngine.Operations
{
    public record CanvasRotateOperation : OperationBase
    {
        public float Angle { get; }

        public CanvasRotateOperation(float angle)
        {
            Angle = angle;
        }
    }
}