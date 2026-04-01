[![GitHub Repo stars](https://img.shields.io/github/stars/LM-Development/ShinyPDF?style=badge)](https://github.com/LM-Development/ShinyPDF/stargazers)



### ShinyPDF is a modern open-source .NET library for PDF document generation. Offering comprehensive layout engine powered by concise and discoverable C# Fluent API. Easily generate PDF documents, reports, invoices, exports etc.

![Usage animation of the PDF library](https://raw.githubusercontent.com/QuestPDF/QuestPDF-Documentation/main/docs/public/previewer/animation.gif)

👨‍💻 Design PDF documents using C# and employ a code-only approach. Utilize your version control system to its fullest potential.

🧱 Compose PDF document with a range of powerful and predictable structural elements, such as text, image, border, table, and many more.

⚙️ Utilize a comprehensive layout engine, specifically designed for PDF document generation and paging support.

📖 Write code using concise and easy-to-understand C# Fluent API. Utilize IntelliSense to quickly discover available options.

🔗 Don't be limited to any proprietary scripting language or format. Follow your experience and leverage all modern C# features.

⌛ Save time thanks to a hot-reload capability, allowing real-time PDF document preview without code recompilation.


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

![Minimal PDF example](https://raw.githubusercontent.com/QuestPDF/QuestPDF-Documentation/main/docs/public/minimal-example-shadow.png)


## Let's get started

Begin exploring the ShinyPDF library today. You are 250 lines of C# code away from creating a fully functional PDF invoice implementation.

Read the Getting Started tutorial to familiarize yourself with general library architecture, important layout structures as well as to better understand helpful patterns and practices.

Easily start designing your PDF documents, reports, invoices, exports and even more.

![Example invoice](https://raw.githubusercontent.com/QuestPDF/QuestPDF-Documentation/main/docs/public/invoice-small.png)
