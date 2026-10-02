# Debug layout issues

This guide shows how to find out why a document does not look the way you expect or why generation fails: how to visualize element boundaries with `DebugArea`, how to get and read the element trace of a `DocumentLayoutException`, how to mark parts of your layout with `DebugPointer`, and how to fix the most common causes. It assumes you know the basics of the Fluent API; the background on measuring, wrapping and paging is in [How the layout engine works](../explanation/layout-engine.md).

## Know which exception you have

ShinyPDF throws four exception types, all in the `ShinyPDF.Drawing.Exceptions` namespace. They tell you in which phase the problem occurred:

| Exception | When | Typical cause |
|---|---|---|
| `DocumentComposeException` | While the element tree is built, before any layout | Invalid structure: two children in one container, a `Layers` element without exactly one primary layer, a table without columns or with impossible cell positions, image data that cannot be decoded |
| `DocumentLayoutException` | During layout | Content that can never fit on a page, or a document longer than `Settings.DocumentLayoutExceptionThreshold` pages |
| `DocumentDrawingException` | During drawing | An exception thrown while a page was drawn (the original one is in `InnerException`), or a character that no configured font contains |
| `InitializationException` | When the PDF, XPS or image backend is created | Missing native SkiaSharp or HarfBuzzSharp libraries, see [Deploy on Linux](deploy-on-linux.md) |

## See element boundaries with DebugArea

Many layout problems are not exceptions: an element is smaller, larger or placed differently than you thought. `DebugArea` draws a colored border and a translucent background around its child, with an optional label, so you can see the exact space an element occupies:

```csharp
container
    .Padding(20)
    .DebugArea("Customer address", Colors.Blue.Medium)
    .Column(column =>
    {
        column.Item().Text("Jane Doe");
        column.Item().Text("Main Street 1");
    });
```

The overloads are `DebugArea()`, `DebugArea(string text)` (red) and `DebugArea(string text, string color)`. `DebugArea` returns the container for the child, so you can insert it anywhere in a chain and remove it again later without restructuring your code. Nest several areas with different colors to compare a parent and its child.

## Get the element trace of a DocumentLayoutException

A `DocumentLayoutException` always has the same message ("Composed layout generates infinite document ..."), because the engine only knows that the page could not be completed. The useful information is in the `ElementTrace` property, which is only filled when `Settings.EnableDebugging` is `true`.

`EnableDebugging` defaults to `true` only when a debugger is attached. In tests, CI or production it is off, and `ElementTrace` only contains "Debug trace is available only in the DEBUG mode." Turn it on explicitly while you investigate:

```csharp
using ShinyPDF;
using ShinyPDF.Drawing.Exceptions;

Settings.EnableDebugging = true;

var document = Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(2, Unit.Centimetre);

        page.Content()
            .Width(100)
            .DebugPointer("Customer card")
            .Width(150)
            .Text("Example");
    });
});

try
{
    document.GeneratePdf("report.pdf");
}
catch (DocumentLayoutException exception)
{
    Console.WriteLine(exception.Message);
    Console.WriteLine(exception.ElementTrace);
    throw;
}
```

Debugging adds a tracking proxy around every element and makes generation slower, so do not leave it enabled in production code.

## Reading a DocumentLayoutException

The trace is the tree of elements that were measured on the page where layout failed, starting at the document root. Shortened, the trace for the example above looks like this:

```text
🔥 Column
---------
Available space: (Width: 14,400.000, Height: 14,400.000)
Requested space: Wrap

    ...

                    🔥 Constrained
                    --------------
                    Available space: (Width: 14,400.000, Height: 14,400.000)
                    Requested space: Wrap
                    Content Direction: LeftToRight
                    Min Width: 595.4
                    Max Width: 595.4
                    Min Height: 842
                    Max Height: 842

                        🔥 Padding
                        ----------
                        Available space: (Width: 595.400, Height: 842.000)
                        Requested space: Wrap
                        Top: 56.692913
                        Right: 56.692913
                        Bottom: 56.692913
                        Left: 56.692913

                            ...

                                🔥 Decoration
                                -------------
                                Available space: (Width: 482.014, Height: 728.614)
                                Requested space: Wrap
                                Content Direction: LeftToRight

                                    Page header 🌟
                                    --------------
                                    Available space: (Width: 482.014, Height: 728.614)
                                    Requested space: FullRender (Width: 0.000, Height: 0.000)

                                    ...

                                            🔥 Page content 🌟
                                            ------------------
                                            Available space: (Width: 482.014, Height: 728.614)
                                            Requested space: Wrap

                                                🔥 Constrained
                                                --------------
                                                Available space: (Width: 482.014, Height: 728.614)
                                                Requested space: Wrap
                                                Content Direction: LeftToRight
                                                Min Width: 100
                                                Max Width: 100
                                                Min Height: -
                                                Max Height: -

                                                    🔥 Customer card 🌟
                                                    -------------------
                                                    Available space: (Width: 100.000, Height: 728.614)
                                                    Requested space: Wrap

                                                        🔥 Constrained
                                                        --------------
                                                        Available space: (Width: 100.000, Height: 728.614)
                                                        Requested space: Wrap
                                                        Content Direction: LeftToRight
                                                        Min Width: 150
                                                        Max Width: 150
                                                        Min Height: -
                                                        Max Height: -
```

