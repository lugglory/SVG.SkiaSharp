# Q & A
This is currently a collection of answered questions in issues that have been closed meanwhile.
The format of the page is preliminary and may be changed, if more questions accumulate.

## How to get started
Please use our [getting started article](http://svg-net.github.io/SVG/articles/GettingStarted.html) to get started with installation and implementation of the SVG library.

## How to re-render an SVG faster?

(from [#327](https://github.com/svg-net/SVG/issues/327), by @flemingtech)

The rendering type plays a significant role on rendering speeds. For example, if anti-aliasing is off for the `SvgDocument` render times are notably faster.

Because of the huge reduction in image quality, this wasn't a viable solution for my needs. Instead, what I've come up with so far seems to work since I can't figure out how to get clipping regions to work.

After I load the SVG, I make new SVG with the same initial `SvgDocumen`t properties (basically a deep copy followed by deleting all children). As I walk the first document tree I'm looking for elements I know are going to be modified. For each one that I find, I remove it from the first SVG and put it into the 2nd SVG. When I'm doing this, I also apply any parent transforms to the new child since it doesn't need/have all of its parents.

Once I'm done, I render the first SVG to an `Image`. When any of the 'animating' elements are changed, the 2nd SVG is rendered on top of a copy of the first SVG's rendering to form a complete composite. This prevents all the non-moving elements for having to re-render, unless of course the target graphics width/height changes. This is giving huge performance gains.

## Can I use SVG.NET in a UWP Windows 10 App?

(from [#219](https://github.com/svg-net/SVG/issues/219), by @jonthysell)

The current version of SVG.NET uses SkiaSharp for rendering, which is compatible with most modern platforms including UWP (via SkiaSharp's UWP support).

## How to render an SVG image to a single-color bitmap image?

(from [#366](https://github.com/svg-net/SVG/issues/366), by @UweKeim)

I was able to find a solution with the following fragment:

```csharp
var svgDoc = SvgDocument.Open<SvgDocument>(svgFilePath, null);

// Recursively change all nodes.
processNodes(svgDoc.Descendants(), new SvgColourServer(SKColors.DarkGreen));

var bitmap = svgDoc.Draw();
```

together with this function:

```csharp
private void processNodes(IEnumerable<SvgElement> nodes, SvgPaintServer colorServer)
{
    foreach (var node in nodes)
    {
        if (node.Fill != SvgPaintServer.None) node.Fill = colorServer;
        if (node.Color != SvgPaintServer.None) node.Color = colorServer;
        if (node.StopColor != SvgPaintServer.None) node.StopColor = colorServer;
        if (node.Stroke != SvgPaintServer.None) node.Stroke = colorServer;

        processNodes(node.Descendants(), colorServer);
    }
}
```

## How to render only a specific SvgElement?

(from [#403](https://github.com/svg-net/SVG/issues/403), by @ievgennaida)

Use `element.RenderElement();`.

## How to render an SVG document to a bitmap in another size?

Use `SvgDocument.Draw(int rasterWidth, int rasterHeight)`. If one of the values is 0, it is set to preserve the aspect ratio, if both values are given, the aspect ratio is ignored.

## Is this code server-safe?

(from [#381](https://github.com/svg-net/SVG/issues/381), by @rangercej, answered by @gvheertum)

Yes. Since migration to SkiaSharp, the library no longer relies on Windows-only GDI+ (System.Drawing) calls. SkiaSharp is a cross-platform 2D graphics API for .NET based on Google's Skia Graphics Library. It is highly performant and designed for server-side environments, avoiding the many resource-heavy and thread-safety issues associated with GDI+ in non-interactive scenarios.

## How to change the SvgUnit DPI?

(from [#313](https://github.com/svg-net/SVG/issues/313), by @KieranSmartMP)

`SvgUnit` takes the DPI (which is called `Ppi` here) from the document. This is set to the system DPI at creation time, but can be set to another value afterward, e.g. 
```c#
  doc = SvgDocument();
  doc.Ppi = 200;
  ...
```

## Why does my application crash with "cannot allocate the required memory"?

(from [#250](https://github.com/svg-net/SVG/issues/250), by @Radzhab)

If you try to open a very large SVG file in your application, it may crash if it exceeds available memory or .NET's object size limits. SkiaSharp handles large bitmaps more efficiently than GDI+, but hardware and OS limits still apply.

## How to add a custom attribute to an SVG element?

(from [#481](https://github.com/svg-net/SVG/issues/481), by @lroye)

Custom attributes are publicly accessible as a collection, you can add an attribute like this:
```C#
    element.CustomAttributes[attributeName] = attributeValue;
```

## I'm getting a SvgSkiaSharpCannotBeLoadedException if running under Linux or MacOs

This happens if the native SkiaSharp libraries are not found. SkiaSharp requires native assets for each platform. Ensure you have installed the appropriate `SkiaSharp.NativeAssets.*` package for your target platform (e.g., `SkiaSharp.NativeAssets.Linux` or `SkiaSharp.NativeAssets.macOS`).

### Validating SkiaSharp capabilities

If you want to make sure the executing system is capable of using SkiaSharp features, you can use one of the functions available in the `SvgDocument` class.

If you only want to get a boolean telling whether the capabilities are available, please use the following code:
```csharp
bool hasSkiaCapabilities = SvgDocument.SystemIsSkiaSharpCapable();
```

If you want to ensure the capabilities and let an error be thrown when these are not available, please use the following code:
```csharp
SvgDocument.EnsureSystemIsSkiaSharpCapable();
```
This function will throw a `SvgSkiaSharpCannotBeLoadedException` if the native libraries are not available.
