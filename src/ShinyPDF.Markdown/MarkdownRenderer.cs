using System;
using System.Collections.Generic;
using System.Linq;
using Markdig;
using Markdig.Extensions.Alerts;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using ShinyPDF.Elements.Table;
using ShinyPDF.Fluent;
using ShinyPDF.Infrastructure;
using SkiaSharp;

namespace ShinyPDF.Markdown
{
    internal class MarkdownRenderer
    {
        private const string Bullet = "•";

        private static readonly float[] HeadingScale = { 2f, 1.6f, 1.35f, 1.15f, 1f, 0.9f };

        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseEmphasisExtras(Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Strikethrough)
            .UsePipeTables()
            .UseTaskLists()
            .UseAlertBlocks()
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

        private void ComposeBlocks(IContainer container, ContainerBlock blocks, float spacing, HorizontalAlignment? textAlignment = null)
        {
            var visibleBlocks = blocks.Where(IsRendered).ToList();

            container.Column(column =>
            {
                column.Spacing(spacing);

                foreach (var block in visibleBlocks)
                    ComposeBlock(column.Item(), block, textAlignment);
            });
        }

        private static bool IsRendered(Block block)
        {
            return block switch
            {
                LinkReferenceDefinitionGroup or BlankLineBlock => false,
                ParagraphBlock paragraph => paragraph.Inline?.FirstChild != null,
                _ => true
            };
        }

        private void ComposeBlock(IContainer container, Block block, HorizontalAlignment? textAlignment)
        {
            switch (block)
            {
                case HeadingBlock heading:
                    ComposeText(container, heading.Inline, GetHeadingStyle(heading.Level), textAlignment);
                    break;

                case ParagraphBlock paragraph:
                    ComposeText(container, paragraph.Inline, TextStyle.Default, textAlignment);
                    break;

                case ListBlock list:
                    ComposeList(container, list);
                    break;

                case AlertBlock alert:
                    ComposeAlert(container, alert);
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

                case Table table:
                    ComposeTable(container, table);
                    break;

                case ContainerBlock other:
                    ComposeBlocks(container, other, Options.BlockSpacing);
                    break;

                case LeafBlock leaf:
                    ComposeText(container, leaf.Inline, TextStyle.Default, textAlignment);
                    break;
            }
        }

