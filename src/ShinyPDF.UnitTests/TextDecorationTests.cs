using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ShinyPDF.Drawing;
using ShinyPDF.Elements.Text.Calculation;
using ShinyPDF.Elements.Text.Items;
using ShinyPDF.Fluent;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;
using SkiaSharp;

namespace ShinyPDF.UnitTests
{
    [TestFixture]
    public class TextDecorationTests
    {
        [Test]
        public void Underline_ByDefault_IsSolidInTextColor()
        {
            var lines = Draw(TextStyle.Default.Underline().FontColor(Colors.Red.Medium));

            var line = lines.Single();
            Assert.That(line.Style, Is.EqualTo(TextDecorationStyle.Solid));
            Assert.That(line.Color, Is.EqualTo(Colors.Red.Medium));
        }

        [Test]
        public void DecorationColor_OverridesTextColor()
        {
            var lines = Draw(TextStyle.Default.Underline().FontColor(Colors.Red.Medium).DecorationColor(Colors.Blue.Medium));

            Assert.That(lines.Single().Color, Is.EqualTo(Colors.Blue.Medium));
        }

        [Test]
        public void DecorationThickness_OverridesFontThickness()
        {
            var lines = Draw(TextStyle.Default.Underline().DecorationThickness(3));

            Assert.That(lines.Single().Thickness, Is.EqualTo(3));
        }

        [TestCase((int)TextDecorationStyle.Double)]
        [TestCase((int)TextDecorationStyle.Dotted)]
        [TestCase((int)TextDecorationStyle.Dashed)]
        [TestCase((int)TextDecorationStyle.Wavy)]
        public void DecorationStyle_IsPassedToCanvas(int styleValue)
        {
            var style = (TextDecorationStyle)styleValue;
            var lines = Draw(TextStyle.Default.Underline().Mutate(TextStyleProperty.DecorationStyle, style));

            var line = lines.Single();
            Assert.That(line.Style, Is.EqualTo(style));
            Assert.That(line.Width, Is.GreaterThan(0));
        }

        [Test]
        public void DecorationOptions_ApplyToStrikethrough()
        {
            var lines = Draw(TextStyle.Default.Strikethrough().DecorationWavy().DecorationColor(Colors.Green.Medium).DecorationThickness(2));

            var line = lines.Single();
            Assert.That(line.Style, Is.EqualTo(TextDecorationStyle.Wavy));
            Assert.That(line.Color, Is.EqualTo(Colors.Green.Medium));
            Assert.That(line.Thickness, Is.EqualTo(2));
        }

        [Test]
        public void UnderlinePosition_Auto_KeepsExistingPlacement()
        {
            var normal = UnderlineOffset(TextStyle.Default);

            // superscript underline stays on the baseline, subscript underline moves down with the glyphs
            Assert.That(UnderlineOffset(TextStyle.Default.Superscript()), Is.EqualTo(normal));
            Assert.That(UnderlineOffset(TextStyle.Default.Subscript()), Is.GreaterThan(normal));
        }

        [Test]
        public void UnderlinePosition_Baseline_AlignsWithNormalText()
        {
            var normal = UnderlineOffset(TextStyle.Default);

            Assert.That(UnderlineOffset(TextStyle.Default.Subscript().UnderlineAtBaseline()), Is.EqualTo(normal));
            Assert.That(UnderlineOffset(TextStyle.Default.Superscript().UnderlineAtBaseline()), Is.EqualTo(normal));
        }

        [Test]
        public void UnderlinePosition_BelowGlyphs_FollowsShiftedGlyphs()
        {
            var normal = UnderlineOffset(TextStyle.Default);

            Assert.That(UnderlineOffset(TextStyle.Default.Superscript().UnderlineBelowGlyphs()), Is.LessThan(normal));
            Assert.That(UnderlineOffset(TextStyle.Default.Subscript().UnderlineBelowGlyphs()), Is.EqualTo(UnderlineOffset(TextStyle.Default.Subscript())));
        }

