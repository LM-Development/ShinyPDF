# Use custom fonts

This guide shows how to register your own font files with ShinyPDF, use them in text styles, configure font fallback for scripts and symbols your main font does not cover (CJK, Arabic, emoji), and lay out right-to-left text. It also lists the fonts that ship inside the library. For the full list of text style options, see the [Text reference](../reference/text.md).

## Built-in fonts

The ShinyPDF assembly embeds these fonts and registers them automatically on first use, so they work on every platform without installation (all under the SIL Open Font License):

| Family name | Constant | Styles |
|---|---|---|
| `Lato` | `Fonts.Lato` | Light, Regular, Bold, Black, each with italic. The library default font. |
| `Lato Hairline` | n/a | Lato Thin and Thin Italic register under this separate family name. |
| `Courier` | `Fonts.Courier` | Regular, Bold, Italic. |
| `Courier BoldItalic` | n/a | Courier Bold Italic registers under this separate family name. |
| `Noto Sans` | `Fonts.NotoSans` | Thin to Black (100 to 900), each with italic. |
| `Noto Sans SemiCondensed`, `Noto Sans Condensed`, `Noto Sans ExtraCondensed` | n/a | Narrower widths of Noto Sans, Thin to Black with italic. |
| `Noto Color Emoji` | n/a | Color emoji. The library default fallback font. |

The constants live in `ShinyPDF.Helpers.Fonts`. Fonts installed on the operating system can also be used by their family name, but they are not available on every machine (see [Deploy on Linux](deploy-on-linux.md)). Prefer registering the font files you need, as shown below, so output does not depend on the environment.

```csharp
container.Column(column =>
{
    column.Item().Text("Lato (default)");
    column.Item().Text("Noto Sans").FontFamily(Fonts.NotoSans);
    column.Item().Text("Noto Sans Condensed").FontFamily("Noto Sans Condensed");
    column.Item().Text("Courier").FontFamily(Fonts.Courier);
});
```

## Registering fonts

`FontManager` (namespace `ShinyPDF.Drawing`) is a static class. Register fonts once at application startup, before the first document is generated. Registration is global for the process.

| Method | Description |
|---|---|
| `RegisterFont(Stream stream)` | Registers the font under the family name stored in the font file. |
| `RegisterFontWithCustomName(string fontName, Stream stream)` | Registers the font under its own family name and additionally under `fontName`. |
| `RegisterFontFromEmbeddedResource(string pathName)` | Loads an embedded resource from the calling assembly and registers it like `RegisterFont`. Throws `ArgumentException` if the resource does not exist. |
| `GetRegisteredFontFamilies()` | Returns the family names of installed system fonts plus all registered fonts. |

