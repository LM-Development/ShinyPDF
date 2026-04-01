using ShinyPDF.Elements;

namespace ShinyPDF.Infrastructure
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