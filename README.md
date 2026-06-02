# minblazor

Run a single `.razor` file as a Blazor WebAssembly app — no project, no ceremony.

```
minblazor run Index.razor
```

> **Status: experimental / work in progress.** Not published to NuGet — build from source (see below).

## Quick example — MudBlazor counter

Three files, no `.csproj`:

**`Index.razor`**
```razor
#:package MudBlazor@7.10.0

<link @hostTag href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
<script @hostTag src="_content/MudBlazor/MudBlazor.min.js"></script>

<MudThemeProvider />

<MudStack AlignItems="AlignItems.Center" Class="mt-8">
    <MudText Typo="Typo.h4">Count: @count</MudText>
    <MudButton Variant="Variant.Filled" Color="Color.Primary" OnClick="Increment">
        Click me
    </MudButton>
</MudStack>

@code {
    int count = 0;
    void Increment() => count++;
}
```

**`Dependencies.cs`**
```csharp
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

public static class Dependencies
{
    public static void Configure(IServiceCollection services)
        => services.AddMudServices();
}
```

**`Build.cs`** *(optional — build-time hooks)*
```csharp
using MinBlazor.Build;

public static class Build
{
    public static void AfterCompile(AfterCompileContext ctx)
    {
        ctx.Log($"compiled {ctx.Compilation.Components.Count} components");
    }
}
```

Then run:
```
minblazor run Index.razor
```

The app opens at `http://localhost:5005`. No `.csproj`, no `bin/`, no `obj/` in your folder.

## What it does

`minblazor run Index.razor` compiles your `.razor` plus any sibling components into a
throwaway Blazor WebAssembly project, builds it, and serves it. The scaffold lives in a
temp cache (`%TEMP%/minblazor/<hash>/`) so your folder stays clean. Warm runs skip
unchanged files so MSBuild short-circuits.

**Dialect features:**

| Feature | Example |
|---|---|
| NuGet packages | `#:package MudBlazor@7.10.0` |
| Hoist to `<head>` | `<link @hostTag href="..." />` |
| Sibling components | Just drop `Card.razor` next to `Index.razor` |
| DI registration | `Dependencies.cs` → `Configure(IServiceCollection)` |
| Build hooks | `Build.cs` → `BeforeCompile` / `AfterCompile` |

## Commands

```
minblazor run Index.razor        # build, serve, and watch
minblazor build Index.razor      # build only
minblazor restore Index.razor    # restore packages
minblazor clean [Index.razor]    # delete cache (omit file = clear all)
minblazor list                   # show cached scaffolds
```

## Build.cs hooks

`Build.cs` runs on your machine at build time — it never ships to the browser.

```csharp
using MinBlazor.Build;

public static class Build
{
    // Runs before compilation — add virtual components, static assets, options
    public static void BeforeCompile(BeforeCompileContext ctx)
    {
        ctx.AddComponent("Banner", "<h1>Hello from Build.cs</h1>");
        ctx.AddStaticAsset("data/config.json", """{ "theme": "dark" }""");
        ctx.AddOption("ApiUrl", ctx.Environment.GetValueOrDefault("API_URL") ?? "https://localhost");
    }

    // Runs after compilation — inspect which components were resolved
    public static void AfterCompile(AfterCompileContext ctx)
    {
        ctx.Log($"resolved {ctx.Compilation.Components.Count} components");
    }
}
```

`ctx.AddOption("Key", "value")` generates a `BuildOptions` constants class compiled into the app:
```csharp
// accessible in any component:
@BuildOptions.ApiUrl
```

## Requirements

- .NET 10 SDK
- `wasm-tools` workload: `dotnet workload install wasm-tools`

## Install from source

```
dotnet pack src/MinBlazor/MinBlazor.csproj -c Release
dotnet tool install -g --add-source ./nupkg MinBlazor
```
