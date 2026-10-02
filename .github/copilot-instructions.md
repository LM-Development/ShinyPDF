# Copilot instructions

Follow [AGENTS.md](../AGENTS.md) in the repository root. It holds the project rules, commands, and workflow for all AI tools.

When reviewing pull requests, check in particular:

- Changes to the public API (`ShinyPDF.Fluent`, public types) that break consumers need a `BREAKING CHANGE:` commit footer.
- SkiaSharp, HarfBuzzSharp and their `NativeAssets.*` packages stay on matching versions.
- Tests use NUnit `Assert.That`.
