using System;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Drawing
{
    public class DocumentMetadata
    {
        public int ImageQuality { get; set; } = 101;
        public int RasterDpi { get; set; } = 72;
        public bool PdfA { get; set; }
        
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Subject { get; set; }
        public string? Keywords { get; set; }
        public string? Creator { get; set; }
        public string? Producer { get; set; }

        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        public static DocumentMetadata Default => new DocumentMetadata();
    }
}