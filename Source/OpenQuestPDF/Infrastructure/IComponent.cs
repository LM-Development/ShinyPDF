using OpenQuestPDF.Elements;

namespace OpenQuestPDF.Infrastructure
{
    interface ISlot
    {
        
    }

    class Slot : Container, ISlot
    {
        
    }
    
    public interface IComponent
    {
        void Compose(IContainer container);
    }
}