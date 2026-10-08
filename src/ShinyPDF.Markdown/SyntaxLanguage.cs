using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ShinyPDF.Markdown
{
    /// <summary>
    /// Highlighting rules for one language, matched against the language name of a fenced code block.
    /// Rules are regular expressions tried in the order they were added, so add comments and strings
    /// before keywords. Text that no rule matches keeps the default code color.
    /// </summary>
    public class SyntaxLanguage
    {
        private const string GroupPrefix = "shinypdf";

        private readonly List<(SyntaxTokenKind Kind, string Pattern)> Rules = new();
        private Regex? CompiledRules;

        /// <param name="names">Name and aliases used after the opening fence, e.g. "csharp", "cs". Matched case-insensitively.</param>
        public SyntaxLanguage(params string[] names)
        {
            if (names == null || names.Length == 0 || names.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("A syntax language needs at least one non-empty name.", nameof(names));

            Names = names;
        }

        public IReadOnlyList<string> Names { get; }

        /// <summary>Adds a regular expression whose matches are colored as <paramref name="kind"/>. Patterns run in multiline mode.</summary>
        public SyntaxLanguage Rule(SyntaxTokenKind kind, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentException("Pattern must not be empty.", nameof(pattern));

            // fail early on invalid patterns instead of on the first render
            _ = new Regex(pattern);

            Rules.Add((kind, pattern));
            CompiledRules = null;
            return this;
        }

        /// <summary>Colors the given words as keywords (whole words only, case-sensitive).</summary>
        public SyntaxLanguage Keywords(params string[] words) => Words(SyntaxTokenKind.Keyword, words, false);

        /// <summary>Colors the given words as types (whole words only, case-sensitive).</summary>
        public SyntaxLanguage Types(params string[] words) => Words(SyntaxTokenKind.Type, words, false);

        internal SyntaxLanguage Words(SyntaxTokenKind kind, IEnumerable<string> words, bool ignoreCase)
        {
            var alternatives = string.Join("|", words.Select(Regex.Escape));
            var pattern = $@"(?<![\w$])(?:{alternatives})(?![\w$])";
            return Rule(kind, ignoreCase ? $"(?i:{pattern})" : pattern);
        }

        internal bool HasName(string name)
        {
            return Names.Any(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Splits code into consecutive pieces; pieces without a kind are plain code.</summary>
        internal IEnumerable<(SyntaxTokenKind? Kind, string Text)> Tokenize(string code)
        {
            if (Rules.Count == 0)
            {
                yield return (null, code);
                yield break;
            }

            var regex = CompiledRules ??= Compile();
            var position = 0;

            foreach (Match match in regex.Matches(code))
            {
                if (match.Length == 0)
                    continue;

                if (match.Index > position)
                    yield return (null, code[position..match.Index]);

                yield return (GetKind(match), match.Value);
                position = match.Index + match.Length;
            }

            if (position < code.Length)
                yield return (null, code[position..]);
        }

        private Regex Compile()
        {
            var alternatives = Rules.Select((rule, index) => $"(?<{GroupPrefix}{index}>{rule.Pattern})");
            return new Regex(string.Join("|", alternatives), RegexOptions.Multiline | RegexOptions.CultureInvariant);
        }

        private SyntaxTokenKind GetKind(Match match)
        {
            for (var index = 0; index < Rules.Count; index++)
            {
                if (match.Groups[$"{GroupPrefix}{index}"].Success)
                    return Rules[index].Kind;
            }

            throw new InvalidOperationException("Matched text does not belong to any rule.");
        }
    }
}
