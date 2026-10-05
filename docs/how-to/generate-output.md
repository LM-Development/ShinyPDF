# Generate PDF, XPS and images

This guide shows how to turn a ShinyPDF document into output: a PDF file, a byte array or a stream, an XPS document, or one PNG image per page. It also covers document metadata, the global `Settings` that influence generation, and how to move a document into its own class by implementing `IDocument`.

All generation methods are extension methods on `IDocument` in the `ShinyPDF.Fluent` namespace. `Document.Create(...)` returns a `Document`, which implements `IDocument`, so you can call them directly on it.

## Generate a PDF file

Pass a file path to `GeneratePdf`. An existing file is overwritten.

```csharp
using ShinyPDF.Fluent;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;

var document = Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(2, Unit.Centimetre);
        page.Content().Text("Hello PDF!");
    });
});

document.GeneratePdf("hello.pdf");
```

A `Document` only stores the description of your content. Every call to a `Generate*` method runs the layout and rendering again, so you can generate the same document several times, for example as PDF and as images.

## Generate a PDF in memory or into a stream

Without arguments, `GeneratePdf` returns the PDF as a `byte[]`:

```csharp
var document = Document.Create(container =>
{
    container.Page(page => page.Content().Text("Hello PDF!"));
});

byte[] pdf = document.GeneratePdf();
```

To write into a stream you own, pass it to `GeneratePdf(Stream)`:

```csharp
var document = Document.Create(container =>
{
    container.Page(page => page.Content().Text("Hello PDF!"));
});

using var stream = new MemoryStream();
document.GeneratePdf(stream);
stream.Position = 0;
```

The stream must support writing and seeking (`CanWrite` and `CanSeek`); otherwise `GeneratePdf` throws an `ArgumentException`. `MemoryStream` and `FileStream` work. Network streams such as an ASP.NET Core response body do not support seeking, so generate into a `byte[]` or a `MemoryStream` first and then write that to the response. ShinyPDF does not dispose or rewind the stream; set `Position = 0` before you read it back.

For example, in an ASP.NET Core minimal API you can return the byte array as a file result (this snippet needs the ASP.NET Core shared framework, so it is not compiled here):

```csharp
// not compiled: requires an ASP.NET Core project
app.MapGet("/invoice.pdf", () =>
{
    var pdf = Document.Create(container =>
    {
        container.Page(page => page.Content().Text("Hello PDF!"));
    })
    .GeneratePdf();

    return Results.File(pdf, "application/pdf", "invoice.pdf");
});
```

## Generate XPS

XPS output uses the same pattern with `GenerateXps`: a file path, a `Stream` (writable and seekable), or no argument to get a `byte[]`.

```csharp
var document = Document.Create(container =>
{
    container.Page(page => page.Content().Text("Hello XPS!"));
});

document.GenerateXps("hello.xps");
```

> **Note:** ShinyPDF creates XPS documents through SkiaSharp's XPS backend, which is only available on Windows. On Linux, `GenerateXps` currently fails with a `NullReferenceException` instead of a descriptive error. Generate XPS on Windows; on Linux and macOS use PDF.

## Generate images

`GenerateImages` renders every page as a PNG image. Without arguments it returns one `byte[]` per page:

```csharp
var document = Document.Create(container =>
{
    container.Page(page => page.Content().Text("Hello images!"));
});

IEnumerable<byte[]> pages = document.GenerateImages();
```

To write the images to files, pass a function that maps the page index to a file name. The index starts at 0 for the first page. Existing files are overwritten.

```csharp
var document = Document.Create(container =>
{
    container.Page(page => page.Content().Text("Hello images!"));
});

document.GenerateImages(index => $"page-{index + 1}.png");
```

This writes `page-1.png`, `page-2.png` and so on. The image resolution comes from `DocumentMetadata.RasterDpi` (see the next section). The default of 72 DPI produces one pixel per point, so an A4 page becomes an image of about 595 by 842 pixels. For sharper images, raise `RasterDpi`, for example to 150 or 300.

## Set document metadata

`DocumentMetadata` (namespace `ShinyPDF.Drawing`) holds the PDF metadata and a few rendering options. Attach it with `WithMetadata`:

```csharp
using ShinyPDF.Drawing;

var metadata = new DocumentMetadata
{
    Title = "Invoice 2026-0042",
    Author = "Shiny Consulting GmbH",
    Subject = "Invoice",
    Keywords = "invoice, consulting",
    Creator = "InvoiceDemo",
    Producer = "ShinyPDF",
    CreationDate = DateTime.Now,
    ModifiedDate = DateTime.Now,
    RasterDpi = 150,
};

Document
    .Create(container =>
    {
        container.Page(page => page.Content().Text("Hello PDF!"));
    })
    .WithMetadata(metadata)
    .GeneratePdf("invoice.pdf");
```

The available properties:

