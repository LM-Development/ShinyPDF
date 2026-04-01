namespace ShinyPDF.Infrastructure
{
    public interface IContainer
    {
        IElement Child { get; set; }
    }
}