TrueType (`.ttf`) and OpenType (`.otf`) files work, and font collections (`.ttc`) register every face they contain. The font data is copied during registration, so you can dispose the stream afterwards. Register every style file of a family (regular, bold, italic and so on) with the same family name; ShinyPDF picks the right file per span (see [Font weight and italic matching](#font-weight-and-italic-matching)).

### From a file

There is no overload that takes a path: open a stream.

```csharp
using ShinyPDF.Drawing;

using (var regular = File.OpenRead("fonts/NotoSansJP-Regular.ttf"))
    FontManager.RegisterFont(regular);

using (var arabic = File.OpenRead("fonts/NotoSansArabic-Regular.ttf"))
    FontManager.RegisterFont(arabic);
```

Make sure the font files are copied to the output directory, for example with `<None Update="fonts\*.ttf" CopyToOutputDirectory="PreserveNewest" />` in your project file.

### From an embedded resource

Mark the files as embedded resources in your project file:

```xml
<ItemGroup>
  <EmbeddedResource Include="Fonts\*.ttf" />
</ItemGroup>
```

The resource name is the assembly's default namespace followed by the folder path and file name, separated by dots (for a project `MyApp` with `Fonts\Barlow-Regular.ttf`, it is `MyApp.Fonts.Barlow-Regular.ttf`). The method searches the assembly that calls it, so call it from the assembly that contains the resource, not from a helper in another assembly.

```csharp
using ShinyPDF.Drawing;

FontManager.RegisterFontFromEmbeddedResource("MyApp.Fonts.Barlow-Regular.ttf");
FontManager.RegisterFontFromEmbeddedResource("MyApp.Fonts.Barlow-Bold.ttf");
```

### From any stream with a custom name

Use `RegisterFontWithCustomName` when the family name inside the file is inconvenient, or to load a font from a database or HTTP response. The font stays available under its original family name as well.

```csharp
using ShinyPDF.Drawing;

var fontBytes = File.ReadAllBytes("fonts/LibreBarcode39-Regular.ttf");

using (var stream = new MemoryStream(fontBytes))
    FontManager.RegisterFontWithCustomName("Barcode", stream);
```

## Using a registered font

Refer to the font by its family name with `FontFamily`, either on a span or in a default text style. To find the exact family name of a file, register it and inspect `FontManager.GetRegisteredFontFamilies()`.

```csharp
using ShinyPDF.Drawing;

FontManager.RegisterFont(File.OpenRead("fonts/LibreBarcode39-Regular.ttf"));

Document.Create(document =>
{
    document.Page(page =>
    {
        page.DefaultTextStyle(x => x.FontFamily(Fonts.NotoSans).FontSize(11));

        page.Content().Column(column =>
        {
            column.Item().Text("Body text in Noto Sans");
            column.Item().Text("*SHINYPDF*").FontFamily("Libre Barcode 39").FontSize(48);
        });
    });
}).GeneratePdf("fonts.pdf");
```

If a family name is neither registered nor installed, document generation throws an `ArgumentException` ("The typeface '...' could not be found") that lists all available family names.

## Font weight and italic matching

Each span requests a weight (`Thin()` to `ExtraBlack()`, see the [Text reference](../reference/text.md#font-weight)) and a slant (`Italic()`). ShinyPDF picks the best file of the family with the CSS font matching rules:

1. Slant first: italic falls back to oblique, then upright; upright falls back to oblique, then italic.
2. Weight second: for a requested 400 or 500, weights in 400 to 500 are preferred. Below that, the nearest lighter weight is preferred; above, the nearest heavier one. If none exists in that direction, the closest weight in the other direction is used.

There is no synthetic bold or italic: if a family has only a regular file, bold text renders in regular. Register a file for every weight and style you use. Two consequences for the built-in fonts:

- Lato has no Medium or SemiBold file: `Medium()` renders as Regular and `SemiBold()` as Bold. `Thin()` and `ExtraLight()` render as Light, unless you use the `Lato Hairline` family.
- `Courier` has no bold italic file in its family, so `Bold().Italic()` renders as regular italic (slant wins over weight). Use the `Courier BoldItalic` family instead.

Subscript and superscript spans request one weight step heavier (+100) to keep strokes as thick as the surrounding text.

## Font fallback

A single font rarely covers every script. With `Fallback` you define a chain of styles; for each character ShinyPDF uses the first style in the chain whose font contains the glyph:

1. The span's font.
2. Its `Fallback` style, then that style's own `Fallback`, and so on.
3. The library default style: Lato, then Noto Color Emoji.

Because of step 3, emoji work out of the box. For other scripts, register a suitable font and add it as a fallback, usually in the page's default text style:

```csharp
using ShinyPDF.Drawing;

FontManager.RegisterFont(File.OpenRead("fonts/NotoSansJP-Regular.ttf"));
FontManager.RegisterFont(File.OpenRead("fonts/NotoSansArabic-Regular.ttf"));

Document.Create(document =>
{
    document.Page(page =>
    {
        page.DefaultTextStyle(x => x
            .FontFamily(Fonts.Lato)
            .Fallback(y => y
                .FontFamily("Noto Sans JP")
                .Fallback(z => z.FontFamily("Noto Sans Arabic"))));

        page.Content().Text(text =>
        {
            text.Line("Latin text in Lato.");
            text.Line("Mixed line: This 中文 is 文本 mixed.");
            text.Line("Arabic: خوارزمية ترتيب");
            text.Line("Emoji need no setup: 😊👍");
        });
    });
}).GeneratePdf("fallback.pdf");
```

A fallback style can set other properties too, but properties set on the main style take precedence: the fallback keeps its own font family, and its other properties only apply when nothing in the main style chain sets them. For example, if the fallback sets `Underline()` and a span sets `Underline(false)`, the fallback characters in that span are not underlined. Details are in [Style inheritance](../reference/text.md#style-inheritance).

> **Note:** Configure fallbacks on a `TextStyle` (in `DefaultTextStyle` or with `Style(...)`), not with the span method `Fallback(...)`, which currently modifies a shared style instance. See the note in the [Text reference](../reference/text.md#font-fallback).

### Detecting missing glyphs

`Settings.CheckIfAllTextGlyphsAreAvailable` (namespace `ShinyPDF`) controls what happens when no font in the chain contains a character:

- `true`: generation throws a `DocumentDrawingException` (namespace `ShinyPDF.Drawing.Exceptions`). The message names the character and its code point, for example `U-4E2D '中'`, and lists the installed or registered font families that contain it.
- `false`: the first font of the chain is used and the character renders as that font's placeholder glyph (often an empty box), without any warning.

The default is `true` only when a debugger is attached, so you see the problem during development and production runs are not interrupted. Set it explicitly if you want the same behavior everywhere, for example in tests:

```csharp
ShinyPDF.Settings.CheckIfAllTextGlyphsAreAvailable = true;
```

Page numbers (`CurrentPageNumber()` and the other page number members) are not checked and do not use fallback.

## Right-to-left text

Arabic, Hebrew and other right-to-left scripts need two things: a font that contains the script, and the right direction.

- **Font:** register a font such as Noto Sans Arabic and use it as the family or as a fallback. None of the built-in fonts contain Arabic or Hebrew glyphs.
- **Shaping direction:** by default (`DirectionAuto()`) the direction of each span is detected from its content, and HarfBuzz shapes the glyphs (ligatures, contextual forms). Force it with `DirectionFromRightToLeft()` or `DirectionFromLeftToRight()`.
- **Layout direction:** `ContentFromRightToLeft()` on a page or container makes text align right by default and mirrors layouts such as rows. The text direction alone does not change alignment.

To mix directions in one paragraph, put each part in its own span; each span is then shaped with its own direction.

```csharp
using ShinyPDF.Drawing;

FontManager.RegisterFont(File.OpenRead("fonts/NotoSansArabic-Regular.ttf"));

Document.Create(document =>
{
    document.Page(page =>
    {
        page.ContentFromRightToLeft();
        page.DefaultTextStyle(x => x
            .FontSize(14)
            .FontFamily(Fonts.Lato)
            .Fallback(y => y.FontFamily("Noto Sans Arabic")));

        page.Content().Column(column =>
        {
            column.Item().Text("في المعلوماتية أو الرياضيات، خوارزمية الترتيب هي خوارزمية تمكن من تنظيم مجموعة عناصر حسب ترتيب محدد.");

            column.Item().Text(text =>
            {
                text.Span("الجوريتم");
                text.Span(" - ");
                text.Span("algorithm in Arabic").DirectionFromLeftToRight();
            });
        });
    });
}).GeneratePdf("rtl.pdf");
```

## See also

- [Text reference](../reference/text.md): every text and style option.
- [Deploy on Linux](deploy-on-linux.md): native dependencies and system fonts in containers.
- [Debug layout issues](debug-layout-issues.md)
