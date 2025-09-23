using System;
using System.Collections;
using System.Collections.Generic;
using OpenQuestPDF.Drawing;

namespace OpenQuestPDF.Infrastructure
{
    internal abstract class Element : IElement
    {
        internal IPageContext PageContext { get; set; } = null!;
        internal ICanvas Canvas { get; set; } = null!;
        
        internal virtual IEnumerable<Element?> GetChildren()
        {
            yield break;
        }

        internal virtual void CreateProxy(Func<Element?, Element?> create)
        {
            
        }
        
        internal abstract SpacePlan Measure(Size availableSpace);
        internal abstract void Draw(Size availableSpace);
    }
}