# ShinyPDF - .NET library for PDF generation with a C# Fluent API

The single orientation file for any AI agent (Claude Code, Copilot, Codex, ...) and any human working in this repo. Keep it current.

## Project

- Status: active. Open-source library, published to NuGet as `ShinyPDF`. Fork of the last MIT-licensed version of QuestPDF.
- Stack: C# / .NET 10 (SDK pinned in `src/global.json`), SkiaSharp + HarfBuzzSharp for rendering, NUnit for tests.
- Layout: `src/ShinyPDF` (library: `Fluent` = public API, `Elements`, `Drawing`, `Infrastructure`, `Helpers`), `src/ShinyPDF.UnitTests` (fast tests with an operation-recording test engine), `src/ShinyPDF.Examples` (NUnit tests that render real PDFs/images).

## Key rules

- Never commit `.env`, `*.tfvars`, or `.claude/settings.local.json`.
- The public API (`ShinyPDF.Fluent`, public types) is used by consumers: breaking changes need a `BREAKING CHANGE:` commit footer (major release).
- SkiaSharp, HarfBuzzSharp and their `NativeAssets.*` packages must stay on matching versions, including the Linux ones in `Directory.Build.props`.
- Native asset references in `Directory.Build.props` keep `PrivateAssets="all"` so they never become package dependencies.
- The `Grid` element is deprecated; use `Table` or `Row`/`Column` in new code and examples.
- Use NUnit `Assert.That` in tests (no FluentAssertions).

## Commands

```bash
dotnet build src/ShinyPDF.slnx                                    # build all projects
dotnet test src/ShinyPDF.UnitTests/ShinyPDF.UnitTests.csproj      # unit tests (seconds, what CI runs)
dotnet test src/ShinyPDF.Examples/ShinyPDF.Examples.csproj        # example renders (~8 min, writes PDFs/PNGs to bin/)
dotnet pack src/ShinyPDF/ShinyPDF.csproj -c Release -o out        # local NuGet package
```

Example tests marked `.ShowResults()` try to open the generated file in a viewer; run them only when rendering changed.
On Linux (or with `SHINYPDF_DEVCONTAINER=true`) the Linux native assets are added automatically.

## Workflow

- GitHub: `LM-Development/ShinyPDF`, use `gh` for issues and PRs. Branch from `main`, open a PR, CI (`pull-request.yaml`) builds and runs the unit tests.
- Commits follow Conventional Commits. Every push to `main` runs `release.yaml`: `feat:` / `fix:` / `BREAKING CHANGE` create a GitHub release and publish to NuGet; `chore:`, `test:`, `docs:` release nothing.
- Merge PRs with a merge commit, not squash, so commit types and footers reach `main` unchanged.
- Published NuGet versions cannot be deleted, only unlisted: check the commit type before merging.

## Current focus

Open issues: user documentation (#9, #22), more underline options (#11), template designer app (#12).
