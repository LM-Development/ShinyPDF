using System;
using System.Collections.Generic;
using System.Linq;
using ShinyPDF.Elements;
using ShinyPDF.Fluent;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Drawing
{
    internal class DocumentContainer : IDocumentContainer
    {
        internal List<IComponent> Pages { get; set; } = new List<IComponent>();
        
        internal Container Compose()
        {
            var container = new Container();
            
            container
                .Column(column =>
                {
                    Pages
                        .SelectMany(x => new List<Action>()
                        {
                            () => column.Item().PageBreak(),
                            () => column.Item().Component(x)
                        })
                        .Skip(1)
                        .ToList()
                        .ForEach(x => x());
                });

            return container;
        }
    }
}