# Layout elements reference

This page lists every layout element of the ShinyPDF Fluent API: what it does, how it behaves when content flows over several pages, its method signatures and a short example. All methods are extension methods in the `ShinyPDF.Fluent` namespace and operate on `IContainer` (namespace `ShinyPDF.Infrastructure`). Text and text styling are covered in [Text reference](text.md); page size, margins and the page slots (header, content, footer) are covered in [Getting started](../tutorials/getting-started.md) and [Headers, footers and page numbers](../how-to/headers-footers-page-numbers.md).

## Contents

- [Conventions](#conventions)
- [Layout containers](#layout-containers): [Column](#column), [Row](#row), [Table](#table), [Layers](#layers), [Inlined](#inlined), [Decoration](#decoration), [Grid](#grid)
- [Sizing](#sizing): [Width, Height, Min and Max](#width-height-min-and-max), [Extend](#extend), [AspectRatio](#aspectratio), [ScaleToFit](#scaletofit), [MinimalBox](#minimalbox), [Unconstrained](#unconstrained)
- [Spacing and positioning](#spacing-and-positioning): [Padding](#padding), [Alignment](#alignment), [Translate](#translate), [Rotate](#rotate), [Scale and Flip](#scale-and-flip)
- [Visual](#visual): [Background](#background), [Border](#border), [Line](#line), [Image](#image), [Canvas](#canvas), [Placeholder](#placeholder)
- [Paging and flow control](#paging-and-flow-control): [PageBreak](#pagebreak), [EnsureSpace](#ensurespace), [ShowEntire](#showentire), [ShowOnce](#showonce), [SkipOnce](#skiponce), [ShowIf](#showif), [StopPaging](#stoppaging)
- [Navigation](#navigation): [Hyperlink](#hyperlink), [Section](#section), [SectionLink](#sectionlink)
- [Content direction](#content-direction)
- [Composition helpers](#composition-helpers): [Container and Element](#container-and-element), [DefaultTextStyle](#defaulttextstyle), [Components](#components), [Dynamic components](#dynamic-components)
- [Debugging](#debugging): [DebugArea](#debugarea), [DebugPointer](#debugpointer)

## Conventions

**Containers and chaining.** Most methods wrap the following content and return a new `IContainer`, so they chain: `container.Padding(10).Background(Colors.Grey.Lighten3).Text("Hello")`. Every container accepts exactly one child; assigning a second child to the same container throws a `DocumentComposeException`. Methods that return `void` (for example `Column`, `Row`, `Table`, `Image`, `PageBreak`) end a chain. Methods that take a handler (`Column`, `Row`, `Table`, ...) hold several children through their descriptor.

**Order matters.** Elements are applied from the outside in. `Padding(10).Background(...)` paints the background inside the padding, `Background(...).Padding(10)` paints it under the padding as well.

**Units.** Every size parameter is a `float` in points (1/72 inch) unless an optional `Unit unit = Unit.Point` parameter is offered. The `Unit` enum (namespace `ShinyPDF.Infrastructure`) has the values `Point`, `Meter`, `Centimetre`, `Millimetre`, `Feet`, `Inch` and `Mill` (1/1000 inch). Signatures below show the parameter as `Unit unit = Unit.Point` where it exists; where it is missing, the value is in points.

**Paging.** The layout engine measures each element against the space left on the current page. An element either fits fully, fits partially (the rest continues on the next page) or does not fit at all (it moves to the next page). If an element never fits, even on an empty page, generation fails with a `DocumentLayoutException`. See [How the layout engine works](../explanation/layout-engine.md) and [Debug layout issues](../how-to/debug-layout-issues.md).

**Examples.** Fragments on this page use a variable `container` of type `IContainer`, which is what `page.Content()`, `page.Header()`, `column.Item()` and similar methods return:

```csharp
Document
    .Create(document =>
    {
        document.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);

            page.Content().Padding(10).Background(Colors.Grey.Lighten3).Text("Hello");
        });
    })
    .GeneratePdf("hello.pdf");
```

Colors are hex strings; the `Colors` helper (namespace `ShinyPDF.Helpers`) provides the Material Design palette, for example `Colors.Blue.Medium` or `Colors.Grey.Lighten3`. Methods that take a color validate it and throw on an invalid value.

## Layout containers

### Column

Stacks items vertically. Every item gets the full available width. When the page is full, the remaining items continue on the next page; an item that supports paging (text, nested columns, tables) is split, other items move to the next page as a whole.

| Member | Description |
| --- | --- |
| `void Column(this IContainer element, Action<ColumnDescriptor> handler)` | Creates the column. |
| `ColumnDescriptor.Spacing(float value, Unit unit = Unit.Point)` | Vertical space between items. Default 0. |
| `IContainer ColumnDescriptor.Item()` | Adds an item and returns its container. |

```csharp
container.Column(column =>
{
    column.Spacing(5);

    column.Item().Background(Colors.Grey.Lighten3).Padding(5).Text("First");
    column.Item().Background(Colors.Grey.Lighten2).Padding(5).Text("Second");

    foreach (var i in Enumerable.Range(1, 3))
        column.Item().Text($"Item {i}");
});
```

### Row

Places items horizontally. The width of each item depends on its type: constant (fixed width), relative (share of the remaining width) or auto (the width its content needs). Every item is drawn with the height of the tallest item. If any item does not fit on the page, the whole row moves to the next page; items that are split continue on the next page side by side.

| Member | Description |
| --- | --- |
| `void Row(this IContainer element, Action<RowDescriptor> handler)` | Creates the row. |
| `RowDescriptor.Spacing(float value)` | Horizontal space between items, in points (no `Unit` parameter). Default 0. |
| `IContainer RowDescriptor.ConstantItem(float size, Unit unit = Unit.Point)` | Item with a fixed width. |
| `IContainer RowDescriptor.RelativeItem(float size = 1)` | Item that takes `size` shares of the width left after constant items, auto items and spacing. |
| `IContainer RowDescriptor.AutoItem()` | Item as wide as its content, measured without width limit. Use it for short content such as labels or icons. |

```csharp
container.Row(row =>
{
    row.Spacing(10);

    row.ConstantItem(3, Unit.Centimetre).Background(Colors.Grey.Lighten2).Text("Fixed");
    row.RelativeItem().Background(Colors.Grey.Lighten3).Text("1 share");
    row.RelativeItem(2).Background(Colors.Grey.Lighten4).Text("2 shares");
    row.AutoItem().Text("Auto");
});
```

### Table

A grid of cells with column definitions, optional header and footer, cell spanning and explicit or automatic cell placement. Tables page row by row: a row that does not fit moves to the next page, and if a cell is split, the rest of its row continues on the next page. The header and footer are repeated on every page the table occupies and must fit on a page without splitting.

**Descriptor members (`TableDescriptor`):**

| Member | Description |
| --- | --- |
| `void Table(this IContainer element, Action<TableDescriptor> handler)` | Creates the table. |
| `ColumnsDefinition(Action<TableColumnsDefinitionDescriptor> handler)` | Defines the columns. Required: a table without columns throws a `DocumentComposeException`. |
| `Header(Action<TableCellDescriptor> handler)` | Cells repeated at the top of every page. |
| `Footer(Action<TableCellDescriptor> handler)` | Cells repeated at the bottom of every page. |
| `ITableCellContainer Cell()` | Adds a content cell. `TableCellDescriptor.Cell()` does the same inside `Header` and `Footer`. |
| `ExtendLastCellsToTableBottom()` | Stretches the last cells of each column to the bottom of the table on every page, so cell borders and backgrounds reach the table bottom. |

**Column definitions (`TableColumnsDefinitionDescriptor`):**

| Member | Description |
| --- | --- |
| `ConstantColumn(float width, Unit unit = Unit.Point)` | Column with a fixed width. |
| `RelativeColumn(float width = 1)` | Column that takes `width` shares of the width left after constant columns. |

**Cell positioning (`TableCellExtensions`, on `ITableCellContainer`):**

| Member | Description |
| --- | --- |
| `Row(uint value)` | Row position, starting at 1. |
| `Column(uint value)` | Column position, starting at 1. |
| `RowSpan(uint value)` | Number of rows the cell covers. Default 1. |
| `ColumnSpan(uint value)` | Number of columns the cell covers. Default 1. |

Cells without a position are placed automatically, left to right and top to bottom, into the next free slot after the previous cell; slots covered by spanning cells are skipped. If only one of `Row` and `Column` is set, the other defaults to 1. A cell that starts at, or spans into, a column that does not exist throws a `DocumentComposeException`. Call the positioning methods directly after `Cell()`, before any other element method, because the other methods return a plain `IContainer`.

Cells have no border or padding of their own. A common pattern is a local style function applied with `Element`:

```csharp
static IContainer CellStyle(IContainer cell) =>
    cell.Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5);

container.Table(table =>
{
    table.ColumnsDefinition(columns =>
    {
        columns.ConstantColumn(40);
        columns.RelativeColumn(3);
        columns.RelativeColumn();
    });

    table.Header(header =>
    {
        header.Cell().Element(CellStyle).Text("#");
        header.Cell().Element(CellStyle).Text("Product");
        header.Cell().Element(CellStyle).AlignRight().Text("Price");
    });

    foreach (var i in Enumerable.Range(1, 50))
    {
        table.Cell().Element(CellStyle).Text($"{i}");
        table.Cell().Element(CellStyle).Text(Placeholders.Label());
        table.Cell().Element(CellStyle).AlignRight().Text(Placeholders.Decimal());
    }

    table.Footer(footer =>
    {
        footer.Cell().ColumnSpan(3).Element(CellStyle).Text("Prices in EUR");
    });
});
```

Explicit positions and spans:

```csharp
container.Table(table =>
{
    table.ColumnsDefinition(columns =>
    {
        columns.RelativeColumn();
        columns.RelativeColumn();
        columns.RelativeColumn();
    });

    table.Cell().Row(1).Column(1).RowSpan(2).Background(Colors.Blue.Lighten3).Text("A (2 rows)");
    table.Cell().Row(1).Column(2).ColumnSpan(2).Background(Colors.Green.Lighten3).Text("B (2 columns)");
    table.Cell().Row(2).Column(3).Background(Colors.Orange.Lighten3).Text("C");
    table.Cell().Text("D"); // placed automatically in the next free slot
});
```

### Layers

Draws several layers on top of each other in the same space. Exactly one layer must be the primary layer: it determines the size and the paging of the whole element. The other layers get the same space on every page the primary layer occupies, and are drawn in declaration order (later layers on top). Zero or more than one primary layer throws a `DocumentComposeException`.

| Member | Description |
| --- | --- |
| `void Layers(this IContainer element, Action<LayersDescriptor> handler)` | Creates the layers element. |
| `IContainer LayersDescriptor.Layer()` | Adds a secondary layer. |
| `IContainer LayersDescriptor.PrimaryLayer()` | Adds the primary layer. |

```csharp
container.Layers(layers =>
{
    layers.Layer().Background(Colors.Grey.Lighten3);
    layers.PrimaryLayer().Padding(10).Text(Placeholders.Paragraph());
    layers.Layer().AlignBottom().AlignRight().Text("DRAFT").FontColor(Colors.Red.Medium);
});
```

### Inlined

Places items next to each other like words in a paragraph and wraps them into new lines when the width is used up. Items are never split; whole lines continue on the next page.

| Member | Description |
| --- | --- |
| `void Inlined(this IContainer element, Action<InlinedDescriptor> handler)` | Creates the element. |
| `Spacing(float value, Unit unit = Unit.Point)` | Sets vertical and horizontal spacing. |
| `VerticalSpacing(float value, Unit unit = Unit.Point)` | Space between lines. |
| `HorizontalSpacing(float value, Unit unit = Unit.Point)` | Space between items in a line. |
| `BaselineTop()`, `BaselineMiddle()`, `BaselineBottom()` | Vertical alignment of items within a line. |
| `AlignLeft()`, `AlignCenter()`, `AlignRight()`, `AlignJustify()`, `AlignSpaceAround()` | Horizontal distribution of items within a line. Default: left (right for right-to-left content). |
| `IContainer Item()` | Adds an item. |

```csharp
container.Inlined(inlined =>
{
    inlined.Spacing(5);
    inlined.BaselineMiddle();
    inlined.AlignCenter();

    foreach (var tag in new[] { "pdf", "dotnet", "fluent", "layout", "csharp" })
        inlined.Item().Background(Colors.Blue.Lighten4).PaddingHorizontal(6).PaddingVertical(2).Text(tag);
});
```

### Decoration

Splits a space into three parts: `Before` (top), `Content` and `After` (bottom). When the content spans several pages, `Before` and `After` are repeated on every page. `Before` and `After` must fit on a page without splitting. This is the element behind table headers and footers and is useful for section titles that repeat, for example "Orders (continued)".

| Member | Description |
| --- | --- |
| `void Decoration(this IContainer element, Action<DecorationDescriptor> handler)` | Creates the element. |
| `IContainer Before()` / `void Before(Action<IContainer> handler)` | Repeated top part. |
| `IContainer Content()` / `void Content(Action<IContainer> handler)` | Main content, pages normally. |
| `IContainer After()` / `void After(Action<IContainer> handler)` | Repeated bottom part. |

```csharp
container.Decoration(decoration =>
{
    decoration.Before().Background(Colors.Grey.Lighten3).Padding(5).Text("Orders");

    decoration.Content().Column(column =>
    {
        foreach (var i in Enumerable.Range(1, 100))
            column.Item().Text($"Order {i}");
    });

    decoration.After(after => after.AlignRight().Text("End of section"));
});
```

### Grid

Arranges items in rows of a fixed number of columns (12 by default); each item takes a number of those columns, and a new row starts when an item does not fit into the current one. Internally, a grid is a [Column](#column) of [Rows](#row) with relative items, so it pages like a column of rows.

> **Note:** `Grid` is currently marked `[Obsolete]`. The marker is inherited from QuestPDF, and calling `Grid` produces compiler warning CS0618.

| Member | Description |
| --- | --- |
| `void Grid(this IContainer element, Action<GridDescriptor> handler)` | Creates the grid. If the container was created by `AlignLeft()`, `AlignCenter()` or `AlignRight()`, that horizontal alignment is used as the grid alignment. |
| `Columns(int value = 12)` | Number of columns per row. |
| `Spacing(float value, Unit unit = Unit.Point)` | Sets vertical and horizontal spacing. |
| `VerticalSpacing(float value, Unit unit = Unit.Point)` | Space between rows. |
| `HorizontalSpacing(float value, Unit unit = Unit.Point)` | Space between items in a row. |
| `Alignment(HorizontalAlignment alignment)`, `AlignLeft()`, `AlignCenter()`, `AlignRight()` | Position of incomplete rows. Default: left. |
| `IContainer Item(int columns = 1)` | Adds an item spanning `columns` columns. |

```csharp
container.Grid(grid =>
{
    grid.Columns(12);
    grid.Spacing(5);
    grid.AlignCenter();

    grid.Item(8).Background(Colors.Blue.Lighten3).Height(40);
    grid.Item(4).Background(Colors.Blue.Lighten2).Height(40);
    grid.Item(6).Background(Colors.Green.Lighten3).Height(40);
});
```

## Sizing

### Width, Height, Min and Max

Constrain the size of the content. `Min*` makes the content at least that large; `Max*` limits the space the content may use. `Width` and `Height` set both minimum and maximum to the same value. If a minimum is larger than the space left on the page, the element moves to the next page. Content larger than a maximum must fit into it or page within it.

| Method | Description |
| --- | --- |
| `IContainer Width(float value, Unit unit = Unit.Point)` | Exact width. |
| `IContainer MinWidth(float value, Unit unit = Unit.Point)` | Minimum width. |
| `IContainer MaxWidth(float value, Unit unit = Unit.Point)` | Maximum width. |
| `IContainer Height(float value, Unit unit = Unit.Point)` | Exact height. |
| `IContainer MinHeight(float value, Unit unit = Unit.Point)` | Minimum height. |
| `IContainer MaxHeight(float value, Unit unit = Unit.Point)` | Maximum height. |

Calls chained directly after each other configure the same element.

```csharp
container.Column(column =>
{
    column.Item().Width(5, Unit.Centimetre).Height(2, Unit.Centimetre).Background(Colors.Blue.Lighten3);
    column.Item().MinHeight(50).MaxWidth(200).Background(Colors.Green.Lighten3).Text(Placeholders.Sentence());
});
```

### Extend

Makes the element take all available space in one or both directions instead of only the space its content needs. On a page, `ExtendVertical` fills the remaining height of the current page.

| Method | Description |
| --- | --- |
| `IContainer Extend()` | Extends in both directions. |
| `IContainer ExtendVertical()` | Takes the full available height. |
| `IContainer ExtendHorizontal()` | Takes the full available width. |

```csharp
container.ExtendVertical().Background(Colors.Grey.Lighten4).AlignBottom().Text("Bottom of the page");
```

### AspectRatio

Gives the content a size with a fixed width-to-height ratio. The content never pages: if the computed size does not fit into the remaining space, the element moves to the next page.

`IContainer AspectRatio(float ratio, AspectRatioOption option = AspectRatioOption.FitWidth)`

- `ratio`: width divided by height, for example `16f / 9`.
- `option` (enum `AspectRatioOption`): `FitWidth` uses the full available width and computes the height; `FitHeight` uses the full available height and computes the width; `FitArea` uses the largest size that fits both.

```csharp
container.AspectRatio(16f / 9).Background(Colors.Blue.Lighten3).AlignCenter().AlignMiddle().Text("16:9");
```

### ScaleToFit

Scales the content down until it fits completely into the available space, for example a long text in a fixed-size box. Content that already fits is not scaled; content is never scaled up. The scale is found by a binary search, and if the content does not fit even at a very small scale, the element moves to the next page.

`IContainer ScaleToFit()`

```csharp
container.Width(150).Height(40).Border(1).ScaleToFit().Text(Placeholders.Paragraph());
```

### MinimalBox

Draws the content with the size it actually needs instead of the full space offered by the parent. Useful in front of `Background` or `Border` so they wrap the content tightly instead of stretching to the full width.

`IContainer MinimalBox()`

```csharp
container.MinimalBox().Border(1).Padding(5).Text("Border fits the text");
```

### Unconstrained

Removes all size limits: the content is measured with unlimited space and takes no space in the parent layout. It is drawn from its position and can overflow the parent and the page. Useful for overlays such as stamps or markers. In right-to-left content it extends to the left.

`IContainer Unconstrained()`

```csharp
container.Column(column =>
{
    column.Item().Unconstrained().TranslateX(-20).Text("Margin note");
    column.Item().Text(Placeholders.Paragraph());
});
```

## Spacing and positioning

### Padding

Adds empty space around the content. Padding reduces the space available to the content; if no space is left, the element moves to the next page. Directly chained padding calls add up: `Padding(10).PaddingLeft(5)` results in 15 points on the left.

| Method | Description |
| --- | --- |
| `IContainer Padding(float value, Unit unit = Unit.Point)` | All four sides. |
| `IContainer PaddingHorizontal(float value, Unit unit = Unit.Point)` | Left and right. |
| `IContainer PaddingVertical(float value, Unit unit = Unit.Point)` | Top and bottom. |
| `IContainer PaddingTop(float value, Unit unit = Unit.Point)` | Top. |
| `IContainer PaddingBottom(float value, Unit unit = Unit.Point)` | Bottom. |
| `IContainer PaddingLeft(float value, Unit unit = Unit.Point)` | Left. |
| `IContainer PaddingRight(float value, Unit unit = Unit.Point)` | Right. |

```csharp
container.PaddingVertical(1, Unit.Centimetre).PaddingHorizontal(20).Background(Colors.Grey.Lighten3).Text("Padded");
```

### Alignment

Positions the content inside the available space. The content gets its own size in the aligned direction and the full space in the other direction. A horizontal and a vertical alignment chained directly configure the same element.

| Method | Description |
| --- | --- |
| `IContainer AlignLeft()`, `AlignCenter()`, `AlignRight()` | Horizontal alignment. |
| `IContainer AlignTop()`, `AlignMiddle()`, `AlignBottom()` | Vertical alignment. |

```csharp
container.Height(100).Background(Colors.Grey.Lighten3).AlignCenter().AlignMiddle().Text("Centered");
```

### Translate

Moves the content when it is drawn, without changing the layout: the space reserved for the content stays where it was, and the moved content may overlap neighbors. Positive values move right and down. Directly chained calls add up.

| Method | Description |
| --- | --- |
| `IContainer TranslateX(float value, Unit unit = Unit.Point)` | Horizontal offset. |
| `IContainer TranslateY(float value, Unit unit = Unit.Point)` | Vertical offset. |

```csharp
container.TranslateX(10).TranslateY(-5).Text("Shifted");
```

### Rotate

`RotateLeft` and `RotateRight` turn the content by 90 degrees and swap its width and height in the layout, so the parent reserves the rotated size. Calls add up: two `RotateRight` calls turn it upside down. `Rotate(angle)` turns the content by any angle, clockwise, around its top-left corner; it only affects drawing, the layout keeps the unrotated size.

| Method | Description |
| --- | --- |
| `IContainer RotateLeft()` | 90 degrees counterclockwise, affects layout. |
| `IContainer RotateRight()` | 90 degrees clockwise, affects layout. |
| `IContainer Rotate(float angle)` | Any angle in degrees, drawing only. |

```csharp
container.Row(row =>
{
    row.AutoItem().RotateLeft().Text("Vertical label");
    row.RelativeItem().Padding(10).Rotate(15).Text("Tilted text");
});
```

### Scale and Flip

Scales the content. Unlike `Rotate(angle)`, scaling affects the layout: the content is measured with the available space divided by the factor and reserves the scaled size. Negative factors mirror the content. Directly chained calls multiply.

| Method | Description |
| --- | --- |
| `IContainer Scale(float value)` | Scales both directions by `value` (1 = original size). |
| `IContainer ScaleHorizontal(float value)` | Scales horizontally. |
| `IContainer ScaleVertical(float value)` | Scales vertically. |
| `IContainer FlipHorizontal()` | Mirrors horizontally (`ScaleHorizontal(-1)`). |
| `IContainer FlipVertical()` | Mirrors vertically (`ScaleVertical(-1)`). |
| `IContainer FlipOver()` | Mirrors in both directions. |

```csharp
container.Column(column =>
{
    column.Item().Scale(0.5f).Text("Half size");
    column.Item().ScaleHorizontal(2).Text("Stretched");
    column.Item().FlipHorizontal().Text("Mirrored");
});
```

## Visual

### Background

Fills the space given to the element with a color, then draws the content on top. The background covers the full space offered by the parent (often the full width); combine it with [MinimalBox](#minimalbox) or an alignment to fit it to the content.

`IContainer Background(string color)`

```csharp
container.Background(Colors.Amber.Lighten4).Padding(10).Text("Highlighted");
```

### Border

Draws lines along the edges of the element. Borders take no layout space: each line is centered on the edge (half inside, half outside) and drawn over the content, so add [Padding](#padding) inside the border to keep content away from it. Default color: black.

| Method | Description |
| --- | --- |
| `IContainer Border(float value, Unit unit = Unit.Point)` | All four sides. |
| `IContainer BorderVertical(float value, Unit unit = Unit.Point)` | Left and right. |
| `IContainer BorderHorizontal(float value, Unit unit = Unit.Point)` | Top and bottom. |
| `IContainer BorderTop(float value, Unit unit = Unit.Point)` | Top. |
| `IContainer BorderBottom(float value, Unit unit = Unit.Point)` | Bottom. |
| `IContainer BorderLeft(float value, Unit unit = Unit.Point)` | Left. |
| `IContainer BorderRight(float value, Unit unit = Unit.Point)` | Right. |
| `IContainer BorderColor(string color)` | Line color. |

Chain the thickness and color calls directly so they configure the same border.

```csharp
container.BorderBottom(2).BorderColor(Colors.Blue.Medium).PaddingBottom(5).Text("Underlined block");
```

### Line

Draws a horizontal or vertical line. A horizontal line takes the full available width and `size` points of height; a vertical line takes the full available height and `size` points of width. Default color: black. The methods return `ILine` (namespace `ShinyPDF.Elements`), on which `LineColor` sets the color and ends the chain.

| Method | Description |
| --- | --- |
| `ILine LineHorizontal(float size, Unit unit = Unit.Point)` | Horizontal line of thickness `size`. |
| `ILine LineVertical(float size, Unit unit = Unit.Point)` | Vertical line of thickness `size`. |
| `void LineColor(this ILine descriptor, string value)` | Line color. |

```csharp
container.Column(column =>
{
    column.Item().Text("Above");
    column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);
    column.Item().Text("Below");
});
```

### Image

Draws a raster image (any format SkiaSharp can decode, for example PNG or JPEG). The image is never split across pages: if it does not fit into the remaining space, it moves to the next page. An image that cannot be loaded or decoded throws a `DocumentComposeException`.

| Method | Description |
| --- | --- |
| `void Image(byte[] imageData, ImageScaling scaling = ImageScaling.FitWidth)` | Image from bytes. |
| `void Image(string filePath, ImageScaling scaling = ImageScaling.FitWidth)` | Image from a file. |
| `void Image(Stream fileStream, ImageScaling scaling = ImageScaling.FitWidth)` | Image from a stream. |
| `void Image(Func<Size, byte[]> imageSource)` | Dynamic image, see below. |

`ImageScaling` (namespace `ShinyPDF.Infrastructure`): `FitWidth` uses the full available width and keeps the aspect ratio; `FitHeight` uses the full available height; `FitArea` uses the largest size that fits; `Resize` stretches the image to the available space and ignores the aspect ratio. Limit the size with [Width, Height, Min and Max](#width-height-min-and-max).

```csharp
var logo = File.ReadAllBytes("logo.png");

container.Row(row =>
{
    row.ConstantItem(80).Image(logo);
    row.RelativeItem().Height(60).Image("photo.jpg", ImageScaling.FitArea);
});
```

The dynamic overload calls `imageSource` while drawing, with the size (in points) the image gets, and expects encoded image bytes. Use it to render charts or other graphics in the exact target size. The image takes all available space, so constrain it.

```csharp
container.Height(150).Image(size => Placeholders.Image((int)size.Width, (int)size.Height));
```

### Canvas

Gives direct access to the SkiaSharp canvas for custom drawing. The handler has the delegate type `DrawOnCanvas(SKCanvas canvas, Size availableSpace)` (namespace `ShinyPDF.Elements`); coordinates start at the top-left corner of the element, in points. The element takes all available space; the canvas transformation is restored after the handler returns.

`void Canvas(DrawOnCanvas handler)`

```csharp
using SkiaSharp;

container.Height(100).Canvas((canvas, size) =>
{
    using var paint = new SKPaint
    {
        Color = SKColor.Parse(Colors.Blue.Medium),
        IsStroke = true,
        StrokeWidth = 2
    };

    canvas.DrawRoundRect(0, 0, size.Width, size.Height, 10, 10, paint);
});
```

### Placeholder

Draws a grey box with an image icon, or with the given text. Use it while designing a layout to mark areas whose content is not ready yet. It fills the space it is given, so set a size.

`void Placeholder(string? text = null)`

```csharp
container.Row(row =>
{
    row.RelativeItem().Height(100).Placeholder();
    row.RelativeItem().Height(100).Placeholder("Chart");
});
```

The `Placeholders` helper (namespace `ShinyPDF.Helpers`) generates random sample content: text (`LoremIpsum()`, `Label()`, `Sentence()`, `Paragraph()`, `Paragraphs()`), values (`Name()`, `Email()`, `PhoneNumber()`, `Integer()`, `Decimal()`, `Percent()`, dates and times), colors (`Color()`, `BackgroundColor()`) and images (`Image(int width, int height)`, `Image(Size size)`).

## Paging and flow control

### PageBreak

Ends the current page; the following content starts on the next page. Typically used as a column item.

`void PageBreak()`

```csharp
container.Column(column =>
{
    column.Item().Text("Chapter 1");
    column.Item().PageBreak();
    column.Item().Text("Chapter 2");
});
```

### EnsureSpace

Avoids starting content at the very bottom of a page. If the content would be split and less than `minHeight` points are left on the current page, the whole element moves to the next page. Content that fits completely stays where it is.

`IContainer EnsureSpace(float minHeight = 150)`

```csharp
container.Column(column =>
{
    column.Item().Text(Placeholders.Paragraphs());
    column.Item().EnsureSpace(100).Text(Placeholders.Paragraphs());
});
```

### ShowEntire

Prevents splitting: if the content does not fit completely into the remaining space, it moves to the next page. Content that is taller than a whole page then fails with a `DocumentLayoutException`.

`IContainer ShowEntire()`

```csharp
container.ShowEntire().Column(column =>
{
    column.Item().Text("Signature");
    column.Item().Height(60).Border(1);
});
```

### ShowOnce

Shows the content only the first time it is drawn completely. When the element is repeated on later pages (for example in a page header, a `Decoration` or a table header), it takes no space there.

`IContainer ShowOnce()`

### SkipOnce

The opposite of `ShowOnce`: hides the content the first time it is drawn and shows it on all later repetitions. Combined with `ShowOnce`, it creates "continued" labels:

`IContainer SkipOnce()`

```csharp
container.Decoration(decoration =>
{
    decoration.Before().Column(column =>
    {
        column.Item().ShowOnce().Text("Invoice items");
        column.Item().SkipOnce().Text("Invoice items (continued)");
    });

    decoration.Content().Column(column =>
    {
        foreach (var i in Enumerable.Range(1, 80))
            column.Item().Text($"Item {i}");
    });
});
```

### ShowIf

Includes the content only if `condition` is `true`. If it is `false`, the returned container is detached from the document and everything added to it is ignored.

`IContainer ShowIf(bool condition)`

```csharp
var isDraft = true;

container.Column(column =>
{
    column.Item().ShowIf(isDraft).Text("DRAFT").FontColor(Colors.Red.Medium);
    column.Item().Text("Document body");
});
```

### StopPaging

Limits the content to one page: only the part that fits into the current space is drawn, the rest is discarded. If nothing fits, the content is skipped and takes no space. Useful for previews or content that must not create extra pages.

`IContainer StopPaging()`

```csharp
container.Height(200).StopPaging().Text(Placeholders.Paragraphs());
```

## Navigation

### Hyperlink

Makes the area of the content a clickable link to an external URL. For a link on part of a text, use `Hyperlink` inside a text block, see [Text reference](text.md).

`IContainer Hyperlink(string url)`

```csharp
container.Hyperlink("https://github.com/LM-Development/ShinyPDF").Text("ShinyPDF on GitHub").FontColor(Colors.Blue.Medium);
```

### Section

Marks the content as a named location (a PDF destination) that `SectionLink` can jump to. The section name also feeds the section page number methods of text blocks (`BeginPageNumberOfSection`, `EndPageNumberOfSection`, `PageNumberWithinSection`, `TotalPagesWithinSection`, see [Text reference](text.md)).

`IContainer Section(string sectionName)`

### SectionLink

Makes the area of the content a clickable link that jumps to the section with the given name.

`IContainer SectionLink(string sectionName)`

```csharp
container.Column(column =>
{
    column.Item().SectionLink("details").Text("Go to details").FontColor(Colors.Blue.Medium);
    column.Item().PageBreak();
    column.Item().Section("details").Text("Details");
});
```

## Content direction

Sets the direction for the content and everything inside it. Right-to-left mirrors the order of row items and table columns, the default alignment of inlined items, and the placement of elements that do not fill the full width. The default is left-to-right; it can also be set for a whole page with `page.ContentFromRightToLeft()`.

| Method | Description |
| --- | --- |
| `IContainer ContentFromLeftToRight()` | Left-to-right layout (default). |
| `IContainer ContentFromRightToLeft()` | Right-to-left layout, for example for Arabic or Hebrew documents. |

```csharp
container.ContentFromRightToLeft().Row(row =>
{
    row.ConstantItem(50).Background(Colors.Red.Lighten3).Text("1");
    row.RelativeItem().Background(Colors.Green.Lighten3).Text("2");
});
```

## Composition helpers

### Container and Element

`Container()` inserts an empty wrapper and returns its container. `Element` applies a function to the container, which lets you extract and reuse a chain of styling calls.

| Method | Description |
| --- | --- |
| `IContainer Container()` | Adds a neutral container. |
| `void Element(Action<IContainer> handler)` | Passes the container to `handler`. |
| `IContainer Element(Func<IContainer, IContainer> handler)` | Passes the container to `handler` and continues the chain with the returned container. |

```csharp
static IContainer Card(IContainer card) =>
    card.Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten5).Padding(10);

container.Column(column =>
{
    column.Spacing(10);
    column.Item().Element(Card).Text("First card");
    column.Item().Element(Card).Text("Second card");
    column.Item().Element(item => item.Text("Built inline"));
});
```

### DefaultTextStyle

Sets the default text style for all text inside the container. The available style options are described in [Text reference](text.md).

| Method | Description |
| --- | --- |
| `IContainer DefaultTextStyle(TextStyle textStyle)` | Uses the given style. |
| `IContainer DefaultTextStyle(Func<TextStyle, TextStyle> handler)` | Builds a style from `TextStyle.Default`. |

```csharp
container.DefaultTextStyle(style => style.FontSize(14).FontColor(Colors.Grey.Darken3)).Column(column =>
{
    column.Item().Text("Uses the default style");
    column.Item().Text("Overrides the size").FontSize(20);
});
```

### Components

`Component(T component)` and `Component<T>()` insert a reusable piece of layout that implements `IComponent` (a single `Compose(IContainer container)` method). See [Reusable components](../how-to/reusable-components.md).

### Dynamic components

`Dynamic<TState>(IDynamicComponent<TState> dynamicElement)` inserts a component that is composed again for every page, with access to the page number, the total page count and the available size (`DynamicContext`). Use it for content that depends on its position, such as running totals. The related overload `Element(IDynamicElement child)` places an element created with `DynamicContext.CreateElement`. See [Reusable components](../how-to/reusable-components.md) for details and examples.

## Debugging

These elements help to find layout problems; see [Debug layout issues](../how-to/debug-layout-issues.md).

### DebugArea

Draws a 1-point border and a semi-transparent fill over the space the content gets, with an optional label centered at the top, so you can see the space an element actually occupies.

| Method | Description |
| --- | --- |
| `IContainer DebugArea()` | Red, without label. |
| `IContainer DebugArea(string text)` | Red, with label. |
| `IContainer DebugArea(string text, string color)` | Label, border and fill in the given color. |

```csharp
container.Padding(10).DebugArea("Content", Colors.Blue.Medium).Text(Placeholders.Paragraph());
```

### DebugPointer

Adds a named marker that shows up in the element trace of a `DocumentLayoutException`, so you can locate the failing element. It does not change the output.

`IContainer DebugPointer(string elementTraceText)`

```csharp
container.DebugPointer("Invoice items").Column(column =>
{
    column.Item().Text("Item 1");
});
```
