using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ShinyPDF.Elements;
using ShinyPDF.Elements.Text;
using ShinyPDF.Elements.Text.Items;
using ShinyPDF.Fluent;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;
using ShinyPDF.Markdown;

namespace ShinyPDF.UnitTests
{
    [TestFixture]
    public class MarkdownTests
    {
        [Test]
        public void Headings_AreBoldAndScaledFromBaseFontSize()
        {
            var root = Render("# Title\n\n###### Small");

            var title = Span(root, "Title");
            Assert.That(title.Style.Size, Is.EqualTo(24));
            Assert.That(title.Style.FontWeight, Is.EqualTo(FontWeight.Bold));

            var small = Span(root, "Small");
            Assert.That(small.Style.Size, Is.EqualTo(12 * 0.9f));
        }

        [Test]
        public void BaseFontSize_IsAppliedToBodyAndHeadings()
        {
            var root = Render("# Title\n\nBody", x => x.BaseFontSize = 10);

            Assert.That(Span(root, "Title").Style.Size, Is.EqualTo(20));
            Assert.That(Descendants(root).OfType<DefaultTextStyle>().First().TextStyle.Size, Is.EqualTo(10));
        }

        [Test]
        public void HeadingStyle_CanBeOverridden()
        {
            var root = Render("## Custom", x => x.HeadingStyle = level => TextStyle.Default.FontSize(level * 10).Italic());

            var heading = Span(root, "Custom");
            Assert.That(heading.Style.Size, Is.EqualTo(20));
            Assert.That(heading.Style.IsItalic, Is.True);
        }

        [Test]
        public void InlineFormatting_IsMappedToTextStyles()
        {
            var root = Render("plain **bold** *italic* ~~strike~~ `code`");

            Assert.That(Span(root, "plain ").Style.FontWeight, Is.Null);
            Assert.That(Span(root, "bold").Style.FontWeight, Is.EqualTo(FontWeight.Bold));
            Assert.That(Span(root, "italic").Style.IsItalic, Is.True);
            Assert.That(Span(root, "strike").Style.HasStrikethrough, Is.True);
            Assert.That(Span(root, "code").Style.FontFamily, Is.EqualTo(Fonts.Courier));
        }

        [Test]
        public void NestedEmphasis_CombinesStyles()
        {
            var root = Render("***both***");

            var span = Span(root, "both");
            Assert.That(span.Style.FontWeight, Is.EqualTo(FontWeight.Bold));
            Assert.That(span.Style.IsItalic, Is.True);
        }

        [Test]
        public void Link_BecomesStyledHyperlink()
        {
            var root = Render("see [**the** docs](https://example.com)");

            var links = Spans(root).OfType<TextBlockHyperlink>().ToList();
            Assert.That(links.Select(x => x.Text), Is.EqualTo(new[] { "the", " docs" }));
            Assert.That(links.Select(x => x.Url), Is.All.EqualTo("https://example.com"));
            Assert.That(links[0].Style.FontWeight, Is.EqualTo(FontWeight.Bold));
            Assert.That(links[1].Style.HasUnderline, Is.True);
            Assert.That(links[1].Style.Color, Is.EqualTo(new MarkdownOptions().LinkColor));
            Assert.That(Span(root, "see "), Is.Not.InstanceOf<TextBlockHyperlink>());
        }

        [Test]
        public void EmailAutolink_UsesMailto()
        {
            var root = Render("<info@example.com>");

            var link = (TextBlockHyperlink)Span(root, "info@example.com");
            Assert.That(link.Url, Is.EqualTo("mailto:info@example.com"));
        }

        [Test]
        public void BulletList_RendersMarkerPerItem()
        {
            var root = Render("- one\n- two\n  - nested");

            var texts = Spans(root).Select(x => x.Text).ToList();
            Assert.That(texts, Is.EqualTo(new[] { "•", "one", "•", "two", "•", "nested" }));
        }