        [Test]
        public void DecorationProperties_AreInheritedAndOverridden()
        {
            var parent = TextStyle.Default
                .DecorationColor(Colors.Red.Medium)
                .DecorationThickness(2)
                .DecorationDashed()
                .UnderlineAtBaseline();

            var inherited = TextStyle.Default.ApplyInheritedStyle(parent);
            Assert.That(inherited.DecorationColor, Is.EqualTo(Colors.Red.Medium));
            Assert.That(inherited.DecorationThickness, Is.EqualTo(2));
            Assert.That(inherited.DecorationStyle, Is.EqualTo(TextDecorationStyle.Dashed));
            Assert.That(inherited.UnderlinePosition, Is.EqualTo(UnderlinePosition.Baseline));

            var overridden = TextStyle.Default.DecorationDotted().UnderlineBelowGlyphs().ApplyInheritedStyle(parent);
            Assert.That(overridden.DecorationStyle, Is.EqualTo(TextDecorationStyle.Dotted));
            Assert.That(overridden.UnderlinePosition, Is.EqualTo(UnderlinePosition.BelowGlyphs));
        }

        [Test]
        public void GlobalStyle_DefaultsToSolidAndAutoPosition()
        {
            var style = TextStyle.Default.ApplyGlobalStyle();

            Assert.That(style.DecorationStyle, Is.EqualTo(TextDecorationStyle.Solid));
            Assert.That(style.UnderlinePosition, Is.EqualTo(UnderlinePosition.Auto));
            Assert.That(style.DecorationColor, Is.Null);
            Assert.That(style.DecorationThickness, Is.Null);
        }

        [Test]
        public void DecorationThickness_MustBePositive()
        {
            Assert.Throws<ArgumentException>(() => TextStyle.Default.DecorationThickness(0));
        }

        [Test]
        public void DecorationColor_IsValidated()
        {
            Assert.Throws<ArgumentException>(() => TextStyle.Default.DecorationColor("not a color"));
        }

        [Test]
        public void Document_WithAllDecorationStyles_GeneratesPdf()
        {
            var pdf = Document
                .Create(document => document.Page(page => page.Content().Text(text =>
                {
                    text.Span("solid ").Underline();
                    text.Span("double ").Underline().DecorationDouble();
                    text.Span("dotted ").Underline().DecorationDotted();
                    text.Span("dashed ").Underline().DecorationDashed();
                    text.Span("wavy ").Underline().DecorationWavy().DecorationColor(Colors.Red.Medium);
                    text.Span("strike").Strikethrough().DecorationThickness(2);
                    text.Span("2").Subscript().Underline().UnderlineAtBaseline();
                })))
                .GeneratePdf();

            Assert.That(pdf, Is.Not.Empty);
        }

        private static float UnderlineOffset(TextStyle style)
        {
            return Draw(style.Underline()).Single().Offset;
        }

        private static List<DecorationLine> Draw(TextStyle style)
        {
            var span = new TextBlockSpan
            {
                Text = "Decorated",
                Style = style.ApplyGlobalStyle()
            };

            var canvas = new DecorationRecordingCanvas();
            var pageContext = new PageContext();

            var measurement = span.Measure(new TextMeasurementRequest
            {
                Canvas = canvas,
                PageContext = pageContext,
                AvailableWidth = 1000,
                IsFirstElementInBlock = true,
                IsFirstElementInLine = true
            })!;

            span.Draw(new TextDrawingRequest
            {
                Canvas = canvas,
                PageContext = pageContext,
                StartIndex = measurement.StartIndex,
                EndIndex = measurement.EndIndex,
                TotalAscent = measurement.Ascent,
                TextSize = new Size(measurement.Width, measurement.Height)
            });

            return canvas.Lines;
        }

        private record DecorationLine(float Offset, float Width, float Thickness, string Color, TextDecorationStyle Style);

        private class DecorationRecordingCanvas : ICanvas
        {
            public List<DecorationLine> Lines { get; } = new();

            public void DrawRectangle(Position vector, Size size, string color) => Lines.Add(new DecorationLine(vector.Y, size.Width, size.Height, color, TextDecorationStyle.Solid));
            public void DrawTextDecoration(Position vector, float width, float thickness, string color, TextDecorationStyle style) => Lines.Add(new DecorationLine(vector.Y, width, thickness, color, style));

            public void Translate(Position vector) { }
            public void DrawText(SKTextBlob skTextBlob, Position position, TextStyle style) { }
            public void DrawImage(SKImage image, Position position, Size size) { }
            public void DrawHyperlink(string url, Size size) { }
            public void DrawSectionLink(string sectionName, Size size) { }
            public void DrawSection(string sectionName) { }
            public void Rotate(float angle) { }
            public void Scale(float scaleX, float scaleY) { }
        }
    }
}
