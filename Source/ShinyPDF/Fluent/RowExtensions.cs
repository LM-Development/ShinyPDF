using System;
using ShinyPDF.Elements;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Fluent
{
    public class RowDescriptor
    {
        internal Row Row { get; } = new();

        public void Spacing(float value)
        {
            Row.Spacing = value;
        }

        private IContainer Item(RowItemType type, float size = 0)
        {
            var element = new RowItem
            {
                Type = type,
                Size = size
            };
            
            Row.Items.Add(element);
            return element;
        }

        public IContainer RelativeItem(float size = 1)
        {
            return Item(RowItemType.Relative, size);
        }
        
        public IContainer ConstantItem(float size, Unit unit = Unit.Point)
        {
            return Item(RowItemType.Constant, size.ToPoints(unit));
        }

        public IContainer AutoItem()
        {
            return Item(RowItemType.Auto);
        }
    }
    
    public static class RowExtensions
    {
        public static void Row(this IContainer element, Action<RowDescriptor> handler)
        {
            var descriptor = new RowDescriptor();
            handler(descriptor);
            element.Element(descriptor.Row);
        }
    }
}