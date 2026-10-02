# Headers, footers and page numbers

This guide shows how to set up pages in ShinyPDF: page size and orientation, margins, the header, content and footer slots, background and foreground layers, and page numbers. It also shows how to give the first page a different header with `ShowOnce` and `SkipOnce`.

## The page slots

You configure a page inside `container.Page(page => ...)`. The `PageDescriptor` you receive has five slots, each returning an `IContainer` that takes one child element:

- `page.Header()`: drawn at the top of every page.
- `page.Content()`: the main content. It flows from page to page; ShinyPDF adds pages until all content is placed.
- `page.Footer()`: drawn at the bottom of every page.
- `page.Background()`: a layer behind the page that covers the whole page, including the margins.
- `page.Foreground()`: a layer in front of the page, also covering the whole page.

```csharp
using ShinyPDF.Fluent;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;

Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(2, Unit.Centimetre);
        page.PageColor(Colors.White);
        page.DefaultTextStyle(x => x.FontSize(11));

        page.Header()
            .PaddingBottom(10)
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten1)
            .Text("Quarterly report").FontSize(18).SemiBold();

        page.Content()
            .PaddingVertical(10)
            .Text(Placeholders.Paragraphs());

        page.Footer()
            .AlignCenter()
            .Text(text =>
            {
                text.Span("Page ");
                text.CurrentPageNumber();
                text.Span(" of ");
                text.TotalPages();
            });
    });
})
.GeneratePdf("report.pdf");
```

The header and footer take the height they need on each page; the content gets the space in between. Keep the header and footer small: if they leave no room for the content, ShinyPDF cannot place it and fails with a `DocumentLayoutException` (see [Debug layout issues](debug-layout-issues.md)).

Every slot is optional. A slot you do not use stays empty.

## Page size and orientation

`page.Size(...)` sets a fixed page size. If you do not call it, pages are A4 portrait.

```csharp
Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.Letter);
        page.Content().Text("US Letter");
    });
});
```

- `PageSizes` (namespace `ShinyPDF.Helpers`) has predefined sizes: the ISO A and B series (`A0` to `A10`, `B0` to `B10`), the envelopes `Env10`, `EnvC4` and `EnvDL`, the North American sizes `Letter`, `Legal` and `Executive`, and the architectural sizes `ARCH_A` to `ARCH_E3`. All predefined sizes are portrait.
- `.Landscape()` and `.Portrait()` are extension methods on `PageSize` that swap width and height when needed: `PageSizes.A4.Landscape()`.
- `page.Size(width, height, unit)` sets a custom size, for example `page.Size(10, 15, Unit.Centimetre)`. You can also create a reusable `new PageSize(10, 15, Unit.Centimetre)`.

```csharp
Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4.Landscape());
        page.Content().Text("A4 landscape");
    });

    container.Page(page =>
    {
        page.Size(105, 148, Unit.Millimetre);
        page.Content().Text("Custom size");
    });
});
```

As the example shows, you can call `container.Page(...)` several times. Each call starts a new page set with its own size, margins and slots, and always begins on a new page. Page numbers continue across page sets, and `TotalPages()` counts all pages of the document.

For special cases, `page.ContinuousSize(width, unit)` creates a single page with a fixed width whose height grows with the content (useful for receipts or long images), and `page.MinSize(...)` and `page.MaxSize(...)` let the page size vary between two limits.

## Margins

Margins are the space between the page edge and the header, content and footer. They default to 0.

```csharp
Document.Create(container =>
{
    container.Page(page =>
    {
        page.MarginVertical(2, Unit.Centimetre);
        page.MarginHorizontal(25, Unit.Millimetre);

        page.Content().Text(Placeholders.Paragraph());
    });
});
```

The available methods are `Margin` (all sides), `MarginVertical`, `MarginHorizontal`, `MarginTop`, `MarginBottom`, `MarginLeft` and `MarginRight`. All take a value and an optional `Unit` (`Unit.Point` by default; also `Millimetre`, `Centimetre`, `Meter`, `Inch`, `Feet` and `Mill`, a thousandth of an inch). Later calls override earlier ones for the same side, so `page.Margin(20); page.MarginTop(40);` gives a larger top margin.

To add space only between the header and the content, use padding inside the slot, for example `page.Content().PaddingVertical(10)`.

## Background and foreground

`Background()` and `Foreground()` are drawn on every page and ignore the margins. Use them for watermarks, colored bands or decorative frames.

