using System;
using ShinyPDF.Infrastructure;
using ShinyPDF.Markdown;

namespace ShinyPDF.Fluent
{
    public static class MarkdownExtensions
    {
        /// <summary>
        /// Renders Markdown (CommonMark plus GitHub extensions) as PDF content.
        /// Supported: headings, paragraphs, bold, italic, strikethrough, inline code, highlighted code blocks,
        /// links, images, bullet, numbered and task lists, tables, blockquotes, alerts and horizontal rules.
        /// Raw HTML is shown as plain text. Images are loaded from <c>data:</c> URIs or through
        /// <see cref="MarkdownOptions.ImageResolver"/>; otherwise their alternative text is shown.
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
