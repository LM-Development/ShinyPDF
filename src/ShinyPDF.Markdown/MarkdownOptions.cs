using System;
using System.Collections.Generic;
using System.Linq;
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

        public string TableBorderColor { get; set; } = Colors.Grey.Lighten1;

        public string TableHeaderBackgroundColor { get; set; } = Colors.Grey.Lighten4;

        /// <summary>Space between a table cell border and its content.</summary>
        public float TableCellPadding { get; set; } = 4;

        /// <summary>
        /// Largest height of an image; taller images are scaled down proportionally.
        /// Keep it below the page content height, otherwise the image cannot be placed on any page.
        /// </summary>
        public float MaxImageHeight { get; set; } = 500;

        /// <summary>
        /// Largest image (width x height in pixels) that is decoded. Larger images show their alternative text,
        /// so a small compressed image cannot claim gigabytes of memory. Default: 40 megapixels.
        /// </summary>
        public long MaxImagePixels { get; set; } = 40_000_000;

        /// <summary>Border and fill color of task list checkboxes.</summary>
        public string CheckboxColor { get; set; } = Colors.Grey.Darken2;

        /// <summary>
        /// Overrides the style of a heading. Receives the heading level (1-6).
        /// When not set, headings are bold and scaled from <see cref="BaseFontSize"/>.
        /// </summary>
        public Func<int, TextStyle>? HeadingStyle { get; set; }

        /// <summary>
        /// Loads the bytes of an image from its Markdown URL (e.g. from disk, embedded resources or HTTP).
        /// Return <c>null</c> to show the alternative text instead. <c>data:</c> URIs are decoded without a resolver.
        /// When not set, only <c>data:</c> URIs are rendered, so Markdown never reads files or the network on its own.
        /// </summary>
        public Func<string, byte[]?>? ImageResolver { get; set; }

        /// <summary>
        /// Languages used to highlight fenced code blocks, matched by the name after the opening fence.
        /// Starts with <see cref="SyntaxLanguages.All"/>; add your own or clear the list to disable highlighting.
        /// When several languages share a name, the one added last wins.
        /// </summary>
        public List<SyntaxLanguage> CodeLanguages { get; } = SyntaxLanguages.All().ToList();

        /// <summary>
        /// Time budget for highlighting one code block. When it is exceeded (slow custom patterns or hostile input),
        /// the rest of the block is shown without highlighting.
        /// </summary>
        public TimeSpan SyntaxHighlightingTimeout { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>Colors of highlighted code. Kinds without a color use the regular text color.</summary>
        public Dictionary<SyntaxTokenKind, string> SyntaxColors { get; } = new()
        {
            [SyntaxTokenKind.Keyword] = "#CF222E",
            [SyntaxTokenKind.Type] = "#8250DF",
            [SyntaxTokenKind.String] = "#0A3069",
            [SyntaxTokenKind.Number] = "#0550AE",
            [SyntaxTokenKind.Comment] = "#6E7781",
            [SyntaxTokenKind.Tag] = "#116329",
            [SyntaxTokenKind.Attribute] = "#0550AE"
        };

        /// <summary>
        /// Custom renderers for fenced code blocks by language name, e.g. "mermaid" or "math".
        /// The renderer receives the container and the code; it replaces the code box entirely.
        /// </summary>
        public Dictionary<string, Action<IContainer, string>> CodeBlockRenderers { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Title and color of alert blocks (<c>&gt; [!NOTE]</c>, ...) by alert kind.</summary>
        public Dictionary<string, MarkdownAlertStyle> AlertStyles { get; } = new(StringComparer.OrdinalIgnoreCase)
        {
            ["NOTE"] = new("Note", "#0969DA"),
            ["TIP"] = new("Tip", "#1A7F37"),
            ["IMPORTANT"] = new("Important", "#8250DF"),
            ["WARNING"] = new("Warning", "#9A6700"),
            ["CAUTION"] = new("Caution", "#CF222E")
        };
    }
}