| Property | Default | Meaning |
| --- | --- | --- |
| `Title`, `Author`, `Subject`, `Keywords`, `Creator`, `Producer` | `null` | Text fields written to the PDF document information. |
| `CreationDate`, `ModifiedDate` | `DateTime.Now` when the metadata object is created | Dates written to the PDF. |
| `RasterDpi` | `72` | Resolution for content that has to be rasterized in PDF and XPS output, and the resolution of images from `GenerateImages`. |
| `ImageQuality` | `101` | Encoding quality for images in the PDF, passed to SkiaSharp. 101 means lossless. With a value from 0 to 100, SkiaSharp may encode opaque images as JPEG with that quality, which makes the file smaller. |
| `PdfA` | `false` | Asks SkiaSharp to add the metadata that PDF/A requires (such as XMP metadata and an output intent). |

> **Note:** `PdfA = true` only adds metadata. It does not check that your content (for example fonts or transparency) conforms to PDF/A. Validate the output with a PDF/A validator if you need certified conformance.

`DocumentMetadata.Default` returns a new instance with the default values. Passing `null` to `WithMetadata` keeps the current metadata.

## Implement IDocument as a class

For anything beyond a small script, put your document into its own class. Implement `IDocument` with two methods: `GetMetadata()` returns the metadata, and `Compose(IDocumentContainer)` adds the pages. The data the document needs comes in through the constructor.

```csharp
using ShinyPDF.Drawing;
using ShinyPDF.Fluent;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;

public record InvoiceLine(string Description, decimal Amount);

public class InvoiceDocument : IDocument
{
    private readonly string _number;
    private readonly IReadOnlyList<InvoiceLine> _lines;

    public InvoiceDocument(string number, IReadOnlyList<InvoiceLine> lines)
    {
        _number = number;
        _lines = lines;
    }

    public DocumentMetadata GetMetadata() => new DocumentMetadata
    {
        Title = $"Invoice {_number}",
        Author = "Shiny Consulting GmbH",
    };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(11));

            page.Header().Text($"Invoice {_number}").FontSize(20).SemiBold();
            page.Content().PaddingVertical(10).Element(ComposeLines);
            page.Footer().AlignCenter().Text(text =>
            {
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
        });
    }

    private void ComposeLines(IContainer container)
    {
        container.Column(column =>
        {
            column.Spacing(5);

            foreach (var line in _lines)
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text(line.Description);
                    row.AutoItem().Text($"{line.Amount:N2} EUR");
                });
            }
        });
    }
}
```

Generate it like any other document:

```csharp
// not compiled: uses the InvoiceDocument class from the previous block
var lines = new List<InvoiceLine>
{
    new("Workshop", 1200m),
    new("Template development", 1330m),
};

var document = new InvoiceDocument("2026-0042", lines);
document.GeneratePdf("invoice.pdf");
```

`Document.Create` is a convenience implementation of the same interface: it stores your lambda and calls it from `Compose`. You can also split a class-based document into smaller parts with `IComponent`; see [Build reusable components](reusable-components.md).

## Global settings

The static class `ShinyPDF.Settings` contains process-wide switches. Set them once at application startup, before you generate documents. They apply to every document generated afterwards.

```csharp
using ShinyPDF;

Settings.DocumentLayoutExceptionThreshold = 1000;
Settings.EnableCaching = true;
Settings.EnableDebugging = false;
Settings.CheckIfAllTextGlyphsAreAvailable = true;
```

| Setting | Default | What it does |
| --- | --- | --- |
| `DocumentLayoutExceptionThreshold` | `250` | Maximum number of pages ShinyPDF produces. When the document reaches this many pages, generation stops with a `DocumentLayoutException`. This protects you from layouts that would never finish, for example an element that never fits on a page. Raise it if your documents are legitimately longer. |
| `EnableCaching` | `true` when no debugger is attached | Caches layout measurements. This usually speeds up generation noticeably at the cost of slightly more memory. |
| `EnableDebugging` | `true` when a debugger is attached | Records extra layout information, so that a `DocumentLayoutException` carries an element trace (`ElementTrace`) that points to the problematic area. Costs performance. |
| `CheckIfAllTextGlyphsAreAvailable` | `true` when a debugger is attached | Checks that the selected font contains every character in your text. If not, generation fails with a `DocumentDrawingException` instead of silently drawing placeholder characters. Costs a little performance. |

Because the debugging-related defaults depend on whether a debugger is attached, a document can behave differently under the debugger than in production: for example, a missing glyph throws while debugging but renders as a placeholder character in production. Set the flags explicitly if you want the same behavior everywhere. [Debug layout issues](debug-layout-issues.md) explains how to read layout exceptions and element traces, and [Use custom fonts](fonts.md) shows how to provide fonts for missing glyphs.

## Errors during generation

The exceptions ShinyPDF throws during generation live in the `ShinyPDF.Drawing.Exceptions` namespace:

- `DocumentComposeException`: the document description is invalid, for example a table without column definitions.
- `DocumentLayoutException`: the content cannot be laid out, or it exceeds `DocumentLayoutExceptionThreshold` pages. With `EnableDebugging`, its `ElementTrace` property describes where the problem is.
- `DocumentDrawingException`: an error occurred while drawing a page, for example a missing glyph when `CheckIfAllTextGlyphsAreAvailable` is enabled. The original exception is the `InnerException`.
- `InitializationException`: SkiaSharp could not create the PDF or XPS document, usually because native libraries are missing. See [Deploy on Linux and Docker](deploy-on-linux.md).
