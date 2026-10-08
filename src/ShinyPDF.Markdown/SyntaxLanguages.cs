using System.Collections.Generic;

namespace ShinyPDF.Markdown
{
    /// <summary>
    /// Ready-made highlighting rules. Every method returns a new instance, so it can be extended
    /// without affecting other documents. <see cref="MarkdownOptions.CodeLanguages"/> starts with <see cref="All"/>.
    /// </summary>
    public static class SyntaxLanguages
    {
        private const string LineComment = @"//.*";
        private const string BlockComment = @"/\*[\s\S]*?\*/";
        private const string HashComment = @"(?<![^\s])#.*";
        private const string DoubleQuoted = @"""(?:\\.|[^""\\\n])*""";
        private const string SingleQuoted = @"'(?:\\.|[^'\\\n])*'";
        private const string Number = @"(?<![\w$])(?:0[xX][0-9a-fA-F_]+|0[bB][01_]+|\d[\d_]*(?:\.\d[\d_]*)?(?:[eE][+-]?\d+)?)[a-zA-Z]*(?![\w$])";

        public static IEnumerable<SyntaxLanguage> All()
        {
            yield return CSharp();
            yield return JavaScript();
            yield return TypeScript();
            yield return Json();
            yield return Xml();
            yield return Css();
            yield return Sql();
            yield return Python();
            yield return Shell();
            yield return PowerShell();
            yield return Yaml();
        }

        public static SyntaxLanguage CSharp()
        {
            return new SyntaxLanguage("csharp", "cs", "c#")
                .Rule(SyntaxTokenKind.Comment, LineComment)
                .Rule(SyntaxTokenKind.Comment, BlockComment)
                .Rule(SyntaxTokenKind.String, @"\$*""""""[\s\S]*?""""""")
                .Rule(SyntaxTokenKind.String, @"(?:\$@|@\$|@)""(?:""""|[^""])*""")
                .Rule(SyntaxTokenKind.String, @"\$?" + DoubleQuoted)
                .Rule(SyntaxTokenKind.String, @"'(?:\\.|[^'\\\n])'")
                .Rule(SyntaxTokenKind.Keyword, @"^[ \t]*#[a-z]+.*")
                .Rule(SyntaxTokenKind.Number, Number)
                .Keywords(
                    "abstract", "as", "async", "await", "base", "break", "case", "catch", "checked", "class", "const",
                    "continue", "default", "delegate", "do", "else", "enum", "event", "explicit", "extern", "false",
                    "finally", "fixed", "for", "foreach", "get", "goto", "if", "implicit", "in", "init", "interface",
                    "internal", "is", "lock", "namespace", "new", "null", "operator", "out", "override", "params",
                    "partial", "private", "protected", "public", "readonly", "record", "ref", "required", "return",
                    "sealed", "set", "sizeof", "stackalloc", "static", "struct", "switch", "this", "throw", "true",
                    "try", "typeof", "unchecked", "unsafe", "using", "var", "virtual", "void", "volatile", "when",
                    "where", "while", "with", "yield")
                .Types(
                    "bool", "byte", "char", "decimal", "double", "dynamic", "float", "int", "long", "nint", "nuint",
                    "object", "sbyte", "short", "string", "uint", "ulong", "ushort");
        }

        public static SyntaxLanguage JavaScript()
        {
            return AddEcmaScriptRules(new SyntaxLanguage("javascript", "js", "jsx", "mjs", "cjs"));
        }

        public static SyntaxLanguage TypeScript()
        {
            return AddEcmaScriptRules(new SyntaxLanguage("typescript", "ts", "tsx"))
                .Keywords("abstract", "declare", "enum", "implements", "interface", "keyof", "namespace", "private",
                    "protected", "public", "readonly", "type", "satisfies")
                .Types("any", "boolean", "never", "number", "object", "string", "symbol", "unknown", "void");
        }

