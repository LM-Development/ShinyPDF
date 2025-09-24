using OpenQuestPDF.Infrastructure;

namespace OpenQuestPDF.Elements
{
    internal class Section : ContainerElement, IStateResettable
    {
        public string LocationName { get; set; } = string.Empty;
        private bool IsRendered { get; set; }
        
        public void ResetState()
        {
            IsRendered = false;
        }
        
        internal override void Draw(Size availableSpace)
        {
            if(PageContext == null)
                return;

            if (!IsRendered)
            {
                if (Canvas == null)
                    return;
                Canvas.DrawSection(LocationName);
                IsRendered = true;
            }
            
            PageContext.SetSectionPage(LocationName);
            base.Draw(availableSpace);
        }
    }
}