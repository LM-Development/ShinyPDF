using System;
using System.Collections.Generic;
using System.Linq;
using ShinyPDF.Drawing;
using ShinyPDF.Elements;
using ShinyPDF.Elements.Text;
using ShinyPDF.Elements.Text.Items;
using ShinyPDF.Infrastructure;
using static System.String;

namespace ShinyPDF.Fluent
{
    public class TextSpanDescriptor
    {
        internal TextStyle TextStyle = TextStyle.Default;
        internal Action<TextStyle> AssignTextStyle { get; }

        internal TextSpanDescriptor(Action<TextStyle> assignTextStyle)
        {
            AssignTextStyle = assignTextStyle;
        }

        internal void MutateTextStyle(Func<TextStyle, TextStyle> handler)
        {
            TextStyle = handler(TextStyle);
            AssignTextStyle(TextStyle);
        }
    }

    public delegate string PageNumberFormatter(int? pageNumber);
    
    public class TextPageNumberDescriptor : TextSpanDescriptor
    {
        internal Action<PageNumberFormatter> AssignFormatFunction { get; }
        
        internal TextPageNumberDescriptor(Action<TextStyle> assignTextStyle, Action<PageNumberFormatter> assignFormatFunction) : base(assignTextStyle)
        {
            AssignFormatFunction = assignFormatFunction;
            AssignFormatFunction(x => x?.ToString() ?? string.Empty);
        }

        public TextPageNumberDescriptor Format(PageNumberFormatter formatter)
        {
            AssignFormatFunction(formatter);
            return this;
        }
    }
    
    public class TextDescriptor
    {
        private ICollection<TextBlock> TextBlocks { get; } = new List<TextBlock>();
        private TextStyle? DefaultStyle { get; set; }
        internal HorizontalAlignment? Alignment { get; set; }
        private float Spacing { get; set; } = 0f;

        public void DefaultTextStyle(TextStyle style)
        {
            DefaultStyle = style;
        }
        
        public void DefaultTextStyle(Func<TextStyle, TextStyle> style)
        {
            DefaultStyle = style(TextStyle.Default);
        }
  
        public void AlignLeft()
        {
            Alignment = HorizontalAlignment.Left;
        }
        
        public void AlignCenter()
        {
            Alignment = HorizontalAlignment.Center;
        }
        
        public void AlignRight()
        {
            Alignment = HorizontalAlignment.Right;
        }

        public void ParagraphSpacing(float value, Unit unit = Unit.Point)
        {
            Spacing = value.ToPoints(unit);
        }

        private void AddItemToLastTextBlock(ITextBlockItem item)
        {
            if (!TextBlocks.Any())
                TextBlocks.Add(new TextBlock());
            
            var lastTextBlock = TextBlocks.Last();

            // TextBlock with only one Span with empty text is a special case.
            // It represents an empty line with a given text style (e.g. text height).
            // When more content is put to text block, the first items should be ignored (removed in this case).
            // This change fixes inconsistent line height problem.
            if (lastTextBlock.Items.Count == 1 && lastTextBlock.Items[0] is TextBlockSpan { Text: "" } && lastTextBlock.Items[0] is not TextBlockPageNumber)
            {
                lastTextBlock.Items[0] = item;
                return;
            }
            
            lastTextBlock.Items.Add(item);
        }
        
        
        public TextSpanDescriptor Span(string? text)
        {
            if (text == null)
                return new TextSpanDescriptor(_ => { });
 
            var items = text
                .Replace("\r", string.Empty)
                .Split(new[] { '\n' }, StringSplitOptions.None)
                .Select(x => new TextBlockSpan
                {
                    Text = x
                })
                .ToList();

            AddItemToLastTextBlock(items.First());

            items
                .Skip(1)
                .Select(x => new TextBlock
                {   
                    Items = new List<ITextBlockItem> { x }
                })
                .ToList()
                .ForEach(TextBlocks.Add);

            return new TextSpanDescriptor(x => items.ForEach(y => y.Style = x));
        }

        public TextSpanDescriptor Line(string? text)
        {
            text ??= string.Empty;
            return Span(text + Environment.NewLine);
        }

