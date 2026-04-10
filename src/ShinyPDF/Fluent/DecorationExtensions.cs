using System;
using ShinyPDF.Elements;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Fluent
{
    public class DecorationDescriptor
    {
        internal Decoration Decoration { get; } = new Decoration();
        
        public IContainer Before()
        {
            var container = new Container();
            Decoration.Before = container;
            return container;
        }
        
        public void Before(Action<IContainer> handler)
        {
            handler?.Invoke(Before());
        }
        
        public IContainer Content()
        {
            var container = new Container();
            Decoration.Content = container;
            return container;
        }
        
        public void Content(Action<IContainer> handler)
        {
            handler?.Invoke(Content());
        }
        
        public IContainer After()
        {
            var container = new Container();
            Decoration.After = container;
            return container;
        }
        
        public void After(Action<IContainer> handler)
        {
            handler?.Invoke(After());
        }
    }
    
    public static class DecorationExtensions
    {
        public static void Decoration(this IContainer element, Action<DecorationDescriptor> handler)
        {
            var descriptor = new DecorationDescriptor();
            handler(descriptor);
            
            element.Element(descriptor.Decoration);
        }
    }
}