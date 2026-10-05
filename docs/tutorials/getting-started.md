# Getting started

In this tutorial you build a small but realistic one-page invoice with ShinyPDF: a header with a title and a date, a table of line items, a total, and a footer with page numbers. You start from an empty console application and add one piece at a time, so you see what each part of the Fluent API does. You need the .NET 10 SDK and about fifteen minutes.

## Step 1: Create a project and install ShinyPDF

Create a new console application and add the [ShinyPDF NuGet package](https://www.nuget.org/packages/ShinyPDF):

```bash
dotnet new console -n InvoiceDemo
cd InvoiceDemo
dotnet add package ShinyPDF
```

If you develop or run on Linux (including Docker and WSL), also add the native assets for SkiaSharp and HarfBuzzSharp. Without them the application fails at the first render with `DllNotFoundException: Unable to load shared library 'libSkiaSharp'`:

```bash
dotnet add package SkiaSharp.NativeAssets.Linux
dotnet add package HarfBuzzSharp.NativeAssets.Linux
```

See [Deploy on Linux and Docker](../how-to/deploy-on-linux.md) for the details. On Windows and macOS the native libraries come with ShinyPDF's dependencies.

## Step 2: Generate your first PDF

Replace the contents of `Program.cs` with the following code and run it with `dotnet run`:

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

        page.Content().Text("Hello from ShinyPDF!");
    });
})
.GeneratePdf("invoice.pdf");
```

The program writes `invoice.pdf` to the current working directory (the project folder when you use `dotnet run`). Open it in any PDF viewer.

What the code does:

- `Document.Create` describes a document. The lambda receives a document container, and you add pages to it.
- `container.Page(...)` defines a page set. The settings apply to every page the content needs: if your content is longer than one page, ShinyPDF adds pages automatically.
- `page.Size`, `page.Margin`, `page.PageColor` and `page.DefaultTextStyle` configure the page. Sizes and margins use points by default (1 point is 1/72 inch); pass a `Unit` such as `Unit.Centimetre` or `Unit.Millimetre` to use other units.
- `page.Content()` returns the main content slot. Every slot accepts exactly one child element, here a piece of text.
- `GeneratePdf("invoice.pdf")` lays out and renders the document. [Generate PDF, XPS and images](../how-to/generate-output.md) shows the other output options.

The three `using` lines are the namespaces you need in almost every ShinyPDF file: `ShinyPDF.Fluent` (the API), `ShinyPDF.Helpers` (`Colors`, `PageSizes`, `Placeholders`, `Fonts`) and `ShinyPDF.Infrastructure` (`IContainer`, `Unit`, `TextStyle`).

## Step 3: Split the page into header, content and footer

A page has three main slots: `Header()`, `Content()` and `Footer()`. The header and footer repeat on every page; the content flows across pages. To keep the code readable, give each slot its own local function. Change the page definition like this:

```csharp
Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(2, Unit.Centimetre);
        page.PageColor(Colors.White);
        page.DefaultTextStyle(x => x.FontSize(11));

        page.Header().Element(ComposeHeader);
        page.Content().Element(ComposeContent);
        page.Footer().Element(ComposeFooter);
    });
})
.GeneratePdf("invoice.pdf");

void ComposeHeader(IContainer container)
{
    container.Text("Header");
}

void ComposeContent(IContainer container)
{
    container.Text("Content");
}

void ComposeFooter(IContainer container)
{
    container.Text("Footer");
}
```

`Element(...)` hands the slot's container to your function. Inside the function you build the slot's content on `container` exactly as you would inline. You fill in the three functions in the next steps.

## Step 4: Build the header

The header shows the invoice title and number on the left and the date on the right. A `Row` places items side by side:

```csharp
void ComposeHeader(IContainer container)
{
    container.Row(row =>
    {
        row.RelativeItem().Column(column =>
        {
            column.Item().Text("Invoice #2026-0042").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
            column.Item().Text("Shiny Consulting GmbH");
        });

        row.ConstantItem(150).AlignRight().Text(text =>
        {
            text.Span("Date: ").SemiBold();
            text.Span(DateTime.Today.ToString("yyyy-MM-dd"));
        });
    });
}
```

- `row.RelativeItem()` takes the remaining width; `row.ConstantItem(150)` is always 150 points wide. There is also `row.AutoItem()`, which is as wide as its content.
- `Column` stacks items vertically. Each `column.Item()` is a container for one child.
- `Text("...")` returns a span descriptor, so you can chain styles such as `FontSize`, `SemiBold` and `FontColor`. With `Text(text => ...)` you combine several differently styled spans in one paragraph.
- `AlignRight()` aligns the text inside the constant item.

## Step 5: Add the line items table

Now add the invoice data and a table. Put the data at the top of `Program.cs` (after the `using` lines) and replace `ComposeContent`:

```csharp
var items = new[]
{
    (Name: "Workshop: document design", Quantity: 1, UnitPrice: 1200m),
    (Name: "Template development (hours)", Quantity: 14, UnitPrice: 95m),
    (Name: "Font licensing advice", Quantity: 2, UnitPrice: 60m),
};

