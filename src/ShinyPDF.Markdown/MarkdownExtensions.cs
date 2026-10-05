using System;
using ShinyPDF.Infrastructure;
using ShinyPDF.Markdown;

namespace ShinyPDF.Fluent
{
    public static class MarkdownExtensions
    {
        /// <summary>
        /// Renders Markdown (CommonMark plus ~~strikethrough~~) as PDF content.
        /// Supported: headings, paragraphs, bold, italic, strikethrough, inline code, code blocks,
        /// links, bullet and numbered lists, blockquotes and horizontal rules.
        /// Raw HTML is shown as plain text; images are replaced by their alternative text.
        /// </summary>
        public static void Markdown(this IContainer container, string markdown, Action<MarkdownOptions>? configure = null)
        {
            if (markdown == null)
                throw new ArgumentNullException(nameof(markdown));

            var options = new MarkdownOptions();
            configure?.Invoke(options);

            new MarkdownRenderer(options).Compose(container, markdown);
        }
    }
}