        private void ComposeText(IContainer container, ContainerInline? inlines, TextStyle style, HorizontalAlignment? alignment)
        {
            container.Text(text =>
            {
                // aligned per line, so wrapped text in centered or right-aligned table columns lines up
                if (alignment == HorizontalAlignment.Center)
                    text.AlignCenter();
                else if (alignment == HorizontalAlignment.Right)
                    text.AlignRight();

                ComposeInlines(text, inlines, style, null);
            });
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
                    var task = GetTask(item);

                    column.Item().Row(row =>
                    {
                        if (task != null)
                            row.ConstantItem(Options.ListIndent).Element(x => ComposeCheckbox(x, task.Checked));
                        else
                            row.ConstantItem(Options.ListIndent).Text(marker);

                        row.RelativeItem().Element(x => ComposeBlocks(x, item, Options.ListItemSpacing));
                    });
                }
            });
        }

        private static TaskList? GetTask(ListItemBlock item)
        {
            return (item.FirstOrDefault() as ParagraphBlock)?.Inline?.FirstChild as TaskList;
        }

        private void ComposeCheckbox(IContainer container, bool isChecked)
        {
            var size = Options.BaseFontSize * 0.75f;

            var box = container
                .PaddingTop(Options.BaseFontSize * 0.25f)
                .AlignLeft()
                .AlignTop()
                .Width(size)
                .Height(size)
                .Border(1)
                .BorderColor(Options.CheckboxColor);

            if (!isChecked)
                return;

            box.Background(Options.CheckboxColor).Canvas((canvas, space) =>
            {
                using var paint = new SKPaint
                {
                    Color = SKColors.White,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = space.Width * 0.15f,
                    StrokeCap = SKStrokeCap.Round,
                    IsAntialias = true
                };

                var corner = new SKPoint(space.Width * 0.42f, space.Height * 0.72f);
                canvas.DrawLine(new SKPoint(space.Width * 0.22f, space.Height * 0.52f), corner, paint);
                canvas.DrawLine(corner, new SKPoint(space.Width * 0.78f, space.Height * 0.3f), paint);
            });
        }

        private void ComposeAlert(IContainer container, AlertBlock alert)
        {
            var kind = alert.Kind.ToString();

            if (!Options.AlertStyles.TryGetValue(kind, out var style))
                style = new MarkdownAlertStyle(ToTitleCase(kind), Options.BlockquoteBorderColor);

            container
                .BorderLeft(3)
                .BorderColor(style.Color)
                .PaddingLeft(10)
                .Column(column =>
                {
                    column.Spacing(Options.BlockSpacing / 2);
                    column.Item().Text(style.Title).Style(TextStyle.Default.Bold().FontColor(style.Color));

                    if (alert.Any(IsRendered))
                        column.Item().Element(x => ComposeBlocks(x, alert, Options.BlockSpacing));
                });

            static string ToTitleCase(string value)
            {
                return value.Length == 0 ? value : char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
            }
        }

        private void ComposeCodeBlock(IContainer container, CodeBlock code)
        {
            var lines = code.Lines.Lines
                .Take(code.Lines.Count)
                .Select(x => x.Slice.ToString());

            var content = string.Join("\n", lines);
            var language = (code as FencedCodeBlock)?.Info?.Trim() ?? string.Empty;

            if (Options.CodeBlockRenderers.TryGetValue(language, out var renderer))
            {
                renderer(container, content);
                return;
            }

            var codeStyle = TextStyle.Default
                .FontFamily(Options.CodeFontFamily)
                .FontSize(Options.BaseFontSize * 0.9f);

            var syntax = language.Length == 0 ? null : Options.CodeLanguages.LastOrDefault(x => x.HasName(language));
            var tokens = syntax?.Tokenize(content) ?? new[] { ((SyntaxTokenKind?)null, content) };

            container
                .Background(Options.CodeBackgroundColor)
                .Padding(8)
                .Text(text =>
                {
                    foreach (var (kind, value) in tokens)
                    {
                        var style = kind != null && Options.SyntaxColors.TryGetValue(kind.Value, out var color)
                            ? codeStyle.FontColor(color)
                            : codeStyle;

                        text.Span(value).Style(style);
                    }
                });
        }

        private void ComposeTable(IContainer container, Table table)
        {
            var rows = table.OfType<TableRow>().ToList();
            var columnCount = rows
                .Select(row => row.OfType<TableCell>().Sum(cell => Math.Max(1, cell.ColumnSpan)))
                .DefaultIfEmpty(0)
                .Max();

            if (columnCount == 0)
                return;

            var headerRows = rows.TakeWhile(x => x.IsHeader).ToList();
            var bodyRows = rows.Skip(headerRows.Count).ToList();

            container.Table(descriptor =>
            {
                descriptor.ColumnsDefinition(columns =>
                {
                    for (var i = 0; i < columnCount; i++)
                        columns.RelativeColumn();
                });

                // the header repeats on every page the table spans
                if (headerRows.Any())
                    descriptor.Header(header => ComposeTableRows(table, headerRows, header.Cell, true));

                ComposeTableRows(table, bodyRows, descriptor.Cell, false);
            });
        }

        private void ComposeTableRows(Table table, List<TableRow> rows, Func<ITableCellContainer> createCell, bool isHeader)
        {
            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var columnIndex = 0;

                foreach (var cell in rows[rowIndex].OfType<TableCell>())
                {
                    if (cell.ColumnIndex >= 0)
                        columnIndex = cell.ColumnIndex;

                    var columnSpan = Math.Max(1, cell.ColumnSpan);

                    IContainer cellContainer = createCell()
                        .Row((uint)rowIndex + 1)
                        .Column((uint)columnIndex + 1)
                        .ColumnSpan((uint)columnSpan)
                        .Border(0.5f)
                        .BorderColor(Options.TableBorderColor);

                    if (isHeader)
                    {
                        cellContainer = cellContainer
                            .Background(Options.TableHeaderBackgroundColor)
                            .DefaultTextStyle(x => x.Bold());
                    }

                    cellContainer = cellContainer.Padding(Options.TableCellPadding);

                    var alignment = columnIndex < table.ColumnDefinitions.Count
                        ? table.ColumnDefinitions[columnIndex].Alignment
                        : null;

                    var textAlignment = alignment switch
                    {
                        TableColumnAlign.Center => HorizontalAlignment.Center,
                        TableColumnAlign.Right => HorizontalAlignment.Right,
                        _ => (HorizontalAlignment?)null
                    };

                    ComposeBlocks(cellContainer, cell, Options.ListItemSpacing, textAlignment);
                    columnIndex += columnSpan;
                }
            }
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
                case TaskList:
                    // rendered as the checkbox of its list item
                    break;

                case LiteralInline literal when literal.PreviousSibling is TaskList:
                    AddSpan(text, literal.Content.ToString().TrimStart(), style, url);
                    break;

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
                    ComposeImage(text, image, style, url);
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

        private void ComposeImage(TextDescriptor text, LinkInline image, TextStyle style, string? url)
        {
            var data = LoadImage(image.Url);
            var size = data == null ? null : DecodeImageSize(data);

            if (data == null || size == null)
            {
                // unavailable or undecodable image, show its alternative text instead
                ComposeInlines(text, image, style, url);
                return;
            }

            // natural size (one pixel per point), scaled down to MaxImageHeight and to the available width;
            // inline elements are measured without a height limit, so the height has to be bounded here
            var (width, height) = size.Value;
            var maxWidth = Math.Min(width, Options.MaxImageHeight * width / height);

            var container = text.Element();

            if (!string.IsNullOrWhiteSpace(url))
                container = container.Hyperlink(url);

            container.MaxWidth(maxWidth).Image(data, ImageScaling.FitWidth);
        }

        private byte[]? LoadImage(string? source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return null;

            if (source.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                return DecodeDataUri(source);

            return Options.ImageResolver?.Invoke(source);
        }

        private static byte[]? DecodeDataUri(string uri)
        {
            var separator = uri.IndexOf(',');

            if (separator < 0 || !uri[..separator].EndsWith(";base64", StringComparison.OrdinalIgnoreCase))
                return null;

            try
            {
                return Convert.FromBase64String(uri[(separator + 1)..]);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private static (float Width, float Height)? DecodeImageSize(byte[] data)
        {
            using var stream = new SKMemoryStream(data);
            using var codec = SKCodec.Create(stream);

            if (codec == null || codec.Info.Width == 0 || codec.Info.Height == 0)
                return null;

            // decode all pixels: a valid header alone does not guarantee the image is complete
            var info = new SKImageInfo(codec.Info.Width, codec.Info.Height);
            using var bitmap = new SKBitmap(info);

            if (codec.GetPixels(info, bitmap.GetPixels()) != SKCodecResult.Success)
                return null;

            return (info.Width, info.Height);
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
