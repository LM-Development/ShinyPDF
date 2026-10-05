# ShinyPDF documentation

ShinyPDF is a .NET library for generating PDF documents with a C# Fluent API. This documentation is organized by what you need right now: learning the basics, solving a specific task, looking up an API, or understanding how the library works.

New to ShinyPDF? Start with the [getting started tutorial](tutorials/getting-started.md).

## Tutorials

Step-by-step lessons that build a working document from scratch.

- [Getting started](tutorials/getting-started.md): install the package and build your first document

## How-to guides

Recipes for specific tasks.

- [Generate PDF, XPS and images](how-to/generate-output.md): files, streams, images per page, metadata and settings
- [Headers, footers and page numbers](how-to/headers-footers-page-numbers.md): page setup, repeating content and numbering
- [Use custom fonts](how-to/fonts.md): register fonts, fallback for missing glyphs, right-to-left text
- [Build reusable components](how-to/reusable-components.md): components, extension methods and dynamic content
- [Render Markdown](how-to/render-markdown.md): the `ShinyPDF.Markdown` package, styling options and supported syntax
- [Debug layout issues](how-to/debug-layout-issues.md): find out why content does not fit or a layout fails
- [Deploy on Linux and Docker](how-to/deploy-on-linux.md): native assets, system libraries and fonts

## Reference

Lookup material for the public API.

- [Elements](reference/elements.md): layout containers, sizing, spacing, visual elements, paging and navigation
- [Text](reference/text.md): text content, spans, page numbers and text styles

## Explanation

Background on how ShinyPDF works.

- [Layout engine](explanation/layout-engine.md): measuring and drawing, paging, layout exceptions and the rendering pipeline

## More examples

The [examples project](../src/ShinyPDF.Examples) contains a rendering test for almost every feature. Each test produces a PDF or images, so it is a good place to see an element in action.