void ComposeContent(IContainer container)
{
    container.PaddingVertical(1, Unit.Centimetre).Table(table =>
    {
        table.ColumnsDefinition(columns =>
        {
            columns.ConstantColumn(25);
            columns.RelativeColumn(3);
            columns.RelativeColumn();
            columns.RelativeColumn();
            columns.RelativeColumn();
        });

        table.Header(header =>
        {
            header.Cell().Element(HeaderCellStyle).Text("#");
            header.Cell().Element(HeaderCellStyle).Text("Description");
            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Quantity");
            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Unit price");
            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Amount");
        });

        var position = 1;

        foreach (var item in items)
        {
            table.Cell().Element(CellStyle).Text(position++.ToString());
            table.Cell().Element(CellStyle).Text(item.Name);
            table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
            table.Cell().Element(CellStyle).AlignRight().Text($"{item.UnitPrice:N2} EUR");
            table.Cell().Element(CellStyle).AlignRight().Text($"{item.Quantity * item.UnitPrice:N2} EUR");
        }
    });
}

IContainer HeaderCellStyle(IContainer container)
{
    return container
        .DefaultTextStyle(x => x.SemiBold())
        .BorderBottom(1)
        .BorderColor(Colors.Black)
        .PaddingVertical(5);
}

IContainer CellStyle(IContainer container)
{
    return container
        .BorderBottom(1)
        .BorderColor(Colors.Grey.Lighten2)
        .PaddingVertical(5);
}
```

- `ColumnsDefinition` defines the columns: `ConstantColumn(25)` is 25 points wide, and the `RelativeColumn`s share the remaining width in the ratio 3:1:1:1.
- `table.Header(...)` defines header cells. If the table ever spans several pages, the header row repeats on each page.
- `table.Cell()` adds the next cell. Cells fill the table left to right, row by row, so five cells make one row.
- `HeaderCellStyle` and `CellStyle` are plain functions that take a container, add styling (border, padding, default text style) and return the inner container. `Element(CellStyle)` applies such a function in the middle of a chain. This is the simplest way to reuse styling; [Build reusable components](../how-to/reusable-components.md) shows more options.
- `PaddingVertical(1, Unit.Centimetre)` adds space between the header, the table and the footer.

## Step 6: Show the total

Below the table, show the sum of all items. Wrap the table in a `Column` so you can add another item after it. Replace `ComposeContent` again:

```csharp
// not compiled: excerpt that uses `items` and the cell style functions from step 5
void ComposeContent(IContainer container)
{
    var total = items.Sum(x => x.Quantity * x.UnitPrice);

    container.PaddingVertical(1, Unit.Centimetre).Column(column =>
    {
        column.Spacing(10);

        column.Item().Element(ComposeTable);

        column.Item().AlignRight().Text(text =>
        {
            text.Span("Total: ").SemiBold();
            text.Span($"{total:N2} EUR").SemiBold().FontSize(14);
        });
    });
}
```

Move the `container.Table(...)` call from step 5 into a new function `void ComposeTable(IContainer container)` (without the `PaddingVertical`, which now sits on the column). `column.Spacing(10)` puts 10 points between the table and the total. The complete program at the end of this tutorial shows the result.

## Step 7: Add page numbers to the footer

Finally, fill the footer with a centered page number. `CurrentPageNumber()` and `TotalPages()` are special spans that ShinyPDF fills in while it renders each page:

```csharp
void ComposeFooter(IContainer container)
{
    container.AlignCenter().Text(text =>
    {
        text.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken1));

        text.Span("Page ");
        text.CurrentPageNumber();
        text.Span(" of ");
        text.TotalPages();
    });
}
```

`text.DefaultTextStyle(...)` sets the style for all spans in this text block. The invoice fits on one page, so the footer reads "Page 1 of 1", but the same code works for documents of any length. [Headers, footers and page numbers](../how-to/headers-footers-page-numbers.md) covers formatting page numbers and showing a different header on the first page.

## The complete program

Here is the complete `Program.cs`:

```csharp
using ShinyPDF.Fluent;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;

