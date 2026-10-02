# Deploy on Linux and Docker

ShinyPDF renders with SkiaSharp and HarfBuzzSharp, which call into native libraries. On Windows and macOS these native libraries are part of the package dependencies, but on Linux you have to add them yourself, and the machine or container needs a few system libraries. This guide lists what you need and gives an example Dockerfile.

## Add the Linux native assets

Since version 2.0.0, the ShinyPDF package does not bring the Linux native libraries for SkiaSharp and HarfBuzzSharp. Add both native asset packages to the project that runs on Linux (your application, not a class library in between):

```bash
dotnet add package SkiaSharp.NativeAssets.Linux
dotnet add package HarfBuzzSharp.NativeAssets.Linux
```

Without them, the application compiles and starts, but fails as soon as it renders the first document:

```text
System.DllNotFoundException: Unable to load shared library 'libSkiaSharp' or one of its dependencies.
```

Depending on where the error occurs, it can also arrive wrapped in a ShinyPDF `InitializationException` ("Cannot create the PDF document using the SkiaSharp library") with the `DllNotFoundException` as the inner exception. The same applies to `libHarfBuzzSharp`.

### Use matching versions

The native assets must match the managed SkiaSharp and HarfBuzzSharp versions that ShinyPDF uses. Check which versions your project resolved:

```bash
dotnet list package --include-transitive
```

Look for `SkiaSharp` and `HarfBuzzSharp` in the transitive packages and add the native assets with the same versions. For the current ShinyPDF source, these are:

```xml
<ItemGroup>
  <PackageReference Include="ShinyPDF" Version="*" />
  <PackageReference Include="SkiaSharp.NativeAssets.Linux" Version="4.153.1" />
  <PackageReference Include="HarfBuzzSharp.NativeAssets.Linux" Version="14.2.1.301" />
</ItemGroup>
```

Replace `*` with the ShinyPDF version you use, and update the native asset versions whenever a ShinyPDF update changes the SkiaSharp or HarfBuzzSharp version. A mismatch can lead to errors at runtime that are hard to diagnose.

The packages only add Linux binaries, so you can keep them in the project unconditionally even if you also develop on Windows or macOS.

## Install the native system libraries

SkiaSharp's Linux binary depends on system libraries for fonts and image formats. The ShinyPDF repository develops and tests in a container based on `mcr.microsoft.com/dotnet/sdk:10.0` (Ubuntu 24.04) with these packages (see `.devcontainer/devcontainer.json`):

```bash
apt-get update
apt-get install -y libfontconfig1 libfreetype6 libpng16-16t64 libharfbuzz0b libjpeg-turbo8 libgif7 libwebp7
```

| Package | Purpose |
| --- | --- |
| `libfontconfig1` | Font configuration and lookup of system fonts. `libSkiaSharp` links against it. |
| `libfreetype6` | Font rasterization. |
| `libharfbuzz0b` | Text shaping. |
| `libpng16-16t64`, `libjpeg-turbo8`, `libgif7`, `libwebp7` | PNG, JPEG, GIF and WebP image codecs. |

> **Note:** This is the set the repository uses; it has not been reduced to a verified minimum. The package names are the Ubuntu 24.04 names. On other distributions or versions, the names can differ (for example `libpng16-16` instead of `libpng16-16t64` on older releases); install the equivalent packages.

## Fonts on Linux

ShinyPDF embeds its default fonts in the library and registers them automatically the first time it renders text, so they work in any container without installed system fonts:

- `Fonts.Lato` (`"Lato"`), the default font of every text,
- `Fonts.NotoSans` (`"Noto Sans"`), including condensed variants and Noto Color Emoji,
- `Fonts.Courier` (`"Courier"`).

If your document uses any other font family, ShinyPDF first looks at the fonts you registered with `FontManager` and then asks the operating system through fontconfig. Minimal Linux images contain few or no fonts, so a family that works on your Windows machine (for example `"Arial"`) may be missing in the container. Generation then fails with an `ArgumentException` ("The typeface '...' could not be found") or, if the system substitutes a font, renders with different metrics than on your development machine.

To get identical output everywhere, ship the font files with your application and register them at startup instead of relying on system fonts. [Use custom fonts](fonts.md) shows how.

Characters that the chosen font does not contain are another common source of differences between machines. Enable `Settings.CheckIfAllTextGlyphsAreAvailable` in your tests to catch them (see [Generate PDF, XPS and images](generate-output.md#global-settings)).

## Example Dockerfile

The following Dockerfile is an **example to adapt**, not a tested production setup. It builds a console or web application called `MyApp` in a multi-stage build and installs the system libraries listed above in the runtime image.

```dockerfile
# Example only: adapt project names, base image and packages to your application.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish MyApp/MyApp.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:10.0
# Use mcr.microsoft.com/dotnet/aspnet:10.0 instead for an ASP.NET Core application.

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        libfontconfig1 libfreetype6 libpng16-16t64 libharfbuzz0b libjpeg-turbo8 libgif7 libwebp7 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

Points to check when you adapt it:

- `MyApp.csproj` must reference `SkiaSharp.NativeAssets.Linux` and `HarfBuzzSharp.NativeAssets.Linux`. After `dotnet publish`, the output contains `runtimes/linux-x64/native/libSkiaSharp.so` (or the folder for your architecture).
- The package names match the Ubuntu 24.04 based .NET 10 images. If you switch to a different base image (for example a Debian, Alpine or "chiseled" variant), check the package names and whether a package manager is available at all.
- If your documents use fonts other than the embedded ones, copy the font files into the image (or embed them in your assembly) and register them as described in [Use custom fonts](fonts.md).

## Troubleshooting

| Symptom | Likely cause |
| --- | --- |
| `DllNotFoundException: Unable to load shared library 'libSkiaSharp'` | `SkiaSharp.NativeAssets.Linux` is not referenced by the application project, or a system library that `libSkiaSharp` needs (such as `libfontconfig1`) is missing. |
| `DllNotFoundException` for `libHarfBuzzSharp` | `HarfBuzzSharp.NativeAssets.Linux` is not referenced. |
| `InitializationException` when generating a PDF | One of the two causes above; read the inner exception for the library name. |
| `ArgumentException: The typeface '...' could not be found` | The font is neither registered with `FontManager` nor installed in the container. |
| Text looks different than on Windows | A system font was substituted; register the font files yourself. |
