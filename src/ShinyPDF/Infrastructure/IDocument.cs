using ShinyPDF.Drawing;

namespace ShinyPDF.Infrastructure
{
    public interface IDocument
    {
        DocumentMetadata GetMetadata();
        void Compose(IDocumentContainer container);
    }
}