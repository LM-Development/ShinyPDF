using System.ComponentModel;

namespace ShinyPDF.Elements
{
    internal class DebugPointer : Container
    {
        public required string Target { get; set; }
        public bool Highlight { get; set; }
    }
}