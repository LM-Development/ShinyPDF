using System;
using ShinyPDF.Elements;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Fluent
{
    public class ColumnDescriptor
    {
        internal Column Column { get; } = new();

        public void Spacing(float value, Unit unit = Unit.Point)
        {
            Column.Spacing = value.ToPoints(unit);
        }
        
        public IContainer Item()
        {
            var container = new Container();
            
            Column.Items.Add(new ColumnItem
            {
                Child = container
            });
            
            return container;
        }
    }
    
    public static class ColumnExtensions
    {
        
        public static void Column(this IContainer element, Action<ColumnDescriptor> handler)
        {
            var descriptor = new ColumnDescriptor();
            handler(descriptor);
            element.Element(descriptor.Column);
        }
    }
}