var items = new[]
{
    (Name: "Workshop: document design", Quantity: 1, UnitPrice: 1200m),
    (Name: "Template development (hours)", Quantity: 14, UnitPrice: 95m),
    (Name: "Font licensing advice", Quantity: 2, UnitPrice: 60m),
};

Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(2, Unit.Centimetre);
        page.PageColor(Colors.White);
        page.DefaultTextStyle(x => x.FontSize(11));

        page.Header().Element(ComposeHeader);
        page.Content().Element(ComposeContent);
        page.Footer().Element(ComposeFooter);
    });
})
.GeneratePdf("invoice.pdf");

void ComposeHeader(IContainer container)
{
    container.Row(row =>
    {
        row.RelativeItem().Column(column =>
        {
            column.Item().Text("Invoice #2026-0042").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
            column.Item().Text("Shiny Consulting GmbH");
        });

        row.ConstantItem(150).AlignRight().Text(text =>
        {
            text.Span("Date: ").SemiBold();
            text.Span(DateTime.Today.ToString("yyyy-MM-dd"));
        });
    });
}

void ComposeContent(IContainer container)
{
    var total = items.Sum(x => x.Quantity * x.UnitPrice);

    container.PaddingVertical(1, Unit.Centimetre).Column(column =>
    {
        column.Spacing(10);

        column.Item().Element(ComposeTable);

        column.Item().AlignRight().Text(text =>
        {
            text.Span("Total: ").SemiBold();
            text.Span($"{total:N2} EUR").SemiBold().FontSize(14);
        });
    });
}

void ComposeTable(IContainer container)
{
    container.Table(table =>
    {
        table.ColumnsDefinition(columns =>
        {
            columns.ConstantColumn(25);
            columns.RelativeColumn(3);
            columns.RelativeColumn();
            columns.RelativeColumn();
            columns.RelativeColumn();
        });

        table.Header(header =>
        {
            header.Cell().Element(HeaderCellStyle).Text("#");
            header.Cell().Element(HeaderCellStyle).Text("Description");
            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Quantity");
            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Unit price");
            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Amount");
        });

        var position = 1;

        foreach (var item in items)
        {
            table.Cell().Element(CellStyle).Text(position++.ToString());
            table.Cell().Element(CellStyle).Text(item.Name);
            table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
            table.Cell().Element(CellStyle).AlignRight().Text($"{item.UnitPrice:N2} EUR");
            table.Cell().Element(CellStyle).AlignRight().Text($"{item.Quantity * item.UnitPrice:N2} EUR");
        }
    });
}

IContainer HeaderCellStyle(IContainer container)
{
    return container
        .DefaultTextStyle(x => x.SemiBold())
        .BorderBottom(1)
        .BorderColor(Colors.Black)
        .PaddingVertical(5);
}

IContainer CellStyle(IContainer container)
{
    return container
        .BorderBottom(1)
        .BorderColor(Colors.Grey.Lighten2)
        .PaddingVertical(5);
}

void ComposeFooter(IContainer container)
{
    container.AlignCenter().Text(text =>
    {
        text.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken1));

        text.Span("Page ");
        text.CurrentPageNumber();
        text.Span(" of ");
        text.TotalPages();
    });
}
```

Run `dotnet run` and open `invoice.pdf`. You see the blue invoice title with the date on the right, the table with a bold header row and thin grey row separators, the total below the table, and "Page 1 of 1" at the bottom of the page.

While you design a layout, `Placeholders` gives you sample content, for example `Placeholders.Sentence()`, `Placeholders.Paragraph()` or `Placeholders.Image(200, 100)`. Placeholder text is random, so do not use it in tests that compare output.

## Next steps

- [Generate PDF, XPS and images](../how-to/generate-output.md): write to a stream (for example in a web API), render pages as PNG images, set document metadata, and move your document into an `IDocument` class.
- [Headers, footers and page numbers](../how-to/headers-footers-page-numbers.md): page sizes, orientation, margins and numbering.
- [Use custom fonts](../how-to/fonts.md): register your own fonts.
- [Build reusable components](../how-to/reusable-components.md): turn parts of a document into components.
- [Debug layout issues](../how-to/debug-layout-issues.md): what to do when content does not fit.
- [Deploy on Linux and Docker](../how-to/deploy-on-linux.md): run ShinyPDF in containers.
- [Elements reference](../reference/elements.md) and [Text reference](../reference/text.md): all available elements and text options.
- [Layout engine](../explanation/layout-engine.md): how ShinyPDF measures, pages and draws content.
