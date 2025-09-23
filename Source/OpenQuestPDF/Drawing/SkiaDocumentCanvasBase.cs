using System.IO;
using OpenQuestPDF.Infrastructure;
using SkiaSharp;

namespace OpenQuestPDF.Drawing
{
    internal class SkiaDocumentCanvasBase : SkiaCanvasBase
    {
        private SKDocument Document { get; }

        protected SkiaDocumentCanvasBase(SKDocument document)
        {
            Document = document;
        }

        ~SkiaDocumentCanvasBase()
        {
            Document?.Dispose();
        }
        
        public override void BeginDocument()
        {
            
        }

        public override void EndDocument()
        {
            Canvas?.Dispose();
            
            Document.Close();
            Document.Dispose();
        }

        public override void BeginPage(Size size)
        {
            Canvas = Document.BeginPage(size.Width, size.Height);
        }

        public override void EndPage()
        {
            Document.EndPage();
            Canvas?.Dispose();
        }
    }
}