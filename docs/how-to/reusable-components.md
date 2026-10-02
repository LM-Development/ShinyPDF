# Build reusable components

As documents grow, the same building blocks appear again and again: an address block, a styled table cell, a card with a border, a footer with page numbers. This guide shows the ways ShinyPDF lets you reuse layout code: extension methods and style functions for small pieces, `IComponent` classes for self-contained parts with their own data, `IDocument` classes for whole documents, and dynamic components (`IDynamicComponent<TState>`) for content that depends on the page number or on the space left on the page. All examples use only the public API.

## Choose the right tool

| You want to reuse | Use |
|---|---|
| A style or a chain of decorations (padding, border, background) | A function `IContainer -> IContainer` or an extension method on `IContainer` |
| A self-contained block with its own data (address, invoice header) | A class implementing `IComponent` |
| A whole document, for example one per report type | A class implementing `IDocument` |
| Content that changes per page or depends on the remaining space | A class implementing `IDynamicComponent<TState>` |

The first three only run while the element tree is built, once per document generation. Dynamic components run during layout, see [How the layout engine works](../explanation/layout-engine.md).

## Style functions with Element

The lightest form of reuse is a plain function that takes a container, adds some elements, and returns the innermost container. Apply it with `Element(...)`, which accepts a `Func<IContainer, IContainer>`:

```csharp
container.Table(table =>
{
    table.ColumnsDefinition(columns =>
    {
        columns.RelativeColumn();
        columns.ConstantColumn(80);
    });

    table.Cell().Element(HeaderCell).Text("Item");
    table.Cell().Element(HeaderCell).AlignRight().Text("Price");

    table.Cell().Element(BodyCell).Text("Coffee");
    table.Cell().Element(BodyCell).AlignRight().Text("3.50");

    static IContainer HeaderCell(IContainer cell) => cell
        .DefaultTextStyle(x => x.SemiBold())
        .BorderBottom(1)
        .BorderColor(Colors.Grey.Darken2)
        .PaddingVertical(5);

    static IContainer BodyCell(IContainer cell) => cell
        .BorderBottom(1)
        .BorderColor(Colors.Grey.Lighten2)
        .PaddingVertical(5);
});
```

`Element` also has an overload taking an `Action<IContainer>`, for functions that fill the container completely instead of returning it.

## Extension methods on IContainer

When the same style is used across files, turn it into an extension method. It then reads like a built-in Fluent API method:

```csharp
public static class LayoutExtensions
{
    // Returns the inner container, so the caller decides what goes inside.
    public static IContainer Card(this IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten1)
            .Background(Colors.Grey.Lighten4)
            .Padding(10);
    }

    // Fills the container completely, so it returns nothing.
    public static void SectionTitle(this IContainer container, string text)
    {
        container
            .PaddingBottom(5)
            .Text(text)
            .FontSize(16)
            .SemiBold();
    }
}

public static class LayoutExtensionsUsage
{
    public static void Compose(IContainer container)
    {
        container.Column(column =>
        {
            column.Spacing(10);
            column.Item().SectionTitle("Summary");
            column.Item().Card().Text(Placeholders.Paragraph());
        });
    }
}
```

The convention from the built-in API applies here as well: return `IContainer` when the method only wraps content, return `void` when it produces the final content.

## Components with IComponent

`IComponent` has a single method, `void Compose(IContainer container)`. A component is an ordinary class, so it can take its data through the constructor or properties, hold helper methods, and be unit-tested in isolation. Insert it with `container.Component(...)`:

```csharp
public record Address(string Name, string Street, string City, string Email);

public class AddressComponent : IComponent
{
    private string Title { get; }
    private Address Address { get; }

    public AddressComponent(string title, Address address)
    {
        Title = title;
        Address = address;
    }

    public void Compose(IContainer container)
    {
        container.Column(column =>
        {
            column.Spacing(2);

            column.Item()
                .BorderBottom(1)
                .PaddingBottom(5)
                .Text(Title)
                .SemiBold();

            column.Item().Text(Address.Name);
            column.Item().Text(Address.Street);
            column.Item().Text(Address.City);
            column.Item().Text(Address.Email).FontColor(Colors.Blue.Medium);
        });
    }
}

public static class AddressComponentUsage
{
    public static void Compose(IContainer container, Address seller, Address customer)
    {
        container.Row(row =>
        {
            row.RelativeItem().Component(new AddressComponent("From", seller));
            row.ConstantItem(50);
            row.RelativeItem().Component(new AddressComponent("For", customer));
        });
    }
}
```

There are two public overloads:

- `container.Component(T component)` takes an instance; use it when the component needs data.
- `container.Component<T>()` creates the instance itself and requires a public parameterless constructor (`where T : IComponent, new()`).

`Compose` is called once, immediately, when `Component(...)` runs, with a fresh container. Any loops and conditions inside it are evaluated at that moment, not per page. Like any container, the one passed to `Compose` accepts exactly one child, so use a `Column`, `Row` or similar when the component has several parts.

