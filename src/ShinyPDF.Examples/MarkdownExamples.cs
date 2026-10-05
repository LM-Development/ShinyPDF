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