        private static SyntaxLanguage AddEcmaScriptRules(SyntaxLanguage language)
        {
            return language
                .Rule(SyntaxTokenKind.Comment, LineComment)
                .Rule(SyntaxTokenKind.Comment, BlockComment)
                .Rule(SyntaxTokenKind.String, @"`(?:\\.|[^`\\])*`")
                .Rule(SyntaxTokenKind.String, DoubleQuoted)
                .Rule(SyntaxTokenKind.String, SingleQuoted)
                .Rule(SyntaxTokenKind.Number, Number)
                .Keywords(
                    "async", "await", "break", "case", "catch", "class", "const", "continue", "debugger", "default",
                    "delete", "do", "else", "export", "extends", "false", "finally", "for", "from", "function", "if",
                    "import", "in", "instanceof", "let", "new", "null", "of", "return", "static", "super", "switch",
                    "this", "throw", "true", "try", "typeof", "undefined", "var", "void", "while", "with", "yield");
        }

        public static SyntaxLanguage Json()
        {
            return new SyntaxLanguage("json", "jsonc")
                .Rule(SyntaxTokenKind.Comment, LineComment)
                .Rule(SyntaxTokenKind.Comment, BlockComment)
                .Rule(SyntaxTokenKind.Attribute, DoubleQuoted + @"(?=\s*:)")
                .Rule(SyntaxTokenKind.String, DoubleQuoted)
                .Rule(SyntaxTokenKind.Number, @"-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?")
                .Keywords("true", "false", "null");
        }

        public static SyntaxLanguage Xml()
        {
            return new SyntaxLanguage("xml", "html", "xhtml", "svg", "xaml", "csproj", "razor")
                .Rule(SyntaxTokenKind.Comment, @"<!--[\s\S]*?-->")
                .Rule(SyntaxTokenKind.Keyword, @"<!\[CDATA\[[\s\S]*?\]\]>|<[!?][^>]*>")
                .Rule(SyntaxTokenKind.Tag, @"</?[\w:.\-]+|/?>")
                .Rule(SyntaxTokenKind.Attribute, @"(?<=\s)[\w:.\-]+(?=\s*=\s*[""'])")
                .Rule(SyntaxTokenKind.String, @"(?<==\s*)(?:""[^""]*""|'[^']*')");
        }

        public static SyntaxLanguage Css()
        {
            return new SyntaxLanguage("css", "scss", "less")
                .Rule(SyntaxTokenKind.Comment, BlockComment)
                .Rule(SyntaxTokenKind.String, DoubleQuoted)
                .Rule(SyntaxTokenKind.String, SingleQuoted)
                .Rule(SyntaxTokenKind.Keyword, @"@[\w\-]+|!important")
                .Rule(SyntaxTokenKind.Attribute, @"(?<=[{;\s])[\w\-]+(?=\s*:[^{]*?;)")
                .Rule(SyntaxTokenKind.Number, @"#[0-9a-fA-F]{3,8}\b|(?<![\w\-])-?\d*\.?\d+(?:%|[a-zA-Z]+)?");
        }

        public static SyntaxLanguage Sql()
        {
            return new SyntaxLanguage("sql", "tsql", "mysql", "postgresql", "psql")
                .Rule(SyntaxTokenKind.Comment, @"--.*")
                .Rule(SyntaxTokenKind.Comment, BlockComment)
                .Rule(SyntaxTokenKind.String, @"N?'(?:''|[^'])*'")
                .Rule(SyntaxTokenKind.Number, Number)
                .Words(SyntaxTokenKind.Keyword, new[]
                {
                    "add", "all", "alter", "and", "as", "asc", "begin", "between", "by", "case", "check", "column",
                    "commit", "constraint", "create", "cross", "database", "default", "delete", "desc", "distinct",
                    "drop", "else", "end", "exec", "exists", "foreign", "from", "full", "group", "having", "if", "in",
                    "index", "inner", "insert", "into", "is", "join", "key", "left", "like", "limit", "merge", "not",
                    "null", "offset", "on", "or", "order", "outer", "over", "partition", "primary", "procedure",
                    "references", "return", "returns", "right", "rollback", "select", "set", "table", "then", "top",
                    "transaction", "truncate", "union", "unique", "update", "values", "view", "when", "where", "with"
                }, true)
                .Words(SyntaxTokenKind.Type, new[]
                {
                    "bigint", "bit", "boolean", "char", "date", "datetime", "datetime2", "decimal", "float", "int",
                    "integer", "nchar", "numeric", "nvarchar", "real", "smallint", "text", "time", "timestamp",
                    "uniqueidentifier", "uuid", "varchar"
                }, true);
        }

