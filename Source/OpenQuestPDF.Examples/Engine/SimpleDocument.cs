using System;
using OpenQuestPDF.Drawing;
using OpenQuestPDF.Elements;
using OpenQuestPDF.Fluent;
using OpenQuestPDF.Helpers;
using OpenQuestPDF.Infrastructure;

namespace OpenQuestPDF.Examples.Engine
{
    public class SimpleDocument : IDocument
    {
        public const int ImageScalingFactor = 2;
        
        private Action<IDocumentContainer> Content { get; }

        public SimpleDocument(Action<IDocumentContainer> content)
        {
            Content = content;         
        }
        
        public DocumentMetadata GetMetadata()
        {
            
            return new DocumentMetadata()
            {
                RasterDpi = PageSizes.PointsPerInch * ImageScalingFactor,
            };
        }
        
        public void Compose(IDocumentContainer container)
        {
            Content(container);
        }
    }
}