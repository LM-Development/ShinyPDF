using System;
using System.Collections.Generic;
using OpenQuestPDF.Infrastructure;

#if NET6_0_OR_GREATER

namespace OpenQuestPDF.Previewer
{
    internal class PreviewerRefreshCommand
    {
        public ICollection<Page> Pages { get; set; }

        public class Page
        {
            public string Id { get; } = Guid.NewGuid().ToString("N");
            
            public float Width { get; init; }
            public float Height { get; init; }
        }
    }
}

#endif