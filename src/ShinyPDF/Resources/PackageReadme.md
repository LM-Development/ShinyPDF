# <img src="https://raw.githubusercontent.com/LM-Development/ShinyPDF/main/docs/img/logo.svg" height="25" /> ShinyPDF

ShinyPDF is a modern open-source .NET library for PDF document generation. Offering comprehensive layout engine powered by concise and discoverable C# Fluent API. Shiny PDF is based on the latest fully open source version of [QuestPDF](https://github.com/QuestPDF/QuestPDF).

👨‍💻 Build production-ready PDFs in pure C# with a code-first workflow that fits naturally into your existing development process.

🧱 Compose rich layouts with predictable building blocks: text, images, borders, tables, layers, headers, footers, and more.

⚙️ Rely on a layout engine purpose-built for document generation, with robust paging, measurement, and rendering behavior.

📖 Stay productive with a concise Fluent API and full IntelliSense discoverability across the entire document DSL.

🔗 No proprietary template language. Use modern .NET features, reusable abstractions, and the tools you already trust.


## Simplicity is the key

How easy it is to start and prototype with ShinyPDF? Really easy thanks to its minimal API! Please analyse the code below that generates basic PDF document:

```csharp
using ShinyPDF.Fluent;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;

// code in your main method
Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(2, Unit.Centimetre);
        page.Background(Colors.White);
        page.DefaultTextStyle(x => x.FontSize(20));
        
        page.Header()
            .Text("Hello PDF!")
            .SemiBold().FontSize(36).FontColor(Colors.Blue.Medium);
        
        page.Content()
            .PaddingVertical(1, Unit.Centimetre)
            .Column(x =>
            {
                x.Spacing(20);
                
                x.Item().Text(Placeholders.LoremIpsum());
                x.Item().Image(Placeholders.Image(200, 100));
            });
        
        page.Footer()
            .AlignCenter()
            .Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
            });
    });
})
.GeneratePdf("hello.pdf");
```


## Let's get started

Begin exploring the ShinyPDF library today.
