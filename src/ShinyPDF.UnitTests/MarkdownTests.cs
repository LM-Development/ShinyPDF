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
using SkiaSharp;

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
        public void Image_WithoutResolver_DoesNotLoadFiles()
        {
            var root = Render("![a logo](logo.png)");

            Assert.That(Descendants(root).OfType<Image>(), Is.Empty);
        }

        [Test]
        public void Image_FromDataUri_IsRendered()
        {
            var root = Render($"![dot](data:image/png;base64,{Convert.ToBase64String(CreatePng(4, 2))})");

            Assert.That(Descendants(root).OfType<Image>().Single().InternalImage.Width, Is.EqualTo(4));
            Assert.That(Descendants(root).OfType<Constrained>().Any(x => x.MaxWidth == 4), Is.True);
            Assert.That(Spans(root), Is.Empty);
        }

        [Test]
        public void Image_IsLoadedThroughResolver()
        {
            var requested = new List<string>();
            var root = Render("text ![chart](images/chart.png) text", x => x.ImageResolver = url =>
            {
                requested.Add(url);
                return CreatePng(10, 10);
            });

            Assert.That(requested, Is.EqualTo(new[] { "images/chart.png" }));
            Assert.That(Descendants(root).OfType<Image>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Image_ThatCannotBeDecoded_FallsBackToAltText()
        {
            var root = Render("![broken](broken.png)", x => x.ImageResolver = _ => new byte[] { 1, 2, 3 });

            Assert.That(Spans(root).Single().Text, Is.EqualTo("broken"));
            Assert.That(Descendants(root).OfType<Image>(), Is.Empty);
        }

        [Test]
        public void Image_WithValidHeaderButCorruptPixels_FallsBackToAltText()
        {
            var png = CreatePng(64, 64, noisy: true);
            var truncated = png.Take(png.Length / 2).ToArray();

            using (var codec = SKCodec.Create(new SKMemoryStream(truncated)))
                Assert.That(codec, Is.Not.Null, "the header must stay readable for this test");

            var root = Render("![truncated](a.png)", x => x.ImageResolver = _ => truncated);

            Assert.That(Spans(root).Single().Text, Is.EqualTo("truncated"));
            Assert.That(Descendants(root).OfType<Image>(), Is.Empty);
        }

        [Test]
        public void Image_InsideLink_IsClickable()
        {
            var root = Render("[![logo](logo.png)](https://example.com)", x => x.ImageResolver = _ => CreatePng(4, 4));

            var hyperlink = Descendants(root).OfType<Hyperlink>().Single();
            Assert.That(hyperlink.Url, Is.EqualTo("https://example.com"));
            Assert.That(Descendants(hyperlink).OfType<Image>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Image_TallerThanMaxImageHeight_IsScaledDown()
        {
            var root = Render("![tall](tall.png)", x => x.ImageResolver = _ => CreatePng(200, 1200));

            var expectedWidth = 200 * new MarkdownOptions().MaxImageHeight / 1200;
            Assert.That(Descendants(root).OfType<Constrained>().Single(x => x.MaxWidth != null).MaxWidth, Is.EqualTo(expectedWidth).Within(0.01));
        }

        [Test]
        public void Image_TallerThanPage_StillGeneratesPdf()
        {
            var pdf = Document
                .Create(document => document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Content().Markdown("![tall](tall.png)", x => x.ImageResolver = _ => CreatePng(200, 1200));
                }))
                .GeneratePdf();

            Assert.That(pdf, Is.Not.Empty);
        }

        [Test]
        public void Table_RendersHeaderAndBodyCells()
        {
            var options = new MarkdownOptions();
            var root = Render("| Name | Qty |\n|---|---|\n| Apple | 3 |\n| Pear |");

            Assert.That(Spans(root).Select(x => x.Text), Is.EquivalentTo(new[] { "Name", "Qty", "Apple", "3", "Pear" }));

            var header = Descendants(root).OfType<Background>().Where(x => x.Color == options.TableHeaderBackgroundColor).ToList();
            Assert.That(header, Has.Count.EqualTo(2));
            Assert.That(header.Select(x => Descendants(x).OfType<DefaultTextStyle>().First().TextStyle.FontWeight), Is.All.EqualTo(FontWeight.Bold));

            var cellBorders = Descendants(root).OfType<Border>().Where(x => x.Color == options.TableBorderColor);
            Assert.That(cellBorders.Count(), Is.EqualTo(6));
        }

        [Test]
        public void Table_AlignsEachTextLineOfAlignedColumns()
        {
            var root = Render("| L | C | R |\n|:--|:-:|--:|\n| a | b | c |");

            // alignment is set on the text block, so every wrapped line is aligned, not only the block as a whole
            var alignments = Descendants(root).OfType<TextBlock>().ToDictionary(x => x.Text, x => x.Alignment);
            Assert.That(alignments["L"], Is.Null);
            Assert.That(alignments["a"], Is.Null);
            Assert.That(alignments["C"], Is.EqualTo(HorizontalAlignment.Center));
            Assert.That(alignments["b"], Is.EqualTo(HorizontalAlignment.Center));
            Assert.That(alignments["R"], Is.EqualTo(HorizontalAlignment.Right));
            Assert.That(alignments["c"], Is.EqualTo(HorizontalAlignment.Right));
        }

        [Test]
        public void Table_WithWrappedAlignedText_GeneratesPdf()
        {
            var markdown = "| Centered | Right |\n|:-:|--:|\n| " + string.Join(" ", Enumerable.Repeat("long text", 30)) + " | x |";

            var pdf = Document
                .Create(document => document.Page(page => page.Content().Markdown(markdown)))
                .GeneratePdf();

            Assert.That(pdf, Is.Not.Empty);
        }

        [Test]
        public void TaskList_ReplacesBulletsWithCheckboxes()
        {
            var options = new MarkdownOptions();
            var root = Render("- [x] done\n- [ ] open\n- regular");

            Assert.That(Spans(root).Select(x => x.Text), Is.EqualTo(new[] { "done", "open", "•", "regular" }));

            var checkboxes = Descendants(root).OfType<Border>().Where(x => x.Color == options.CheckboxColor).ToList();
            Assert.That(checkboxes, Has.Count.EqualTo(2));
            Assert.That(Descendants(checkboxes[0]).OfType<Background>().Single().Color, Is.EqualTo(options.CheckboxColor));
            Assert.That(Descendants(checkboxes[1]).OfType<Background>(), Is.Empty);
        }

        [Test]
        public void HtmlEntities_AreDecoded()
        {
            var root = Render("&copy; &amp; &#8364; &lt;tag&gt;");

            Assert.That(string.Concat(Spans(root).Select(x => x.Text)), Is.EqualTo("© & € <tag>"));
        }

        [Test]
        public void Alert_HasTitleAndAccentColor()
        {
            var style = new MarkdownOptions().AlertStyles["WARNING"];
            var root = Render("> [!WARNING]\n> Mind the gap.");

            var title = Span(root, style.Title);
            Assert.That(title.Style.Color, Is.EqualTo(style.Color));
            Assert.That(title.Style.FontWeight, Is.EqualTo(FontWeight.Bold));
            Assert.That(Descendants(root).OfType<Border>().Single().Color, Is.EqualTo(style.Color));
            Assert.That(Span(root, "Mind the gap."), Is.Not.Null);
            Assert.That(Spans(root), Has.Count.EqualTo(2));
        }

        [Test]
        public void AlertStyle_CanBeOverridden()
        {
            var root = Render("> [!NOTE]\n> text", x => x.AlertStyles["note"] = new MarkdownAlertStyle("Hinweis", Colors.Green.Darken2));

            Assert.That(Span(root, "Hinweis").Style.Color, Is.EqualTo(Colors.Green.Darken2));
        }

        [Test]
        public void CodeBlock_WithKnownLanguage_IsHighlighted()
        {
            var options = new MarkdownOptions();
            var root = Render("```csharp\nvar name = \"x\"; // note\nreturn 42;\n```");

            var colors = Spans(root)
                .Where(x => x.Text.Trim().Length > 0)
                .GroupBy(x => x.Text.Trim())
                .ToDictionary(x => x.Key, x => x.First().Style.Color);
            Assert.That(colors["var"], Is.EqualTo(options.SyntaxColors[SyntaxTokenKind.Keyword]));
            Assert.That(colors["\"x\""], Is.EqualTo(options.SyntaxColors[SyntaxTokenKind.String]));
            Assert.That(colors["// note"], Is.EqualTo(options.SyntaxColors[SyntaxTokenKind.Comment]));
            Assert.That(colors["42"], Is.EqualTo(options.SyntaxColors[SyntaxTokenKind.Number]));
            Assert.That(colors["name ="], Is.Null);
        }

        [Test]
        public void CodeBlock_WithUnknownLanguage_IsPlain()
        {
            var root = Render("```brainfuck\nvar x = 1;\n```");

            Assert.That(Spans(root).Single().Text, Is.EqualTo("var x = 1;"));
        }

        [Test]
        public void CodeBlock_CustomLanguage_CanBeAdded()
        {
            var root = Render("```Kotlin\nfun main() {}\n```", x => x.CodeLanguages.Add(new SyntaxLanguage("kotlin").Keywords("fun")));

            Assert.That(Span(root, "fun").Style.Color, Is.EqualTo(new MarkdownOptions().SyntaxColors[SyntaxTokenKind.Keyword]));
        }

        [Test]
        public void CodeBlock_ClearedLanguages_DisableHighlighting()
        {
            var root = Render("```csharp\nvar x = 1;\n```", x => x.CodeLanguages.Clear());

            Assert.That(Spans(root).Single().Text, Is.EqualTo("var x = 1;"));
        }

        [Test]
        public void CodeBlock_CustomRenderer_ReplacesCodeBox()
        {
            string received = null;
            var root = Render("```mermaid\ngraph TD\n  A --> B\n```", x => x.CodeBlockRenderers["mermaid"] = (container, code) =>
            {
                received = code;
                container.Text("diagram");
            });

            Assert.That(received, Is.EqualTo("graph TD\n  A --> B"));
            Assert.That(Spans(root).Single().Text, Is.EqualTo("diagram"));
            Assert.That(Descendants(root).OfType<Background>(), Is.Empty);
        }

        [Test]
        public void SyntaxLanguage_TokensCoverTheWholeCode()
        {
            const string code = "a /* b */ \"c\" 1 // d\nx";
            var tokens = SyntaxLanguages.JavaScript().Tokenize(code).ToList();

            Assert.That(string.Concat(tokens.Select(x => x.Text)), Is.EqualTo(code));
            Assert.That(tokens.Where(x => x.Kind == SyntaxTokenKind.Comment).Select(x => x.Text), Is.EqualTo(new[] { "/* b */", "// d" }));
        }

        [Test]
        public void SyntaxLanguage_StringsWinOverCommentMarkersInside()
        {
            var tokens = SyntaxLanguages.Python().Tokenize("x = \"# not a comment\"").ToList();

            Assert.That(tokens.Single(x => x.Kind != null), Is.EqualTo(((SyntaxTokenKind?)SyntaxTokenKind.String, "\"# not a comment\"")));
        }

        [TestCase("csharp", "public class A { int x = 0x1F; string s = @\"a\"\"b\"; }")]
        [TestCase("js", "const a = `t ${b}`; // c")]
        [TestCase("ts", "type A = { b: number };")]
        [TestCase("json", "{ \"a\": [1, true, null] }")]
        [TestCase("html", "<!-- c --><a href=\"x\">t</a>")]
        [TestCase("css", "a:hover { color: #fff; margin: 1px; }")]
        [TestCase("sql", "SELECT * FROM t WHERE a = 'x' -- c")]
        [TestCase("py", "def f(): return '''x'''")]
        [TestCase("bash", "echo \"$HOME\" # c")]
        [TestCase("powershell", "Get-Item $env:PATH # c")]
        [TestCase("yaml", "key: value\n- item: 1 # c")]
        public void BuiltInLanguages_HighlightSomethingAndKeepAllText(string name, string code)
        {
            var language = SyntaxLanguages.All().Single(x => x.HasName(name));
            var tokens = language.Tokenize(code).ToList();

            Assert.That(string.Concat(tokens.Select(x => x.Text)), Is.EqualTo(code));
            Assert.That(tokens.Count(x => x.Kind != null), Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public void Shell_HighlightsWholeUnquotedVariableNames()
        {
            var variables = SyntaxLanguages.Shell()
                .Tokenize("echo $HOME ${PATH} $1 $?")
                .Where(x => x.Kind == SyntaxTokenKind.Attribute)
                .Select(x => x.Text);

            Assert.That(variables, Is.EqualTo(new[] { "$HOME", "${PATH}", "$1", "$?" }));
        }

        [Test]
        public void SyntaxLanguage_RejectsInvalidPattern()
        {
            Assert.Catch<ArgumentException>(() => new SyntaxLanguage("x").Rule(SyntaxTokenKind.Keyword, "("));
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
            var markdown = $$"""
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

                | Feature | Status |
                |---|:-:|
                | Tables | done |

                - [x] task lists
                - [ ] more

                > [!TIP]
                > Alerts work too.

                ![pixel](data:image/png;base64,{{Convert.ToBase64String(CreatePng(2, 2))}})
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

        private static byte[] CreatePng(int width, int height, bool noisy = false)
        {
            using var bitmap = new SKBitmap(width, height);
            bitmap.Erase(SKColors.Red);

            // noise keeps the compressed pixel data large, so truncating the file cuts into it
            if (noisy)
            {
                for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    bitmap.SetPixel(x, y, new SKColor(unchecked((uint)(x * 7919 ^ y * 104729) * 2654435761u) | 0xFF000000));
            }

            using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        private static IEnumerable<Element> Descendants(Element element)
        {
            yield return element;

            var inlineElements = (element as TextBlock)?.Items.OfType<TextBlockElement>().Select(x => x.Element) ?? Enumerable.Empty<Element>();

            foreach (var child in element.GetChildren().OfType<Element>().Concat(inlineElements))
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
