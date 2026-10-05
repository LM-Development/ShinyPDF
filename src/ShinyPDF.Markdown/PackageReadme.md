# ![ShinyPDF logo](https://raw.githubusercontent.com/LM-Development/ShinyPDF/main/docs/img/logo-48.png) ShinyPDF.Markdown

Markdown rendering for [ShinyPDF](https://www.nuget.org/packages/ShinyPDF). Turn Markdown text into PDF content with a single fluent call. The content is laid out by the regular ShinyPDF engine, so it wraps, pages and combines with every other element.

## Installation

```bash
dotnet add package ShinyPDF.Markdown
```

## Usage

```csharp
using ShinyPDF.Fluent;

Document.Create(container =>
{
    container.Page(page =>
    {
        page.Margin(40);

        page.Content().Markdown("""
            # Release notes

            Version **2.1** adds *Markdown* support.

            - Headings, lists and quotes
            - [Links](https://github.com/LM-Development/ShinyPDF) and `inline code`
            """);
    });
})
.GeneratePdf("release-notes.pdf");
```

Styling can be adjusted through options:

```csharp
page.Content().Markdown(markdown, options =>
{
    options.BaseFontSize = 11;
    options.LinkColor = Colors.Green.Darken2;
});
```

## Supported Markdown

Headings, paragraphs, bold, italic, strikethrough, inline code, code blocks, links, bullet and numbered lists (nested), blockquotes and horizontal rules. Raw HTML is shown as plain text, and images are replaced by their alternative text. Parsing is done by [Markdig](https://github.com/xoofx/markdig).
