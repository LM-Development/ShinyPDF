using System;
using System.Collections.Generic;
using System.Linq;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using ShinyPDF.Fluent;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Markdown
{
    internal class MarkdownRenderer
    {
        private const string Bullet = "•";

        private static readonly float[] HeadingScale = { 2f, 1.6f, 1.35f, 1.15f, 1f, 0.9f };

        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseEmphasisExtras(Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Strikethrough)
            .DisableHtml()
            .Build();

        private MarkdownOptions Options { get; }

        internal MarkdownRenderer(MarkdownOptions options)
        {
            Options = options;
        }

        internal void Compose(IContainer container, string markdown)
        {
            var document = Markdig.Markdown.Parse(markdown, Pipeline);

            container
                .DefaultTextStyle(x => x.FontSize(Options.BaseFontSize))
                .Element(x => ComposeBlocks(x, document, Options.BlockSpacing));
        }

        private void ComposeBlocks(IContainer container, ContainerBlock blocks, float spacing)
        {
            var visibleBlocks = blocks.Where(IsRendered).ToList();

            container.Column(column =>
            {
                column.Spacing(spacing);

                foreach (var block in visibleBlocks)
                    ComposeBlock(column.Item(), block);
            });
        }

        private static bool IsRendered(Block block)
        {
            return block is not (LinkReferenceDefinitionGroup or BlankLineBlock);
        }

        private void ComposeBlock(IContainer container, Block block)
        {
            switch (block)
            {
                case HeadingBlock heading:
                    container.Text(text => ComposeInlines(text, heading.Inline, GetHeadingStyle(heading.Level), null));
                    break;

                case ParagraphBlock paragraph:
                    container.Text(text => ComposeInlines(text, paragraph.Inline, TextStyle.Default, null));
                    break;

                case ListBlock list:
                    ComposeList(container, list);
                    break;

                case QuoteBlock quote:
                    container
                        .BorderLeft(3)
                        .BorderColor(Options.BlockquoteBorderColor)
                        .PaddingLeft(10)
                        .DefaultTextStyle(x => x.FontColor(Options.BlockquoteTextColor))
                        .Element(x => ComposeBlocks(x, quote, Options.BlockSpacing));
                    break;

                case ThematicBreakBlock:
                    container.PaddingVertical(4).LineHorizontal(1).LineColor(Options.HorizontalRuleColor);
                    break;

                case CodeBlock code:
                    ComposeCodeBlock(container, code);
                    break;

                case ContainerBlock other:
                    ComposeBlocks(container, other, Options.BlockSpacing);
                    break;

                case LeafBlock leaf:
                    container.Text(text => ComposeInlines(text, leaf.Inline, TextStyle.Default, null));
                    break;
            }
        }

        private void ComposeList(IContainer container, ListBlock list)
        {
            var number = list.IsOrdered && int.TryParse(list.OrderedStart, out var start) ? start : 1;

            container.Column(column =>
            {
                column.Spacing(Options.ListItemSpacing);

                foreach (var item in list.OfType<ListItemBlock>())
                {
                    var marker = list.IsOrdered ? $"{number++}{list.OrderedDelimiter}" : Bullet;

                    column.Item().Row(row =>
                    {
                        row.ConstantItem(Options.ListIndent).Text(marker);
                        row.RelativeItem().Element(x => ComposeBlocks(x, item, Options.ListItemSpacing));
                    });
                }
            });
        }

        private void ComposeCodeBlock(IContainer container, CodeBlock code)
        {
            var lines = code.Lines.Lines
                .Take(code.Lines.Count)
                .Select(x => x.Slice.ToString());

            var codeStyle = TextStyle.Default
                .FontFamily(Options.CodeFontFamily)
                .FontSize(Options.BaseFontSize * 0.9f);

            container
                .Background(Options.CodeBackgroundColor)
                .Padding(8)
                .Text(string.Join("\n", lines))
                .Style(codeStyle);
        }

        private TextStyle GetHeadingStyle(int level)
        {
            if (Options.HeadingStyle != null)
                return Options.HeadingStyle(level);

            var scale = HeadingScale[Math.Clamp(level, 1, HeadingScale.Length) - 1];
            return TextStyle.Default.FontSize(Options.BaseFontSize * scale).Bold();
        }

        private void ComposeInlines(TextDescriptor text, ContainerInline? inlines, TextStyle style, string? url)
        {
            if (inlines == null)
                return;

            foreach (var inline in inlines)
                ComposeInline(text, inline, style, url);
        }

        private void ComposeInline(TextDescriptor text, Inline inline, TextStyle style, string? url)
        {
            switch (inline)
            {
                case LiteralInline literal:
                    AddSpan(text, literal.Content.ToString(), style, url);
                    break;

                case HtmlEntityInline entity:
                    AddSpan(text, entity.Transcoded.ToString(), style, url);
                    break;

                case CodeInline code:
                    AddSpan(text, code.Content, style.FontFamily(Options.CodeFontFamily).BackgroundColor(Options.CodeBackgroundColor), url);
                    break;

                case LineBreakInline lineBreak:
                    AddSpan(text, lineBreak.IsHard ? "\n" : " ", style, url);
                    break;

                case EmphasisInline emphasis:
                    ComposeInlines(text, emphasis, ApplyEmphasis(style, emphasis), url);
                    break;

                case LinkInline { IsImage: true } image:
                    // images are not supported yet, show their alternative text instead
                    ComposeInlines(text, image, style, url);
                    break;

                case LinkInline link:
                    ComposeLink(text, link, link.Url, style);
                    break;

                case AutolinkInline autolink:
                    var target = autolink.IsEmail ? $"mailto:{autolink.Url}" : autolink.Url;
                    AddSpan(text, autolink.Url, GetLinkStyle(style), target);
                    break;

                case ContainerInline container:
                    ComposeInlines(text, container, style, url);
                    break;

                case LeafInline leaf:
                    AddSpan(text, leaf.ToString(), style, url);
                    break;
            }
        }

        private void ComposeLink(TextDescriptor text, LinkInline link, string? url, TextStyle style)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                ComposeInlines(text, link, style, null);
                return;
            }

            ComposeInlines(text, link, GetLinkStyle(style), url);
        }

        private TextStyle GetLinkStyle(TextStyle style)
        {
            return style.FontColor(Options.LinkColor).Underline();
        }

        private static TextStyle ApplyEmphasis(TextStyle style, EmphasisInline emphasis)
        {
            return emphasis.DelimiterChar switch
            {
                '~' => style.Strikethrough(),
                _ when emphasis.DelimiterCount >= 2 => style.Bold(),
                _ => style.Italic()
            };
        }

        private static void AddSpan(TextDescriptor text, string? content, TextStyle style, string? url)
        {
            if (string.IsNullOrEmpty(content))
                return;

            if (url != null)
                text.Hyperlink(content, url).Style(style);
            else
                text.Span(content).Style(style);
        }
    }
}