When a debugger is attached, `Component(...)` also records the component's class name in the element trace of a `DocumentLayoutException`, which makes layout errors easier to locate. See [Debug layout issues](debug-layout-issues.md#mark-your-own-elements-with-debugpointer).

### Passing content into a component

To let the caller provide part of a component's content (for example the body of a card with a fixed frame and title), accept an `Action<IContainer>` and invoke it with the container where that content belongs:

```csharp
public class TitledBox : IComponent
{
    private string Title { get; }
    private Action<IContainer> Content { get; }

    public TitledBox(string title, Action<IContainer> content)
    {
        Title = title;
        Content = content;
    }

    public void Compose(IContainer container)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Medium)
            .Column(column =>
            {
                column.Item()
                    .Background(Colors.Grey.Lighten3)
                    .Padding(5)
                    .Text(Title)
                    .SemiBold();

                column.Item().Padding(5).Element(Content);
            });
    }
}

public static class TitledBoxUsage
{
    public static void Compose(IContainer container)
    {
        container.Component(new TitledBox("Notes", content =>
        {
            content.Text(Placeholders.Paragraph());
        }));
    }
}
```

> **Note:** The ShinyPDF source contains an internal "slot" mechanism for components, inherited from QuestPDF. It is not part of the public API, so use delegates as shown above to pass content into a component.

## Whole documents with IDocument

`Document.Create(...)` is convenient for small documents. For documents with their own data model and metadata, implement `IDocument` instead. The generation methods (`GeneratePdf`, `GenerateXps`, `GenerateImages`) are extension methods on `IDocument`, so they work the same way:

```csharp
using ShinyPDF.Drawing;

public record InvoiceModel(string Number, IReadOnlyList<string> Lines);

public class InvoiceDocument : IDocument
{
    private InvoiceModel Model { get; }

    public InvoiceDocument(InvoiceModel model)
    {
        Model = model;
    }

    public DocumentMetadata GetMetadata()
    {
        var metadata = DocumentMetadata.Default;
        metadata.Title = $"Invoice {Model.Number}";
        return metadata;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);

            page.Header().Text($"Invoice {Model.Number}").FontSize(24).SemiBold();

            page.Content().PaddingVertical(10).Column(column =>
            {
                foreach (var line in Model.Lines)
                    column.Item().Text(line);
            });

            page.Footer().AlignCenter().Text(text =>
            {
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
        });
    }
}

public static class InvoiceDocumentUsage
{
    public static void Generate()
    {
        var model = new InvoiceModel("2026-001", new[] { "Consulting", "Support" });
        new InvoiceDocument(model).GeneratePdf("invoice.pdf");
    }
}
```

Inside `Compose`, split the page into components (`page.Header().Component(new InvoiceHeader(Model))`) to keep each part small. More on generation options is in [Generate PDF, XPS and images](generate-output.md).

## Dynamic components

A regular component is composed once, before layout starts, so it cannot know on which page it ends up or how much space is left. A dynamic component is asked for its content **during layout, on every page it appears on**. Use it for:

- content that depends on the page number, for example a footer aligned left on even and right on odd pages,
- content that depends on the available space, for example a list that shows as many entries as fit and adds a per-page subtotal.

For page numbers in text, the built-in `CurrentPageNumber()` and `TotalPages()` text elements are simpler; see [Headers, footers and page numbers](headers-footers-page-numbers.md). Reach for a dynamic component when the layout itself, not just a number, changes.

### The contract

A dynamic component implements `IDynamicComponent<TState>`:

- `TState State { get; set; }` holds the component's progress between pages. `TState` must be a struct (`where TState : struct`).
- `DynamicComponentComposeResult Compose(DynamicContext context)` returns the content for the current page in `Content` and sets `HasMoreContent` to `true` if the component needs another page.

`IDynamicComponent<TState>` and `DynamicComponentComposeResult` are in `ShinyPDF.Infrastructure`; `DynamicContext` and `IDynamicElement` are in `ShinyPDF.Elements`, so add `using ShinyPDF.Elements;`.

`DynamicContext` provides:

| Member | Description |
|---|---|
| `PageNumber` | The number of the current page. |
| `TotalPages` | The total number of pages. See the note below. |
| `AvailableSize` | The space offered to the component on this page. |
| `CreateElement(Action<IContainer>)` | Builds content with the Fluent API and returns an `IDynamicElement`, whose `Size` is its measured size. |

Rules that follow from how the engine calls it:

