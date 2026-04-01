using System.Collections.Generic;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Drawing.Proxy
{
    public class DebugStackItem
    {
        public required IElement Element { get; set; }
        public Size AvailableSpace { get; internal set; }
        public SpacePlan SpacePlan { get; internal set; }

        public ICollection<DebugStackItem> Stack { get; internal set; } = new List<DebugStackItem>();
    }
}