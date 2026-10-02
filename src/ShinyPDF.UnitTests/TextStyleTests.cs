using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ShinyPDF.Fluent;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.UnitTests
{
    [TestFixture]
    public class TextStyleTests
    {
        [Test]
        public void ApplyInheritedAndGlobalStyle()
        {
            // arrange
            var defaultTextStyle = TextStyle
                .Default
                .FontSize(20)
                .FontFamily("Lato")
                .BackgroundColor(Colors.Green.Lighten2)
                .Fallback(y => y
                    .FontFamily("Microsoft YaHei")
                    .Underline()
                    .NormalWeight()
                    .BackgroundColor(Colors.Blue.Lighten2));

            var spanTextStyle = TextStyle
                .Default
                .FontFamily("Times New Roman")
                .Bold()
                .Strikethrough()
                .BackgroundColor(Colors.Red.Lighten2);
            
            // act
            var targetStyle = spanTextStyle.ApplyInheritedStyle(defaultTextStyle).ApplyGlobalStyle();
            
            // assert
            var expectedStyle = TextStyle.LibraryDefault with
            {
                Size = 20, 
                FontFamily = "Times New Roman",
                FontWeight = FontWeight.Bold,
                BackgroundColor = Colors.Red.Lighten2,
                HasStrikethrough = true,
                Fallback = TextStyle.LibraryDefault with
                {
                    Size = 20,
                    FontFamily = "Microsoft YaHei",
                    FontWeight = FontWeight.Bold,
                    BackgroundColor = Colors.Red.Lighten2,
                    HasUnderline = true,
                    HasStrikethrough = true,
                    Fallback = null
                }
            };

            Assert.That(targetStyle, Is.EqualTo(expectedStyle), () => $"Expected: {Describe(expectedStyle)}\nBut was:  {Describe(targetStyle)}");
        }

        // TextStyle members are internal, so the record's ToString does not show them
        private static string Describe(TextStyle style)
        {
            if (style == null)
                return "null";

            var members = typeof(TextStyle)
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(x => x.Name != "EqualityContract")
                .Select(x => x.Name == nameof(TextStyle.Fallback)
                    ? $"{x.Name} = {{ {Describe(style.Fallback)} }}"
                    : $"{x.Name} = {x.GetValue(style) ?? "null"}");

            return string.Join(", ", members);
        }
    }
}