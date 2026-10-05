namespace ShinyPDF.Infrastructure
{
    internal enum UnderlinePosition
    {
        /// <summary>Superscript underlines stay on the baseline, subscript underlines follow the lowered glyphs.</summary>
        Auto,
        /// <summary>The underline is drawn where it would be for normal text, aligned with surrounding text.</summary>
        Baseline,
        /// <summary>The underline is drawn directly below the shifted sub/superscript glyphs.</summary>
        BelowGlyphs
    }
}
