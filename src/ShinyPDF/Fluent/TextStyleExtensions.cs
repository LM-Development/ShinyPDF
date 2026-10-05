using System;
using System.ComponentModel;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Fluent
{
    public static class TextStyleExtensions
    {
        public static TextStyle FontColor(this TextStyle style, string value)
        {
            ColorValidator.Validate(value);
            return style.Mutate(TextStyleProperty.Color, value);
        }
        
        public static TextStyle BackgroundColor(this TextStyle style, string value)
        {
            ColorValidator.Validate(value);
            return style.Mutate(TextStyleProperty.BackgroundColor, value);
        }
        
        public static TextStyle FontFamily(this TextStyle style, string value)
        {
            return style.Mutate(TextStyleProperty.FontFamily, value);
        }
        
        /// <include file='../Resources/Documentation.xml' path='documentation/doc[@for=\"text.fontSize\"]/*' />
        public static TextStyle FontSize(this TextStyle style, float value)
        {
            if (value <= 0)
                throw new ArgumentException("Font size must be greater than 0.");
            
            return style.Mutate(TextStyleProperty.Size, value);
        }
        
        public static TextStyle LineHeight(this TextStyle style, float value)
        {
            return style.Mutate(TextStyleProperty.LineHeight, value);
        }

        /// <summary>
        /// Letter spacing controls space between characters. Value 0 corresponds to normal spacing defined by a font.
        /// Positive values create additional space, whereas negative values reduce space between characters.
        /// Added / reduced space is relative to the font size.
        /// </summary>
        public static TextStyle LetterSpacing(this TextStyle style, float value)
        {
            return style.Mutate(TextStyleProperty.LetterSpacing, value);
        }

        public static TextStyle Italic(this TextStyle style, bool value = true)
        {
            return style.Mutate(TextStyleProperty.IsItalic, value);
        }
        
        public static TextStyle Strikethrough(this TextStyle style, bool value = true)
        {
            return style.Mutate(TextStyleProperty.HasStrikethrough, value);
        }
        
        public static TextStyle Underline(this TextStyle style, bool value = true)
        {
            return style.Mutate(TextStyleProperty.HasUnderline, value);
        }
        
        public static TextStyle WrapAnywhere(this TextStyle style, bool value = true)
        {
            return style.Mutate(TextStyleProperty.WrapAnywhere, value);
        }

        #region Decoration

        /// <summary>
        /// Sets the color of underline and strikethrough lines. By default, the text color is used.
        /// </summary>
        public static TextStyle DecorationColor(this TextStyle style, string value)
        {
            ColorValidator.Validate(value);
            return style.Mutate(TextStyleProperty.DecorationColor, value);
        }

        /// <summary>
        /// Sets the thickness of underline and strikethrough lines, in points. By default, the thickness provided by the font is used.
        /// </summary>
        public static TextStyle DecorationThickness(this TextStyle style, float value)
        {
            if (value <= 0)
                throw new ArgumentException("Decoration thickness must be greater than 0.");

            return style.Mutate(TextStyleProperty.DecorationThickness, value);
        }

        public static TextStyle DecorationSolid(this TextStyle style)
        {
            return style.DecorationStyle(TextDecorationStyle.Solid);
        }

        public static TextStyle DecorationDouble(this TextStyle style)
        {
            return style.DecorationStyle(TextDecorationStyle.Double);
        }

        public static TextStyle DecorationDotted(this TextStyle style)
        {
            return style.DecorationStyle(TextDecorationStyle.Dotted);
        }

        public static TextStyle DecorationDashed(this TextStyle style)
        {
            return style.DecorationStyle(TextDecorationStyle.Dashed);
        }

        public static TextStyle DecorationWavy(this TextStyle style)
        {
            return style.DecorationStyle(TextDecorationStyle.Wavy);
        }

        private static TextStyle DecorationStyle(this TextStyle style, TextDecorationStyle decorationStyle)
        {
            return style.Mutate(TextStyleProperty.DecorationStyle, decorationStyle);
        }

        /// <summary>
        /// Default. Superscript underlines stay on the baseline, subscript underlines follow the lowered glyphs.
        /// </summary>
        public static TextStyle UnderlinePositionAuto(this TextStyle style)
        {
            return style.UnderlinePosition(Infrastructure.UnderlinePosition.Auto);
        }

        /// <summary>
        /// Draws sub/superscript underlines where they would be for normal text, aligned with the surrounding text.
        /// </summary>
        public static TextStyle UnderlineAtBaseline(this TextStyle style)
        {
            return style.UnderlinePosition(Infrastructure.UnderlinePosition.Baseline);
        }

        /// <summary>
        /// Draws sub/superscript underlines directly below the shifted glyphs.
        /// </summary>
        public static TextStyle UnderlineBelowGlyphs(this TextStyle style)
        {
            return style.UnderlinePosition(Infrastructure.UnderlinePosition.BelowGlyphs);
        }

        private static TextStyle UnderlinePosition(this TextStyle style, UnderlinePosition position)
        {
            return style.Mutate(TextStyleProperty.UnderlinePosition, position);
        }

        #endregion

        #region Weight
        
        public static TextStyle Weight(this TextStyle style, FontWeight weight)
        {
            return style.Mutate(TextStyleProperty.FontWeight, weight);
        }
        
        public static TextStyle Thin(this TextStyle style)
        {
            return style.Weight(FontWeight.Thin);
        }
        
        public static TextStyle ExtraLight(this TextStyle style)
        {
            return style.Weight(FontWeight.ExtraLight);
        }
        
        public static TextStyle Light(this TextStyle style)
        {
            return style.Weight(FontWeight.Light);
        }
        
        public static TextStyle NormalWeight(this TextStyle style)
        {
            return style.Weight(FontWeight.Normal);
        }
        
        public static TextStyle Medium(this TextStyle style)
        {
            return style.Weight(FontWeight.Medium);
        }
        
        public static TextStyle SemiBold(this TextStyle style)
        {
            return style.Weight(FontWeight.SemiBold);
        }
        
        public static TextStyle Bold(this TextStyle style)
        {
            return style.Weight(FontWeight.Bold);
        }
        
        public static TextStyle ExtraBold(this TextStyle style)
        {
            return style.Weight(FontWeight.ExtraBold);
        }
        
        public static TextStyle Black(this TextStyle style)
        {
            return style.Weight(FontWeight.Black);
        }
        
        public static TextStyle ExtraBlack(this TextStyle style)
        {
            return style.Weight(FontWeight.ExtraBlack);
        }

        #endregion

        #region Position
        
        public static TextStyle NormalPosition(this TextStyle style)
        {
            return style.Position(FontPosition.Normal);
        }

        public static TextStyle Subscript(this TextStyle style)
        {
            return style.Position(FontPosition.Subscript);
        }

        public static TextStyle Superscript(this TextStyle style)
        {
            return style.Position(FontPosition.Superscript);
        }

        private static TextStyle Position(this TextStyle style, FontPosition fontPosition)
        {
            return style.Mutate(TextStyleProperty.FontPosition, fontPosition);
        }
        
        #endregion

        #region Fallback
        
        public static TextStyle Fallback(this TextStyle style, TextStyle? value = null)
        {
            return style.Mutate(TextStyleProperty.Fallback, value ?? TextStyle.Default);
        }
        
        public static TextStyle Fallback(this TextStyle style, Func<TextStyle, TextStyle> handler)
        {
            return style.Fallback(handler(TextStyle.Default));
        }

        #endregion

        #region Direction

        private static TextStyle TextDirection(this TextStyle style, TextDirection textDirection)
        {
            return style.Mutate(TextStyleProperty.Direction, textDirection);
        }
        
        public static TextStyle DirectionAuto(this TextStyle style)
        {
            return style.TextDirection(Infrastructure.TextDirection.Auto);
        }
        
        public static TextStyle DirectionFromLeftToRight(this TextStyle style)
        {
            return style.TextDirection(Infrastructure.TextDirection.LeftToRight);
        }
        
        public static TextStyle DirectionFromRightToLeft(this TextStyle style)
        {
            return style.TextDirection(Infrastructure.TextDirection.RightToLeft);
        }

        #endregion
    }
}