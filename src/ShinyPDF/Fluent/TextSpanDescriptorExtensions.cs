using System;
using System.Runtime.CompilerServices;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;

namespace ShinyPDF.Fluent
{
    public static class TextSpanDescriptorExtensions
    {
        public static T Style<T>(this T descriptor, TextStyle style) where T : TextSpanDescriptor
        {
            if (style == null)
                return descriptor;
            
            descriptor.MutateTextStyle(x => x.OverrideStyle(style));
            return descriptor;
        }
        
        public static T Fallback<T>(this T descriptor, TextStyle? value = null) where T : TextSpanDescriptor
        {
            descriptor.TextStyle.Fallback = value;
            return descriptor;
        }
        
        public static T Fallback<T>(this T descriptor, Func<TextStyle, TextStyle> handler) where T : TextSpanDescriptor
        {
            return descriptor.Fallback(handler(TextStyle.Default));
        }
        
        public static T FontColor<T>(this T descriptor, string value) where T : TextSpanDescriptor
        {
            ColorValidator.Validate(value);
            descriptor.MutateTextStyle(x => x.FontColor(value));
            return descriptor;
        }
        
        public static T BackgroundColor<T>(this T descriptor, string value) where T : TextSpanDescriptor
        {
            ColorValidator.Validate(value);
            descriptor.MutateTextStyle(x => x.BackgroundColor(value));
            return descriptor;
        }
        
        public static T FontFamily<T>(this T descriptor, string value) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.FontFamily(value));
            return descriptor;
        }
        
        public static T FontSize<T>(this T descriptor, float value) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.FontSize(value));
            return descriptor;
        }
        
        public static T LineHeight<T>(this T descriptor, float value) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.LineHeight(value));
            return descriptor;
        }

        /// <summary>
        /// Letter spacing controls space between characters. Value 0 corresponds to normal spacing defined by a font.
        /// Positive values create additional space, whereas negative values reduce space between characters.
        /// Added / reduced space is relative to the font size.
        /// </summary>
        public static T LetterSpacing<T>(this T descriptor, float value) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.LetterSpacing(value));
            return descriptor;
        }

        public static T Italic<T>(this T descriptor, bool value = true) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.Italic(value));
            return descriptor;
        }
        
        public static T Strikethrough<T>(this T descriptor, bool value = true) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.Strikethrough(value));
            return descriptor;
        }
        
        public static T Underline<T>(this T descriptor, bool value = true) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.Underline(value));
            return descriptor;
        }

        public static T WrapAnywhere<T>(this T descriptor, bool value = true) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.WrapAnywhere(value));
            return descriptor;
        }

        #region Decoration

        /// <summary>
        /// Sets the color of underline and strikethrough lines. By default, the text color is used.
        /// </summary>
        public static T DecorationColor<T>(this T descriptor, string value) where T : TextSpanDescriptor
        {
            ColorValidator.Validate(value);
            descriptor.MutateTextStyle(x => x.DecorationColor(value));
            return descriptor;
        }

        /// <summary>
        /// Sets the thickness of underline and strikethrough lines, in points. By default, the thickness provided by the font is used.
        /// </summary>
        public static T DecorationThickness<T>(this T descriptor, float value) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.DecorationThickness(value));
            return descriptor;
        }

        public static T DecorationSolid<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.DecorationSolid());
            return descriptor;
        }

        public static T DecorationDouble<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.DecorationDouble());
            return descriptor;
        }

        public static T DecorationDotted<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.DecorationDotted());
            return descriptor;
        }

        public static T DecorationDashed<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.DecorationDashed());
            return descriptor;
        }

        public static T DecorationWavy<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.DecorationWavy());
            return descriptor;
        }

        /// <summary>
        /// Default. Superscript underlines stay on the baseline, subscript underlines follow the lowered glyphs.
        /// </summary>
        public static T UnderlinePositionAuto<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.UnderlinePositionAuto());
            return descriptor;
        }

        /// <summary>
        /// Draws sub/superscript underlines where they would be for normal text, aligned with the surrounding text.
        /// </summary>
        public static T UnderlineAtBaseline<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.UnderlineAtBaseline());
            return descriptor;
        }

        /// <summary>
        /// Draws sub/superscript underlines directly below the shifted glyphs.
        /// </summary>
        public static T UnderlineBelowGlyphs<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.UnderlineBelowGlyphs());
            return descriptor;
        }

        /// <summary>
        /// Default. Draws sub/superscript strikethroughs through the shifted glyphs.
        /// </summary>
        public static T StrikethroughThroughGlyphs<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.StrikethroughThroughGlyphs());
            return descriptor;
        }

        /// <summary>
        /// Draws sub/superscript strikethroughs where they would be for normal text, aligned with the surrounding text.
        /// </summary>
        public static T StrikethroughAtBaseline<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.StrikethroughAtBaseline());
            return descriptor;
        }

        #endregion

        #region Weight
        
        public static T Thin<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.Thin());
            return descriptor;
        }
        
        public static T ExtraLight<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.ExtraLight());
            return descriptor;
        }
        
        public static T Light<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.Light());
            return descriptor;
        }
        
        public static T NormalWeight<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.NormalWeight());
            return descriptor;
        }
        
        public static T Medium<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.Medium());
            return descriptor;
        }
        
        public static T SemiBold<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.SemiBold());
            return descriptor;
        }
        
        public static T Bold<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.Bold());
            return descriptor;
        }
        
        public static T ExtraBold<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.ExtraBold());
            return descriptor;
        }
        
        public static T Black<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.Black());
            return descriptor;
        }
        
        public static T ExtraBlack<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.ExtraBlack());
            return descriptor;
        }

        #endregion

        #region Position
        public static T NormalPosition<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.NormalPosition());
            return descriptor;
        }

        public static T Subscript<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.Subscript());
            return descriptor;
        }

        public static T Superscript<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.Superscript());
            return descriptor;
        }
        
        #endregion
        
        #region Direction

        public static T DirectionAuto<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.DirectionAuto());
            return descriptor;
        }
        
        public static T DirectionFromLeftToRight<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.DirectionFromLeftToRight());
            return descriptor;
        }
        
        public static T DirectionFromRightToLeft<T>(this T descriptor) where T : TextSpanDescriptor
        {
            descriptor.MutateTextStyle(x => x.DirectionFromRightToLeft());
            return descriptor;
        }

        #endregion
    }
}