How to read it:

- **Each block is one Measure call** on one element, indented under its parent. The title is the internal element type (`Column`, `Padding`, `Constrained`, `Decoration`, `TextBlock`, ...); a block can appear more than once when a parent measures a child several times.
- **Available space** is what the parent offered, **Requested space** is what the element answered: `FullRender` or `PartialRender` with a size, or `Wrap`.
- **The flame marker** (🔥) marks every element that did not answer `FullRender`. Follow the chain of flames from the root downwards. The culprit is usually the deepest flame whose own children fit, or whose configuration cannot be met by its available space. Here, the innermost `Constrained` needs a width of 150 but only gets 100.
- **The star marker** (🌟) marks a `DebugPointer`, shown with its name instead of a type name. The page itself adds pointers named "Page header", "Page content", "Page footer", "Page background layer" and "Page foreground layer", so you can always tell which page area failed.
- **Configuration lines** below the sizes are the element's settings, for example `Min Width` and `Max Width` for size constraints (`Width`, `MinWidth`, `MaxWidth`, `Height`, ...), the four sides of a `Padding`, or `Extend Vertical` for the page content area.
- Numbers are formatted with the current culture, so the separators may look different on your machine.
- The trace only covers the last page that was attempted. If the exception is caused by the page limit, the trace shows the measurement of that last page.

Some internal elements appear in every trace and can usually be skipped: the root `Column` (measured with unlimited space, shown as 14,400 points), `DebuggingProxy`, `ContentDirectionSetter`, `Background`, `Layers`, `DefaultTextStyle`, `Decoration` and the two `Extend` elements of the page content. The outermost `Constrained` and `Padding` are the page size and the page margins: in the example, an A4 page of 595.4 x 842 points minus 2 cm margins leaves 482 x 728.6 points for the page content.

## Mark your own elements with DebugPointer

In a large document, a trace full of `Column`, `Padding` and `TextBlock` entries is hard to map back to your code. `DebugPointer(string)` inserts an invisible marker that appears in the trace under the name you give it, highlighted with the star marker. It does not change the layout or the output:

```csharp
container.Column(column =>
{
    column.Item()
        .DebugPointer("Invoice header")
        .Text("Invoice #1234");

    column.Item()
        .DebugPointer("Line items table")
        .Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.ConstantColumn(80);
            });

            table.Cell().Text("Item");
            table.Cell().AlignRight().Text("Price");
        });
});
```

When a debugger is attached, each `IComponent` used with `container.Component(...)` is also added to the trace under its class name (without the star marker), so well-named components make traces easier to read. See [Build reusable components](reusable-components.md).

## Common causes and how to fix them

### A fixed size larger than the available space

`Width`, `Height`, `MinWidth` and `MinHeight` are hard requirements. If the parent offers less, the element reports `Wrap`, and if even a new page does not offer enough, layout fails. The trace shows a `Constrained` element whose `Min Width` or `Min Height` is larger than its available space.

Remember that margins, padding, borders, the header and the footer all reduce the space. A `Height(800)` does not fit on an A4 page (842 points tall) with 2 cm margins.

Fixes: use `MaxHeight` or `MaxWidth` when you mean "at most", let the content decide its size, or scale the content down:

```csharp
container
    .MaxHeight(300)
    .ScaleToFit()
    .Text(Placeholders.Paragraphs());
```

### Images that are too tall

An image keeps its aspect ratio. With the default `ImageScaling.FitWidth`, its height follows from the available width, and a tall image on a wide page can be higher than the whole content area. Limit the height and fit the image into the resulting area:

```csharp
container
    .Height(250)
    .Image(Placeholders.Image(400, 1200), ImageScaling.FitArea);
```

