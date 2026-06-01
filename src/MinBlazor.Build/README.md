# MinBlazor.Build

Build-time hooks for a minblazor app — the `build.zig` of minblazor.

Drop a **`Build.cs`** next to your entry `.razor`. minblazor compiles it and runs it
**on your machine at build time**. It is never shipped to the WebAssembly app — it just
shapes how the app is built.

## The two hooks

```csharp
using MinBlazor.Build;

public static class Build
{
    // Runs before the .razor is compiled. No component model yet.
    public static void BeforeCompile(BeforeCompileContext ctx) { }

    // Runs after compilation. ctx.Compilation is available (read-only).
    public static void AfterCompile(AfterCompileContext ctx) { }
}
```

Both are optional — define either, both, or skip `Build.cs` entirely.

## Example

```csharp
using MinBlazor.Build;

public static class Build
{
    public static void BeforeCompile(BuildContext ctx)
    {
        // surface a build-time value to the running app
        ctx.AddOption("ApiBaseUrl", ctx.Environment.GetValueOrDefault("API_URL") ?? "https://localhost:5001");

        // generate a component the app can use as <Banner />
        ctx.AddComponent("Banner", "<header>built with minblazor</header>");

        // write a file into wwwroot
        ctx.AddStaticAsset("data/config.json", """{ "theme": "dark" }""");
    }

    public static void AfterCompile(BuildContext ctx)
    {
        ctx.Log($"compiled {ctx.Compilation!.Components.Count} components");

        if (ctx.Compilation.Uses("MudThemeProvider"))
            ctx.AddHeadTag("""<link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />""");
    }
}
```

## What `ctx` gives you

**Read:** `SourceDirectory`, `OutputDirectory`, `Environment`, and `Compilation`
(null in `BeforeCompile`; in `AfterCompile` it has `EntryName`, `Components`, `Packages`,
and `Uses(name)`).

**Write:**

| Method | Effect |
| --- | --- |
| `AddProperty(name, value)` | add an MSBuild property to the csproj |
| `AddHeadTag(html)` | inject into the host page `<head>` |
| `AddStaticAsset(path, string\|bytes)` | write a `wwwroot/...` file |
| `AddSource(file, code)` | add a `.cs` file compiled into the app |
| `AddSourceDirectory(path)` | include every `.cs` under a folder (recursive) into the app |
| `AddOption(name, value)` | see below |
| `AddComponent(name, razor)` | register a virtual component (`BeforeCompile` only) |
| `Log(message)` | print to the minblazor console |

## Build → runtime options

`ctx.AddOption("ApiBaseUrl", "...")` generates a constants class compiled into the app:

```csharp
namespace MinBlazorApp;

public static class BuildOptions
{
    public const string ApiBaseUrl = "https://localhost:5001";
}
```

Read it from any component — `@BuildOptions.ApiBaseUrl`. Values are strings.
Keep them deterministic: a value that changes every run (e.g. a timestamp) rewrites the
generated file each time and forces a rebuild.

## Notes

- `Build.cs` runs with full desktop .NET (filesystem, environment) and is **not** part
  of the app.
- A compile error in `Build.cs`, or a hook that throws, aborts the run.
- `AddSourceDirectory(path)` resolves `path` against `SourceDirectory` (absolute paths
  allowed) and pulls in every `.cs` under it, recursively, by filename — so the names must
  be unique. Reference the brought-in types from a component with `@using Your.Namespace`.
