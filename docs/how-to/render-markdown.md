# Render Markdown

The `ShinyPDF.Markdown` package adds a `Markdown()` element that turns Markdown text into regular ShinyPDF content. Use it for content you already store as Markdown: release notes, knowledge-base articles, user input, reports built from templates.

## Install

```bash
dotnet add package ShinyPDF.Markdown
```

The extension method lives in the `ShinyPDF.Fluent` namespace, so no extra `using` is needed.

## Render Markdown into any container

```csharp
page.Content().Markdown("""
    # Release notes

    Version **2.1** adds *Markdown* support. See [the repository](https://github.com/LM-Development/ShinyPDF).

    1. A `Markdown()` element for any container
    2. Headings, lists, quotes and code blocks
    """);
```

![Rendered Markdown example](../img/example-markdown.png)

`Markdown()` works like any other element: put it inside a `Column`, add padding, or combine it with headers and footers. Long content breaks across pages.

## Adjust the styling

Pass an options callback to change sizes, spacing and colors:

```csharp
container.Markdown(markdown, options =>
{
    options.BaseFontSize = 11;          // body text; headings scale from it
    options.BlockSpacing = 6;           // space between paragraphs, headings, lists, ...
    options.ListItemSpacing = 2;
    options.ListIndent = 18;            // width reserved for bullets and numbers
    options.LinkColor = Colors.Green.Darken2;
    options.CodeFontFamily = Fonts.Courier;
    options.CodeBackgroundColor = Colors.Grey.Lighten4;
    options.BlockquoteBorderColor = Colors.Grey.Lighten1;
    options.BlockquoteTextColor = Colors.Grey.Darken2;
    options.HorizontalRuleColor = Colors.Grey.Lighten1;
});
```

Other text properties (font family, color, line height) are inherited from the surrounding `DefaultTextStyle`, so you can set them on the page:

```csharp
page.DefaultTextStyle(x => x.FontFamily(Fonts.NotoSans).LineHeight(1.4f));
```

By default headings are bold and scaled from `BaseFontSize` (level 1 is 2x, level 6 is 0.9x). To style them yourself, set `HeadingStyle`, which receives the heading level:

```csharp
options.HeadingStyle = level => TextStyle.Default
    .FontSize(level == 1 ? 24 : 16)
    .SemiBold()
    .FontColor(Colors.Blue.Darken3);
```

## Supported Markdown

| Markdown | Result |
|---|---|
| `#` to `######` headings | Bold text scaled from `BaseFontSize` |
| Paragraphs, hard line breaks | Text blocks |
| `**bold**`, `*italic*`, `~~strikethrough~~` | Matching text styles, nesting combines them |
| `` `inline code` `` | Code font with background |
| Fenced and indented code blocks | Code font on a background box, whitespace kept |
| `[text](url)`, `<https://...>`, `<mail@example.com>` | Clickable hyperlinks |
| `-`, `*`, `+` and `1.` lists | Bullets or numbers, nested lists indented |
| `>` blockquotes | Left border with muted text |
| `---` | Horizontal line |

Not supported yet:

- Images: replaced by their alternative text.
- Tables and task lists: shown as plain text.
- Raw HTML: shown as plain text, never interpreted.

Parsing follows CommonMark and is done by [Markdig](https://github.com/xoofx/markdig).