        [Test]
        public void OrderedList_StartsAtGivenNumber()
        {
            var root = Render("3. three\n4. four\n5. five");

            var markers = Spans(root).Select(x => x.Text).Where((_, i) => i % 2 == 0);
            Assert.That(markers, Is.EqualTo(new[] { "3.", "4.", "5." }));
        }

        [Test]
        public void Blockquote_HasLeftBorderAndMutedText()
        {
            var options = new MarkdownOptions();
            var root = Render("> quoted");

            var border = Descendants(root).OfType<Border>().Single();
            Assert.That(border.Left, Is.EqualTo(3));
            Assert.That(border.Color, Is.EqualTo(options.BlockquoteBorderColor));
            Assert.That(Descendants(border).OfType<DefaultTextStyle>().Single().TextStyle.Color, Is.EqualTo(options.BlockquoteTextColor));
        }

        [Test]
        public void HorizontalRule_BecomesLine()
        {
            var root = Render("above\n\n---\n\nbelow");

            Assert.That(Descendants(root).OfType<Line>().Single().Color, Is.EqualTo(new MarkdownOptions().HorizontalRuleColor));
        }

        [Test]
        public void CodeBlock_KeepsLinesAndUsesCodeFont()
        {
            var root = Render("```\nvar x = 1;\n  var y = 2;\n```");

            var spans = Spans(root);
            Assert.That(spans.Select(x => x.Text), Is.EqualTo(new[] { "var x = 1;", "  var y = 2;" }));
            Assert.That(spans.Select(x => x.Style.FontFamily), Is.All.EqualTo(Fonts.Courier));
            Assert.That(Descendants(root).OfType<Background>().Single().Color, Is.EqualTo(new MarkdownOptions().CodeBackgroundColor));
        }

        [Test]
        public void Html_IsShownAsText()
        {
            var root = Render("<b>not bold</b>");

            Assert.That(string.Concat(Spans(root).Select(x => x.Text)), Is.EqualTo("<b>not bold</b>"));
        }

        [Test]
        public void Image_IsReplacedByAltText()
        {
            var root = Render("![a logo](logo.png)");

            Assert.That(Spans(root).Single().Text, Is.EqualTo("a logo"));
        }

        [Test]
        public void HardLineBreak_SplitsParagraphIntoLines()
        {
            var root = Render("first  \nsecond");

            var blocks = Descendants(root).OfType<TextBlock>().ToList();
            Assert.That(blocks.Select(x => x.Text.Trim()), Is.EqualTo(new[] { "first", "second" }));
        }

        [Test]
        public void EmptyMarkdown_RendersNothing()
        {
            Assert.That(Spans(Render(string.Empty)), Is.Empty);
        }

        [Test]
        public void NullMarkdown_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new Container().Markdown(null));
        }

        [Test]
        public void Document_WithAllFeatures_GeneratesPdf()
        {
            const string markdown = """
                # Release notes

                Version **2.1** adds *Markdown* support. See [the docs](https://github.com/LM-Development/ShinyPDF).

                ## Changes

                1. New `Markdown()` element
                2. Lists, quotes and ~~bugs~~ rules

                - nested
                  - items

                > Quoted text
                > over two lines

                ---

                ```csharp
                container.Markdown(text);
                ```
                """;

            var pdf = Document
                .Create(document => document.Page(page => page.Content().Markdown(markdown)))
                .GeneratePdf();

            Assert.That(pdf, Is.Not.Empty);
        }

        private static Element Render(string markdown, Action<MarkdownOptions> configure = null)
        {
            var container = new Container();
            container.Markdown(markdown, configure);
            return container;
        }

        private static IEnumerable<Element> Descendants(Element element)
        {
            yield return element;

            foreach (var child in element.GetChildren().OfType<Element>())
            foreach (var descendant in Descendants(child))
                yield return descendant;
        }

        private static List<TextBlockSpan> Spans(Element root)
        {
            return Descendants(root)
                .OfType<TextBlock>()
                .SelectMany(x => x.Items.OfType<TextBlockSpan>())
                .ToList();
        }

        private static TextBlockSpan Span(Element root, string text)
        {
            return Spans(root).Single(x => x.Text == text);
        }
    }
}