- **Keep all progress in `State`.** `Compose` is called several times per page: when the engine measures the page, its state changes are discarded and `State` is restored; only the call made while drawing keeps the new state. Fields other than `State` must not change in `Compose`.
- **The content must fit.** The returned content has to fit completely into `AvailableSize`. Otherwise the component throws a `DocumentLayoutException` ("Dynamic component generated content that does not fit on a single page."). Use `IDynamicElement.Size` to check before returning.
- **`CreateElement` measures with unlimited space.** Text inside it does not wrap at the page width unless you constrain the width, for example with `.Width(context.AvailableSize.Width)`; otherwise the reported `Size` is not what the element needs on the page.
- **State is reset per generation pass.** The engine lays out the document twice (see [The rendering pipeline](../explanation/layout-engine.md#the-rendering-pipeline)) and restores the state the component had when it was added before each pass. Set the initial state in the constructor.
- `HasMoreContent = true` forever produces pages until `Settings.DocumentLayoutExceptionThreshold` is reached.

> **Note:** During the first layout pass, `TotalPages` is the number of pages laid out so far, not the final count. The final value is available in the second pass, which produces the output. Avoid making the size of the content depend on `TotalPages`, or the two passes may lay out pages differently.

Content created with `CreateElement` inherits the text style and content direction from the place where the component is used.

### Example: content that depends on the page number

This footer alternates its alignment between pages. It keeps no progress, so it uses `int` as a dummy state:

```csharp
using ShinyPDF.Elements;

public class AlternatingFooter : IDynamicComponent<int>
{
    public int State { get; set; }

    public DynamicComponentComposeResult Compose(DynamicContext context)
    {
        var content = context.CreateElement(container =>
        {
            container
                .Element(x => context.PageNumber % 2 == 0 ? x.AlignLeft() : x.AlignRight())
                .Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                });
        });

        return new DynamicComponentComposeResult
        {
            Content = content,
            HasMoreContent = false
        };
    }
}

public static class AlternatingFooterUsage
{
    public static void Generate()
    {
        Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(1, Unit.Centimetre);

                page.Content().Column(column =>
                {
                    foreach (var _ in Enumerable.Range(0, 20))
                        column.Item().PaddingBottom(10).Text(Placeholders.Paragraph());
                });

                page.Footer().Dynamic(new AlternatingFooter());
            });
        })
        .GeneratePdf("alternating-footer.pdf");
    }
}
```

`Dynamic(...)` is an extension method on `IContainer`, so a dynamic component can be placed in the header, footer, content or any nested container.

### Example: content that depends on the available space

This list shows as many entries as fit on each page and adds a line with the number of entries shown on that page. It stores the number of entries already shown in its state and reports `HasMoreContent` until all entries are done:

```csharp
using ShinyPDF.Elements;

public struct EntryListState
{
    public int ShownCount { get; set; }
}

public class PagedEntryList : IDynamicComponent<EntryListState>
{
    private IReadOnlyList<string> Entries { get; }
    public EntryListState State { get; set; }

    public PagedEntryList(IReadOnlyList<string> entries)
    {
        Entries = entries;
        State = new EntryListState { ShownCount = 0 };
    }

    public DynamicComponentComposeResult Compose(DynamicContext context)
    {
        var start = State.ShownCount;
        var fittingCount = 0;
        IDynamicElement content = context.CreateElement(_ => { });

        // Grow the page content one entry at a time until it no longer fits.
        for (var count = 1; start + count <= Entries.Count; count++)
        {
            var candidate = Build(context, start, count);

            if (candidate.Size.Height > context.AvailableSize.Height)
                break;

            content = candidate;
            fittingCount = count;
        }

        State = new EntryListState { ShownCount = start + fittingCount };

        return new DynamicComponentComposeResult
        {
            Content = content,
            HasMoreContent = State.ShownCount < Entries.Count
        };
    }

    private IDynamicElement Build(DynamicContext context, int start, int count)
    {
        return context.CreateElement(container =>
        {
            container
                .Width(context.AvailableSize.Width)
                .Column(column =>
                {
                    foreach (var entry in Entries.Skip(start).Take(count))
                        column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(entry);

                    column.Item().AlignRight().PaddingTop(4).Text($"{count} entries on this page").Italic();
                });
        });
    }
}

public static class PagedEntryListUsage
{
    public static void Generate()
    {
        var entries = Enumerable.Range(1, 80).Select(i => $"{i}. {Placeholders.Sentence()}").ToList();

        Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(1, Unit.Centimetre);
                page.Content().Dynamic(new PagedEntryList(entries));
            });
        })
        .GeneratePdf("entries.pdf");
    }
}
```

Points worth noting in this example:

- The width constraint in `Build` makes text wrap at the real page width, so `candidate.Size.Height` is accurate.
- If not even one entry fits, the component returns empty content and `HasMoreContent = true`, so the entries continue on the next page. An entry that is taller than a whole page would repeat this forever; in real code, guard against that case.
- Building the content once per candidate count is simple but grows quadratically with the number of entries per page. For long lists, measure each entry once with `CreateElement` and add up the heights, then combine the measured elements with `.Element(...)`; `IContainer.Element(IDynamicElement)` accepts elements created by `CreateElement`.

If you only need a table whose header and footer repeat unchanged on every page, a regular `Table` with `table.Header(...)` and `table.Footer(...)` does that without a dynamic component. A dynamic component is needed when the repeated content differs per page, like the per-page count above.
