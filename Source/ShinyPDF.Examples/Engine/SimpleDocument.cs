using System;
using ShinyPDF.Drawing;
using ShinyPDF.Elements;
using ShinyPDF.Fluent;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Examples.Engine
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