```csharp
Document.Create(container =>
{
    container.Page(page =>
    {
        page.Margin(2, Unit.Centimetre);

        page.Background()
            .AlignTop()
            .ExtendHorizontal()
            .Height(80)
            .Background(Colors.Blue.Lighten4);

        page.Foreground()
            .AlignMiddle()
            .AlignCenter()
            .Text("DRAFT").FontSize(72).Bold().FontColor("#33ff0000");

        page.Content().Text(Placeholders.Paragraphs());
    });
});
```

Colors are hexadecimal strings; an 8-digit value such as `#33ff0000` includes an alpha channel, which makes the foreground text semi-transparent. For a plain page color, use `page.PageColor(...)` instead of the background slot.

## Page numbers

Page numbers are special text spans. Add them inside a `Text(text => ...)` block, usually in the footer or header:

| Method | Value |
| --- | --- |
| `CurrentPageNumber()` | The number of the current page, starting at 1. |
| `TotalPages()` | The number of pages in the whole document. |

```csharp
container.AlignRight().Text(text =>
{
    text.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken1));

    text.Span("Page ");
    text.CurrentPageNumber().SemiBold();
    text.Span(" / ");
    text.TotalPages();
});
```

The page number methods return a `TextPageNumberDescriptor`, so you can style them like any other span. With `Format(...)` you control how the number is printed. The formatter receives the number as `int?` and returns a string:

```csharp
container.Text(text =>
{
    text.Span("Page ");
    text.CurrentPageNumber().Format(number => number?.ToString("00") ?? "-");
});
```

### Page numbers per section

For page numbers relative to a part of the document (for example "Chapter page 2 of 5", or a table of contents), mark that part with `Section(name)` and use the section variants:

| Method | Value |
| --- | --- |
| `BeginPageNumberOfSection(name)` | The page on which the section starts. |
| `EndPageNumberOfSection(name)` | The page on which the section ends. |
| `PageNumberWithinSection(name)` | The current page number, counted from the start of the section. |
| `TotalPagesWithinSection(name)` | The number of pages the section spans. |

```csharp
container.Column(column =>
{
    column.Item().Text(text =>
    {
        text.Span("Appendix starts on page ");
        text.BeginPageNumberOfSection("appendix");
    });

    column.Item().PageBreak();

    column.Item().Section("appendix").Column(appendix =>
    {
        appendix.Item().Text("Appendix").FontSize(18).SemiBold();
        appendix.Item().Text(Placeholders.Paragraphs());
    });
});
```

`Section` also makes the area a link target: `SectionLink(name)` on a container, or `text.SectionLink(text, name)` inside a text block, creates a link that jumps to it. Give each section its own name. Avoid the name `document`, which ShinyPDF uses internally for `TotalPages()`.

## A different header on the first page

A common requirement is a large header on the first page and a compact one on all following pages. Because the header is drawn on every page, you put both variants into it and use `ShowOnce` and `SkipOnce`:

- `ShowOnce()` draws its content only the first time, on the first page where it appears.
- `SkipOnce()` hides its content the first time and draws it from the second time on.

```csharp
Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A5);
        page.Margin(1, Unit.Centimetre);
        page.PageColor(Colors.White);

        page.Header().Column(column =>
        {
            column.Item().ShowOnce().Column(first =>
            {
                first.Item().Text("Shiny Consulting GmbH").FontSize(24).SemiBold().FontColor(Colors.Blue.Darken2);
                first.Item().Text("Project proposal, October 2026").FontColor(Colors.Grey.Darken1);
            });

            column.Item().SkipOnce().Text("Project proposal (continued)").FontSize(10).FontColor(Colors.Grey.Darken1);
        });

        page.Content().PaddingVertical(10).Column(column =>
        {
            column.Spacing(10);

            foreach (var _ in Enumerable.Range(0, 10))
                column.Item().Text(Placeholders.Paragraph());
        });

        page.Footer().AlignCenter().Text(text =>
        {
            text.CurrentPageNumber();
            text.Span(" / ");
            text.TotalPages();
        });
    });
})
.GeneratePdf("proposal.pdf");
```

On page 1 the header shows the company name and subtitle; from page 2 on it shows the single "continued" line. The same technique works in the footer, for example to show terms and conditions only on the first page.

`ShowOnce` and `SkipOnce` are not limited to headers. Inside the content, `ShowOnce` prevents an element from being repeated when its parent continues on the next page, for example a label in a row next to a long paragraph.

## Related

- [Getting started](../tutorials/getting-started.md) builds a complete invoice with header, table and footer.
- [Elements reference](../reference/elements.md) lists all layout elements, including `PageBreak`, `EnsureSpace` and `ShowEntire` for controlling page breaks.
- [Text reference](../reference/text.md) covers text styles and spans.
- [Layout engine](../explanation/layout-engine.md) explains how content is split across pages.