        public TextSpanDescriptor EmptyLine()
        {
            return Span(Environment.NewLine);
        }

        private int RunDebug(IPageContext context,Func<IPageContext, int?> pageNumberFunc){
            return pageNumberFunc(context) ?? 0;
        }
        
        private TextPageNumberDescriptor PageNumber(Func<IPageContext, int?> pageNumber)
        {
            var textBlockItem = new TextBlockPageNumber();
            AddItemToLastTextBlock(textBlockItem);
            
            return new TextPageNumberDescriptor(x => textBlockItem.Style = x, x => textBlockItem.Source = context => x(pageNumber(context)));
        }

        public TextPageNumberDescriptor CurrentPageNumber()
        {
            return PageNumber(x => RunDebug(x, y => y.CurrentPage));
        }
        
        public TextPageNumberDescriptor TotalPages()
        {
            return PageNumber(x => x.GetLocation(PageContext.DocumentLocation)?.Length);
        }
        
        public TextPageNumberDescriptor BeginPageNumberOfSection(string locationName)
        {
            return PageNumber(x => x.GetLocation(locationName)?.PageStart);
        }
        
        public TextPageNumberDescriptor EndPageNumberOfSection(string locationName)
        {
            return PageNumber(x => x.GetLocation(locationName)?.PageEnd);
        }
        
        public TextPageNumberDescriptor PageNumberWithinSection(string locationName)
        {
            return PageNumber(x => x.CurrentPage + 1 - x.GetLocation(locationName)?.PageStart);
        }
        
        public TextPageNumberDescriptor TotalPagesWithinSection(string locationName)
        {
            return PageNumber(x => x.GetLocation(locationName)?.Length);
        }
        
        public TextSpanDescriptor SectionLink(string? text, string sectionName)
        {
            if (IsNullOrEmpty(sectionName))
                throw new ArgumentException("Section name cannot be null or empty", nameof(sectionName));

            if (IsNullOrEmpty(text))
                return new TextSpanDescriptor(_ => { });

            var textBlockItem = new TextBlockSectionLink
            {
                Text = text,
                SectionName = sectionName
            };

            AddItemToLastTextBlock(textBlockItem);
            return new TextSpanDescriptor(x => textBlockItem.Style = x);
        }
        
        
        public TextSpanDescriptor Hyperlink(string? text, string url)
        {
            if (IsNullOrEmpty(url))
                throw new ArgumentException("Url cannot be null or empty", nameof(url));

            if (IsNullOrEmpty(text))
                return new TextSpanDescriptor(_ => { });
            
            var textBlockItem = new TextBlockHyperlink
            {
                Text = text,
                Url = url
            };

            AddItemToLastTextBlock(textBlockItem);
            return new TextSpanDescriptor(x => textBlockItem.Style = x);
        }
        
        
        public IContainer Element()
        {
            var container = new Container();
                
            AddItemToLastTextBlock(new TextBlockElement
            {
                Element = container
            });
            
            return container.AlignBottom().MinimalBox();
        }
        
        internal void Compose(IContainer container)
        {
            TextBlocks.ToList().ForEach(x => x.Alignment ??= Alignment);
            
            if (DefaultStyle != null)
                container = container.DefaultTextStyle(DefaultStyle);

            if (TextBlocks.Count == 1)
            {
                container.Element(TextBlocks.First());
                return;
            }
            
            container.Column(column =>
            {
                column.Spacing(Spacing);

                foreach (var textBlock in TextBlocks)
                    column.Item().Element(textBlock);
            }); 
        }
    }
    
    public static class TextExtensions
    {
        public static void Text(this IContainer element, Action<TextDescriptor> content)
        {
            var descriptor = new TextDescriptor();
            
            if (element is Alignment alignment)
                descriptor.Alignment = alignment.Horizontal;
            
            content?.Invoke(descriptor);
            descriptor.Compose(element);
        }
        

        public static TextSpanDescriptor Text(this IContainer element, string? text)
        {
            TextSpanDescriptor? descriptor = null;
            element.Text(x => descriptor = x.Span(text));
            if (descriptor == null)
                throw new InvalidOperationException("Text descriptor cannot be null");
            return descriptor;
        }
    }
}