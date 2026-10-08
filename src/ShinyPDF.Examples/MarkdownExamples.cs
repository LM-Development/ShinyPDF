using System.IO;
using NUnit.Framework;
using ShinyPDF.Examples.Engine;
using ShinyPDF.Fluent;
using ShinyPDF.Helpers;

namespace ShinyPDF.Examples
{
    public class MarkdownExamples
    {
        private const string ReleaseNotes = """
            # Release notes

            Version **2.1** adds *Markdown* rendering to ShinyPDF. Content is laid out by the
            regular engine, so it pages and wraps like any other text. See [the repository](https://github.com/LM-Development/ShinyPDF).

            ## What is new

            1. A `Markdown()` element for any container
            2. Headings, lists, quotes and code blocks
            3. ~~Manual conversion~~ is no longer needed

            ### Supported inline styles

            - **Bold**, *italic* and ***both***
            - `inline code` and [links](https://example.com)
              - nested lists work too
              - with their own bullets

            > Markdown in, PDF out.
            > Quotes can span several lines.

            ---

            ```csharp
            page.Content().Markdown(markdown, options =>
            {
                options.BaseFontSize = 11;
            });
            ```
            """;

        private const string ExtendedFeatures = """
            ## Tables, tasks and alerts

            | Package | Version | Downloads |
            |:--------|:-------:|----------:|
            | ShinyPDF | 2.1 | 12,400 |
            | ShinyPDF.Markdown | 2.1 | 1,250 |

            - [x] Tables with column alignment
            - [x] Task lists
            - [ ] Math formulas

            > [!NOTE]
            > Alerts use the GitHub syntax and colors.

            > [!WARNING]
            > Images are only loaded through `ImageResolver` or `data:` URIs.

            ![ShinyPDF logo](logo.png)

            ```csharp
            // syntax highlighting for common languages
            public static int Add(int a, int b) => a + b;
            var text = $"Sum: {Add(1, 2)}";
            ```

            ```json
            { "name": "ShinyPDF", "version": 2.1, "stable": true }
            ```

            ```sql
            SELECT Name, COUNT(*) FROM Packages WHERE Version = '2.1' -- latest
            ```
            """;

        [Test]
        public void MarkdownExtendedFeatures()
        {
            RenderingTest
                .Create()
                .PageSize(PageSizes.A4)
                .ProduceImages()
                .Render(container => container.Padding(40).Markdown(ExtendedFeatures, options =>
                {
                    options.ImageResolver = path => path == "logo.png" ? File.ReadAllBytes(path) : null;
                }));
        }

        [Test]
        public void Markdown()
        {
            RenderingTest
                .Create()
                .PageSize(PageSizes.A4)
                .ProduceImages()
                .Render(container => container.Padding(40).Markdown(ReleaseNotes));
        }
    }
}