        public static SyntaxLanguage Python()
        {
            return new SyntaxLanguage("python", "py")
                .Rule(SyntaxTokenKind.Comment, @"#.*")
                .Rule(SyntaxTokenKind.String, @"(?i:[rbuf]{0,2})(?:""""""[\s\S]*?""""""|'''[\s\S]*?''')")
                .Rule(SyntaxTokenKind.String, @"(?i:[rbuf]{0,2})(?:" + DoubleQuoted + "|" + SingleQuoted + ")")
                .Rule(SyntaxTokenKind.Keyword, @"^[ \t]*@[\w.]+")
                .Rule(SyntaxTokenKind.Number, Number)
                .Keywords(
                    "and", "as", "assert", "async", "await", "break", "class", "continue", "def", "del", "elif", "else",
                    "except", "False", "finally", "for", "from", "global", "if", "import", "in", "is", "lambda", "match",
                    "case", "None", "nonlocal", "not", "or", "pass", "raise", "return", "True", "try", "while", "with",
                    "yield", "self")
                .Types("bool", "bytes", "dict", "float", "int", "list", "object", "set", "str", "tuple");
        }

        public static SyntaxLanguage Shell()
        {
            return new SyntaxLanguage("bash", "sh", "shell", "zsh", "console")
                .Rule(SyntaxTokenKind.Comment, HashComment)
                .Rule(SyntaxTokenKind.String, DoubleQuoted)
                .Rule(SyntaxTokenKind.String, @"'[^']*'")
                .Rule(SyntaxTokenKind.Attribute, @"\$\{[^}]*\}|\$\w+|\$[@#?*!$-]")
                .Keywords(
                    "case", "do", "done", "elif", "else", "esac", "export", "fi", "for", "function", "if", "in",
                    "local", "return", "then", "until", "while");
        }

        public static SyntaxLanguage PowerShell()
        {
            return new SyntaxLanguage("powershell", "pwsh", "ps1", "ps")
                .Rule(SyntaxTokenKind.Comment, @"<#[\s\S]*?#>")
                .Rule(SyntaxTokenKind.Comment, HashComment)
                .Rule(SyntaxTokenKind.String, @"@""[\s\S]*?\n""@|@'[\s\S]*?\n'@")
                .Rule(SyntaxTokenKind.String, @"""(?:`.|[^""`])*""")
                .Rule(SyntaxTokenKind.String, @"'(?:''|[^'])*'")
                .Rule(SyntaxTokenKind.Attribute, @"\$(?:\{[^}]*\}|[\w:]+)")
                .Rule(SyntaxTokenKind.Type, @"\b[A-Za-z]+-[A-Za-z]+\b|\[[\w.\[\]]+\]")
                .Rule(SyntaxTokenKind.Number, Number)
                .Words(SyntaxTokenKind.Keyword, new[]
                {
                    "begin", "break", "catch", "class", "continue", "data", "do", "dynamicparam", "else", "elseif",
                    "end", "enum", "exit", "filter", "finally", "for", "foreach", "function", "if", "in", "param",
                    "process", "return", "switch", "throw", "trap", "try", "until", "using", "while"
                }, true);
        }

        public static SyntaxLanguage Yaml()
        {
            return new SyntaxLanguage("yaml", "yml")
                .Rule(SyntaxTokenKind.Comment, HashComment)
                .Rule(SyntaxTokenKind.Attribute, @"(?<=^[ \t]*(?:-[ \t]+)?)[\w.\-/]+(?=[ \t]*:(?:[ \t]|$))")
                .Rule(SyntaxTokenKind.String, DoubleQuoted)
                .Rule(SyntaxTokenKind.String, @"'(?:''|[^'])*'")
                .Rule(SyntaxTokenKind.Number, @"(?<=:[ \t]+|^[ \t]*-[ \t]+)-?\d+(?:\.\d+)?(?=[ \t]*(?:#|$))")
                .Rule(SyntaxTokenKind.Keyword, @"(?<=:[ \t]+|^[ \t]*-[ \t]+)(?:true|false|null|yes|no|on|off|~)(?=[ \t]*(?:#|$))");
        }
    }
}
