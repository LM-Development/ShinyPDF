namespace OpenQuestPDF.Infrastructure
{
    public interface IContainer
    {
        IElement? Child { get; set; }
    }
}