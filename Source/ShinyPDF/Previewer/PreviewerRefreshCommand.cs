using System;
using System.Collections.Generic;
using ShinyPDF.Infrastructure;

#if NET6_0_OR_GREATER

namespace ShinyPDF.Previewer
{
    internal class PreviewerRefreshCommand
    {
        public ICollection<Page> Pages { get; set; } = new List<Page>();

        public class Page
        {
            public string Id { get; } = Guid.NewGuid().ToString("N");
            
            public float Width { get; init; }
            public float Height { get; init; }
        }
    }
}

#endif