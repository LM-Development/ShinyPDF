using OpenQuestPDF.Drawing;

namespace OpenQuestPDF.Infrastructure
{
    public interface IDocument
    {
        DocumentMetadata GetMetadata();
        void Compose(IDocumentContainer container);
    }
}