# Text reference

This page lists everything ShinyPDF offers for text: the `Text` entry points, the text descriptor with spans, links, page numbers and inline elements, every text style option, and the rules for how styles are inherited. All members live in the `ShinyPDF.Fluent` and `ShinyPDF.Infrastructure` namespaces. For registering your own fonts and handling missing glyphs, see [Fonts](../how-to/fonts.md).

## Contents

- [Adding text](#adding-text)
- [Spans, lines and paragraphs](#spans-lines-and-paragraphs)
- [Alignment](#alignment)
- [Links](#links)
- [Page numbers](#page-numbers)
- [Inline elements](#inline-elements)
- [Text styles](#text-styles)
  - [Font family, size and color](#font-family-size-and-color)
  - [Font weight](#font-weight)
  - [Italic, underline and strikethrough](#italic-underline-and-strikethrough)
  - [Subscript and superscript](#subscript-and-superscript)
  - [Line height and letter spacing](#line-height-and-letter-spacing)
  - [Word wrapping](#word-wrapping)
  - [Text direction](#text-direction)
  - [Font fallback](#font-fallback)
- [Style inheritance](#style-inheritance)
- [Library defaults](#library-defaults)

## Adding text

`TextExtensions` provides two overloads on `IContainer`:

| Overload | Returns | Use it for |
|---|---|---|
| `Text(string? text)` | `TextSpanDescriptor` | A single run of text with one style. Style methods can be chained directly. |
| `Text(Action<TextDescriptor> content)` | `void` | Rich text: several spans with different styles, links, page numbers, inline elements. |

Both overloads end the container chain: a text element has no child.

```csharp
container.Column(column =>
{
    column.Item().Text("A single styled line").FontSize(16).Bold();

    column.Item().Text(text =>
    {
        text.Span("Rich text with a ");
        text.Span("bold").Bold();
        text.Span(" word.");
    });
});
```

## Spans, lines and paragraphs

Members of `TextDescriptor` that add text. Each returns a `TextSpanDescriptor`, so you can style the added text.

| Member | Description |
|---|---|
| `Span(string? text)` | Appends text to the current paragraph. A `null` value adds nothing. |
| `Line(string? text)` | Appends text followed by a line break, so the next content starts a new paragraph. |
| `EmptyLine()` | Adds an empty paragraph (a blank line in the current style). |
| `ParagraphSpacing(float value, Unit unit = Unit.Point)` | Vertical space between paragraphs. Default is `0`. |
| `DefaultTextStyle(TextStyle style)` / `DefaultTextStyle(Func<TextStyle, TextStyle> style)` | Default style for all spans inside this text element (see [Style inheritance](#style-inheritance)). |

Every line break (`\n`, also inside `Span`) starts a new paragraph; `\r` characters are removed. `ParagraphSpacing` applies between these paragraphs, not between wrapped lines of one paragraph. Lines wrap at spaces; a word longer than the available width is broken only when it is the first item in the line.

```csharp
container.Text(text =>
{
    text.ParagraphSpacing(8);
    text.DefaultTextStyle(x => x.FontSize(11));

    text.Line("First paragraph.");
    text.Line("Second paragraph, separated by 8 points.");
    text.EmptyLine();
    text.Span("Text after a blank line.\nThis starts another paragraph.");
});
```

## Alignment

| Member | Description |
|---|---|
| `AlignLeft()` | Aligns every line to the left. |
| `AlignCenter()` | Centers every line. |
| `AlignRight()` | Aligns every line to the right. |

If you do not set an alignment on the descriptor, ShinyPDF uses the horizontal alignment of an `AlignLeft()`, `AlignCenter()` or `AlignRight()` container directly in front of `Text`. Without either, text is aligned left in left-to-right content and right in right-to-left content (`ContentFromRightToLeft()`). There is no justified alignment.

```csharp
container.Column(column =>
{
    column.Item().Text(text =>
    {
        text.AlignCenter();
        text.Span("Centered with the descriptor");
    });

    // the container alignment is picked up by the text element
    column.Item().AlignRight().Text("Right-aligned with the container");
});
```

## Links

| Member | Description |
|---|---|
| `Hyperlink(string? text, string url)` | Text that opens an external URL. Throws `ArgumentException` if `url` is null or empty. Empty `text` adds nothing. |
| `SectionLink(string? text, string sectionName)` | Text that jumps to a location inside the document marked with `container.Section(sectionName)`. Throws `ArgumentException` if `sectionName` is null or empty. Empty `text` adds nothing. |

Links get no default styling: add color or underline yourself. To make a whole area clickable instead of a text run, use the `Hyperlink(url)` and `SectionLink(sectionName)` container extensions (see [Elements](elements.md)).

```csharp
container.Column(column =>
{
    column.Item().Text(text =>
    {
        text.Span("Visit ");
        text.Hyperlink("the repository", "https://github.com/LM-Development/ShinyPDF")
            .FontColor(Colors.Blue.Medium)
            .Underline();
        text.Span(" or jump to the ");
        text.SectionLink("summary", "summary").Underline();
        text.Span(".");
    });

    column.Item().PageBreak();

    column.Item().Section("summary").Text("Summary").FontSize(20).Bold();
});
```

## Page numbers

These `TextDescriptor` members insert a number that is resolved while the page is drawn. Each returns a `TextPageNumberDescriptor`, which supports every span style method plus `Format`.

| Member | Value |
|---|---|
| `CurrentPageNumber()` | Number of the current page (1-based). |
| `TotalPages()` | Total number of pages in the document. |
| `BeginPageNumberOfSection(string locationName)` | First page of the section marked with `Section(locationName)`. |
| `EndPageNumberOfSection(string locationName)` | Last page of that section. |
| `PageNumberWithinSection(string locationName)` | Current page number counted from the start of that section (1 on its first page). |
| `TotalPagesWithinSection(string locationName)` | Number of pages the section spans. |

`Format(PageNumberFormatter formatter)` controls how the number is printed. The delegate is `string PageNumberFormatter(int? pageNumber)`. The value is `null` when it cannot be resolved, for example when no section with that name exists; the default formatter prints an empty string in that case.

Page numbers are typical in headers and footers; see [Headers, footers and page numbers](../how-to/headers-footers-page-numbers.md) for complete layouts.

```csharp
container.AlignCenter().Text(text =>
{
    text.Span("Page ");
    text.CurrentPageNumber().Bold();
    text.Span(" of ");
    text.TotalPages();

    text.Span(" | ");
    text.CurrentPageNumber().Format(number => number is null ? "-" : $"#{number:D3}");
});
```

## Inline elements

`Element()` returns an `IContainer` placed inside the text flow, like a large glyph. Use it for icons, small images or spacers. The content is bottom-aligned on the line and takes its minimal size, so give it an explicit size. Inline elements inherit the surrounding default text style.

```csharp
container.Text(text =>
{
    text.Span("Status: ");
    text.Element().Width(10).Height(10).Background(Colors.Green.Medium);
    text.Span(" online");
});
```

## Text styles

A `TextStyle` (namespace `ShinyPDF.Infrastructure`) is an immutable record. Start from `TextStyle.Default` (a style with nothing set) and chain methods from `TextStyleExtensions`; each call returns a new style. Reuse a style on a span with `Style(TextStyle style)` or set it as a default with `DefaultTextStyle`.

The same options exist as span methods (`TextSpanDescriptorExtensions`), which work on `TextSpanDescriptor` and `TextPageNumberDescriptor` and return the same descriptor type for chaining.

```csharp
var heading = TextStyle.Default.FontSize(18).SemiBold().FontColor(Colors.Blue.Darken2);

container.Column(column =>
{
    column.Item().Text("Styled with a reusable TextStyle").Style(heading);
    column.Item().Text("Styled with span methods").FontSize(18).SemiBold();
});
```

| Option | `TextStyle` method | Span method |
|---|---|---|
| Font family | `FontFamily(string)` | same |
| Font size (points) | `FontSize(float)` | same |
| Text color | `FontColor(string)` | same |
| Background color | `BackgroundColor(string)` | same |
| Line height | `LineHeight(float)` | same |
| Letter spacing | `LetterSpacing(float)` | same |
| Weight | `Weight(FontWeight)`, `Thin()` ... `ExtraBlack()` | `Thin()` ... `ExtraBlack()` (no `Weight`) |
| Italic | `Italic(bool value = true)` | same |
| Underline | `Underline(bool value = true)` | same |
| Strikethrough | `Strikethrough(bool value = true)` | same |
| Position | `NormalPosition()`, `Subscript()`, `Superscript()` | same |
| Wrapping | `WrapAnywhere(bool value = true)` | same |
| Direction | `DirectionAuto()`, `DirectionFromLeftToRight()`, `DirectionFromRightToLeft()` | same |
| Fallback | `Fallback(TextStyle? value = null)`, `Fallback(Func<TextStyle, TextStyle>)` | same, see the note in [Font fallback](#font-fallback) |
| Apply a whole style | n/a | `Style(TextStyle)` |

### Font family, size and color

- `FontFamily(string value)`: the family name of a built-in, registered or installed font, for example `Fonts.Lato`, `Fonts.NotoSans`, `Fonts.Courier` (constants in `ShinyPDF.Helpers.Fonts`). An unknown family causes an `ArgumentException` during generation. See [Fonts](../how-to/fonts.md).
- `FontSize(float value)`: size in points. Values `<= 0` throw `ArgumentException`.
- `FontColor(string value)` and `BackgroundColor(string value)`: hex colors in the formats `#RGB`, `#ARGB`, `#RRGGBB` or `#AARRGGBB` (the `#` is optional). Invalid values throw `ArgumentException` immediately. Use the `Colors` palette in `ShinyPDF.Helpers` (for example `Colors.Red.Medium`, `Colors.Grey.Lighten3`). The background covers the span's full line height.

```csharp
container.Text(text =>
{
    text.Span("Courier 14, red ").FontFamily(Fonts.Courier).FontSize(14).FontColor(Colors.Red.Medium);
    text.Span("highlighted").BackgroundColor(Colors.Yellow.Lighten3);
    text.Span(" semi-transparent").FontColor("#80000000");
});
```

### Font weight

`FontWeight` enum values and their shortcut methods:

| Method | `FontWeight` | Value |
|---|---|---|
| `Thin()` | `Thin` | 100 |
| `ExtraLight()` | `ExtraLight` | 200 |
| `Light()` | `Light` | 300 |
| `NormalWeight()` | `Normal` | 400 |
| `Medium()` | `Medium` | 500 |
| `SemiBold()` | `SemiBold` | 600 |
| `Bold()` | `Bold` | 700 |
| `ExtraBold()` | `ExtraBold` | 800 |
| `Black()` | `Black` | 900 |
| `ExtraBlack()` | `ExtraBlack` | 1000 |

If the font has no file for the requested weight, the closest available one is used (see [Fonts](../how-to/fonts.md#font-weight-and-italic-matching)). For example, the built-in Lato family has Light, Regular, Bold and Black, so `Medium()` renders as Regular and `SemiBold()` as Bold.

```csharp
var semiBoldStyle = TextStyle.Default.Weight(FontWeight.SemiBold);

container.Text(text =>
{
    text.DefaultTextStyle(x => x.FontFamily(Fonts.NotoSans));
    text.Line("Thin").Thin();
    text.Line("Normal").NormalWeight();
    text.Line("SemiBold via Weight").Style(semiBoldStyle);
    text.Line("Black").Black();
});
```

### Italic, underline and strikethrough

`Italic()`, `Underline()` and `Strikethrough()` accept an optional `bool`, so you can switch an inherited decoration off, for example `Underline(false)`.

Decoration lines use the text color and the position and thickness defined by the font. There are no options for line color, thickness or style. Current behavior with sub- and superscript:

- Superscript: the underline is drawn at the position of normal-sized text, so it lines up with the surrounding text.
- Subscript: the underline follows the lowered, smaller glyphs.
- Strikethrough follows the glyphs; its thickness is scaled to 62.5% for sub- and superscript.

```csharp
container.Text(text =>
{
    text.DefaultTextStyle(x => x.Underline());
    text.Span("Underlined, ");
    text.Span("not underlined, ").Underline(false);
    text.Span("italic, ").Italic();
    text.Span("struck through").Strikethrough();
});
```

### Subscript and superscript

`Subscript()` and `Superscript()` render the span at 62.5% of the font size, shifted down by 10% or up by 35% of the font size. To keep strokes visually as thick as the surrounding text, the font is matched one weight step heavier (+100). `NormalPosition()` resets an inherited position.

```csharp
container.Text(text =>
{
    text.Span("H");
    text.Span("2").Subscript();
    text.Span("O and E = mc");
    text.Span("2").Superscript();
});
```

### Line height and letter spacing

- `LineHeight(float value)`: a multiplier of the font's natural line height (default `1.2`). The height of a wrapped line is the largest height of the items on it, so a big span or inline element makes its whole line taller.
- `LetterSpacing(float value)`: extra space between characters, relative to the font size (em). `0` is the font's normal spacing, positive values add space, negative values reduce it. For example `0.1f` at 20 pt adds 2 pt per character.

```csharp
container.Column(column =>
{
    column.Item().Text(Placeholders.Paragraph()).LineHeight(1.5f);
    column.Item().Text("W I D E").LetterSpacing(0.2f);
    column.Item().Text("Condensed").LetterSpacing(-0.05f);
});
```

### Word wrapping

By default lines break at spaces. `WrapAnywhere()` lets a line break between any two characters, which is useful for long identifiers, URLs or hashes without spaces.

```csharp
container.Width(120).Text("https://example.com/a/very/long/path/without/spaces").WrapAnywhere();
```

### Text direction

Direction methods control how a span is shaped (the order of glyphs):

| Method | Behavior |
|---|---|
| `DirectionAuto()` | Default. The direction is detected from the text content. |
| `DirectionFromLeftToRight()` | Forces left-to-right. |
| `DirectionFromRightToLeft()` | Forces right-to-left. |

Text direction does not change the layout direction or default alignment of the surrounding content; use `ContentFromRightToLeft()` on a container or page for that. To mix directions in one paragraph, put each part in its own span. More on right-to-left documents in [Fonts](../how-to/fonts.md#right-to-left-text).

```csharp
container.Column(column =>
{
    var text = "الجوريتم - algorithm in Arabic";

    column.Item().Text(text).FontFamily("Noto Sans Arabic");
    column.Item().Text(text).FontFamily("Noto Sans Arabic").DirectionFromLeftToRight();
    column.Item().Text(text).FontFamily("Noto Sans Arabic").DirectionFromRightToLeft();
});
```

The example assumes the Noto Sans Arabic font is registered, see [Fonts](../how-to/fonts.md#registering-fonts).

### Font fallback

`Fallback` sets another style that is used for characters the primary font does not contain, for example CJK characters or emoji in a Latin font. Fallbacks can be nested. How fallback styles combine with the main style and what happens with missing glyphs is described in [Fonts](../how-to/fonts.md#font-fallback).

```csharp
container
    .DefaultTextStyle(x => x
        .FontFamily(Fonts.Lato)
        .Fallback(y => y.FontFamily("Noto Sans JP")))
    .Text("Latin text with 日本語 inside");
```

> **Note:** Set fallbacks on a `TextStyle` (with `DefaultTextStyle` or `Style(...)`), as shown above. The span method `Fallback(...)` on `TextSpanDescriptor` currently writes into a shared style instance instead of creating a new one, which can leak the fallback into `TextStyle.Default` and other spans. Use `.Style(TextStyle.Default.Fallback(y => y.FontFamily("...")))` on a single span instead.

## Style inheritance

Text styles are resolved per span, from the most specific to the least specific source. A property that is set wins; only unset properties are taken from the next level:

1. Styles set on the span itself (span methods, `Style(...)`).
2. `DefaultTextStyle` of the `Text` descriptor.
3. `DefaultTextStyle(...)` on containers wrapping the text, innermost first.
4. `page.DefaultTextStyle(...)` of the page (applies to header, content and footer, not to the page `Background()` and `Foreground()` layers).
5. `TextStyle.LibraryDefault` (see [Library defaults](#library-defaults)).

`Style(TextStyle)` on a span overrides every property that the given style sets, including the font family; properties the style leaves unset keep their earlier value. `TextStyle.Default` is an empty style, so `x => x.FontSize(14)` in a `DefaultTextStyle` call changes only the size and inherits everything else.

```csharp
Document.Create(document =>
{
    document.Page(page =>
    {
        page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

        page.Content()
            .DefaultTextStyle(x => x.FontFamily(Fonts.NotoSans))
            .Text(text =>
            {
                text.DefaultTextStyle(x => x.LineHeight(1.4f));

                // Noto Sans, 10 pt, dark grey, line height 1.4, bold
                text.Span("Inherited style plus bold").Bold();
            });
    });
}).GeneratePdf("inheritance.pdf");
```

Fallback styles are resolved against the main style: the fallback keeps its own font family, but every property set on the main style (after steps 1 to 4) overrides the fallback's value. A fallback property only takes effect when nothing in the main chain sets it. Library defaults never override fallback properties. For example, if the page default sets `Fallback(y => y.FontFamily("Noto Sans JP").Underline())` and a span sets `Underline(false)`, the Japanese characters in that span are not underlined either.

## Library defaults

Values used when nothing else sets a property (`TextStyle.LibraryDefault`, internal):

| Property | Default |
|---|---|
| Font family | `Lato` (embedded in the library) |
| Font size | 12 pt |
| Line height | 1.2 |
| Letter spacing | 0 |
| Color | Black (`#000000`) |
| Background color | Transparent |
| Weight | Normal (400) |
| Position | Normal |
| Italic, underline, strikethrough, wrap anywhere | off |
| Direction | Auto |
| Fallback | `Noto Color Emoji` (embedded), black |

Characters that neither your style nor its fallbacks cover are finally tried with Lato and Noto Color Emoji, which is why emoji work without configuration. See [Fonts](../how-to/fonts.md#built-in-fonts) for the full list of embedded fonts.
