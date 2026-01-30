# Getting Started

## Getting the library

Depending on the way you want to work with the library you can get the SVG library via NuGet, or roll your own binary from the sources or a personal fork of the sources. 

### Which version to choose?
The current version of SVG.NET is designed for modern .NET environments (targeting .NET 6, 8, 9, and .NET Standard 2.0/2.1). It uses SkiaSharp as the underlying rendering engine, providing full cross-platform compatibility without the need for GDI+ or `libgdiplus`.

If you are starting a new project, you should use the latest version to benefit from modern .NET performance and cross-platform support.

### Installing via NuGet
The library is available as a NuGet package in the public NuGet feed (https://www.nuget.org/packages/Svg/).
Depending on your development stack you can add it to your code base.

In Visual Studio you can add it by searching it in the NuGet wizard or by using the following command in the NuGet Console:
```
Install-Package Svg
```

When using the .NET command line client (`dotnet-cli`), you can add the package to your solution by running the following command in the terminal/console:
```
dotnet add package Svg
```

If you would like to add a specific version you can add the `--version` parameter to your command or pick a specific version in the wizard.
If you want to use pre-release versions in Visual Studio you need to check the box regarding pre-release packages to be able to select pre-release versions.

### Rolling your own version
If you would like to roll your own version you can download the sources via GitHub, clone the repository to your local machine,
or create a fork in your own account and download/clone this to your machine. This will give you more flexibility in choosing the target framework(s) and compiler flags.

Once you downloaded the sources you can use the IDE of your choice to open the solution file (`Svg.sln`) or the Svg library project (`Svg.csproj`)
and use your IDE to compile the version you would like to have.

If you would like to use `dotnet-cli` to build the sources you can use the following command in the `Sources/` folder to build the library
for example for .NET 8.0 with the compiler setting for release:
```
dotnet build -c release -f net8.0 Svg.csproj
```
This will put the output into the `bin/Release/net8.0/` folder.

## Cross-Platform Support (Windows, Linux, macOS, etc.)
Unlike previous versions that relied on GDI+, the current library uses **SkiaSharp**. This means it runs natively on any platform supported by SkiaSharp.

To ensure the library works on your target platform, you must include the appropriate native assets package for SkiaSharp.

### Native Assets Packages
Add the relevant NuGet package to your **executable project**:

*   **Windows**: `SkiaSharp.NativeAssets.Win32`
*   **Linux**: `SkiaSharp.NativeAssets.Linux` or `SkiaSharp.NativeAssets.Linux.NoDependencies`
*   **macOS**: `SkiaSharp.NativeAssets.macOS`
*   **iOS / Android**: These platforms typically include the necessary assets via their respective SkiaSharp view packages.

For most general-purpose cross-platform applications, you can use the `SkiaSharp` umbrella package, but ensuring the specific `NativeAssets` package is present is recommended for server/CLI environments.

## Linking the library in your application
If you have installed or built the library, it's time to add it to your application.
If you used the NuGet approach, the reference should already be set correctly.

If you rolled your own version, you can link the `.csproj` to your own project via your IDE. If you want to do it through `dotnet-cli` you can run:
```
dotnet add reference SVG/Source/Svg.csproj
```
(where SVG is the root folder you downloaded the sources to).
This approach will also take over all references required to the target project.
This will also compile the Svg sources when you build your own project, which might be useful if you plan to make changes in the Svg project yourself.

If you don't want to reference the project, you can get the `Svg.dll` file from the output folders after you compiled the project with the steps outlined above and reference it.
The Svg library requires `SkiaSharp.dll` and `ExCSS.dll` to function. 
Additionally, remember that you need the native SkiaSharp libraries (`libSkiaSharp`) for your target OS.

## Using the library (examples)
This part will be extended in the future, for now please refer to the [Q&A](http://svg-net.github.io/SVG/articles/Faq.html) for examples of how to use the library.

## Troubleshooting
If you encounter any problems or difficulties, please refer to the [Q&A part of the documentation](http://svg-net.github.io/SVG/articles/Faq.html).
If the Q&A does not solve your problem, please open a ticket with your request.
