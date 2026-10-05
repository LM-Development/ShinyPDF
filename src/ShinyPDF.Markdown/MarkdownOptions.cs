using System;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Markdown
{
    public class MarkdownOptions
    {
        /// <summary>Font size of body text; headings are scaled from it.</summary>
        public float BaseFontSize { get; set; } = 12;

        /// <summary>Vertical space between blocks (paragraphs, headings, lists, ...).</summary>
        public float BlockSpacing { get; set; } = 8;

        /// <summary>Vertical space between list items.</summary>
        public float ListItemSpacing { get; set; } = 2;

        /// <summary>Width reserved for the bullet or number of a list item.</summary>
        public float ListIndent { get; set; } = 18;

        public string LinkColor { get; set; } = Colors.Blue.Darken2;

        public string CodeFontFamily { get; set; } = Fonts.Courier;

        public string CodeBackgroundColor { get; set; } = Colors.Grey.Lighten4;

        public string BlockquoteBorderColor { get; set; } = Colors.Grey.Lighten1;

        public string BlockquoteTextColor { get; set; } = Colors.Grey.Darken2;

        public string HorizontalRuleColor { get; set; } = Colors.Grey.Lighten1;

        /// <summary>
        /// Overrides the style of a heading. Receives the heading level (1-6).
        /// When not set, headings are bold and scaled from <see cref="BaseFontSize"/>.
        /// </summary>
        public Func<int, TextStyle>? HeadingStyle { get; set; }
    }
}
