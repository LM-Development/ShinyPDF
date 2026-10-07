namespace ShinyPDF.Infrastructure
{
    internal enum StrikethroughPosition
    {
        /// <summary>The strikethrough follows the shifted sub/superscript glyphs.</summary>
        ThroughGlyphs,
        /// <summary>The strikethrough is drawn where it would be for normal text, aligned with surrounding text.</summary>
        Baseline
    }
}