### ShowEntire on content that is longer than a page

`ShowEntire()` prevents splitting: if its content does not fit completely, it moves to the next page. If the content is longer than a whole page, it cannot fit anywhere and layout fails. Use `EnsureSpace` instead, which only moves the content when little space is left and otherwise lets it split:

```csharp
container.Column(column =>
{
    foreach (var index in Enumerable.Range(1, 5))
    {
        column.Item()
            .EnsureSpace(120)
            .Column(section =>
            {
                section.Item().Text($"Section {index}").Bold();
                section.Item().Text(Placeholders.Paragraphs());
            });
    }
});
```

Keep `ShowEntire()` for small blocks that must stay together, such as a signature block or a heading with its first paragraph.

### Elements that cannot be split

Images, canvases, lines, `ScaleToFit` and the content of a dynamic component are always drawn in one piece. The same applies to the page header and footer and to table headers and footers, which must fit completely on every page. If they get too large, every page fails:

- Keep headers and footers small. Content that belongs only to the first page goes into the page content, or into the header with `ShowOnce()`.
- A long table header (for example, several rows of column titles) reduces the space for rows on every page.
- A dynamic component must return content that fits into `context.AvailableSize`; otherwise it throws `DocumentLayoutException` with "Dynamic component generated content that does not fit on a single page."

### Rows wider than the page

The width of a `Row` is the sum of its `ConstantItem` widths, its `AutoItem` widths and the spacing. If that is wider than the available width, the row reports `Wrap`. Prefer `RelativeItem` for at least one column so the row can adapt:

```csharp
container.Row(row =>
{
    row.Spacing(10);
    row.ConstantItem(120).Text("Label");
    row.RelativeItem().Text(Placeholders.Sentence());
});
```

### Very long documents

If the trace shows nothing unusual, the document may simply be longer than `Settings.DocumentLayoutExceptionThreshold` (250 pages by default). The limit has to be higher than your longest document, because reaching it already fails:

```csharp
using ShinyPDF;

Settings.DocumentLayoutExceptionThreshold = 5000;
```

A dynamic component that always returns `HasMoreContent = true` also produces pages until this limit is reached.

### Missing glyphs and the font fallback exception

When `Settings.CheckIfAllTextGlyphsAreAvailable` is `true` (the default when a debugger is attached), text containing a character that neither the font nor any fallback font provides throws a `DocumentDrawingException`:

```text
Could not find an appropriate font fallback for glyph: U-4E16 '世'. Font families available on current environment that contain this glyph: ...
```

The message lists the registered font families that contain the character. Fix it by using one of them as the main font or by adding it as a fallback:

```csharp
container
    .Text("Hello 世界")
    .FontFamily(Fonts.Lato)
    .Fallback(x => x.FontFamily("Noto Sans JP"));
```

The font has to be registered first; see [Use custom fonts](fonts.md). When the check is off, missing characters are silently rendered as placeholder glyphs, so it is worth enabling it in your tests:

```csharp
using ShinyPDF;

Settings.CheckIfAllTextGlyphsAreAvailable = true;
```

### DocumentComposeException: multiple children in one container

Most containers hold exactly one child. Calling two content methods on the same container throws a `DocumentComposeException` ("You should not assign multiple child elements to a single-child container"):

```csharp
// not compiled: this throws DocumentComposeException
container.Text("First");
container.Text("Second");
```

Use a `Column` (or `Row`, `Layers`, `Table`) when you need several children:

```csharp
container.Column(column =>
{
    column.Item().Text("First");
    column.Item().Text("Second");
});
```

The same error appears when a container captured in a lambda is used after its scope, or when one container is filled inside a loop.

## Debugging settings at a glance

All settings are static properties of `ShinyPDF.Settings` and apply to every document generated afterwards:

| Setting | Default | Effect |
|---|---|---|
| `EnableDebugging` | `true` only with a debugger attached | Fills `DocumentLayoutException.ElementTrace`; slower |
| `EnableCaching` | `false` with a debugger attached, otherwise `true` | Caches measurements; faster, does not change the output |
| `CheckIfAllTextGlyphsAreAvailable` | `true` only with a debugger attached | Throws on characters that no configured font contains |
| `DocumentLayoutExceptionThreshold` | `250` | Maximum page count before a `DocumentLayoutException` |

Because three of these defaults depend on whether a debugger is attached, a document can fail in a test run but show a placeholder glyph in production (or the other way round). Set the values explicitly where consistent behavior matters.
