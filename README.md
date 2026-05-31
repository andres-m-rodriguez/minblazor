# minblazor

Run a single `.razor` file as a Blazor WebAssembly app — no project, no ceremony.

```
minblazor run Index.razor
```

> **Status: experimental / work in progress.** Not published to NuGet — you build and
> install it from source (see below).

## What it does

`minblazor run Index.razor` compiles your `.razor` — plus any sibling components it
references — into a throwaway Blazor WebAssembly project, builds it, and serves it with
a dev server. The generated project lives in a temp cache keyed by the file path
(`<temp>/minblazor/<hash>/`), so your folder stays clean and repeat runs reuse the
build. An unchanged re-run rewrites nothing, so MSBuild short-circuits and it starts
in seconds.

It understands a small superset of Razor:

- **Components** — referenced components are resolved from sibling `.razor` files,
  transitively (the whole reachable set is scaffolded).
- **`#:package Name@Version`** — adds a NuGet `<PackageReference>` to the generated
  project, the same way .NET 10 file-based apps do it.
- **`<script @hostTag …>` / `<link @hostTag …>`** — hoisted out of the component into
  the host page's `<head>` (the `@hostTag` marker is stripped). Handy for CDN scripts
  and stylesheets.
- **`Dependencies.cs`** — if a sibling `Dependencies.cs` is present, its
  `Dependencies.Configure(IServiceCollection)` is wired into the app's DI container, so
  you can register your own services (e.g. `services.AddMudServices()`).

## Requirements

- .NET 10 SDK
- the `wasm-tools` workload for the Blazor WebAssembly build —
  `dotnet workload install wasm-tools`

## Build from source

Not on NuGet yet — build and install the tool locally:

```
dotnet pack src/MinBlazor/MinBlazor.csproj -c Release
dotnet tool install -g --add-source ./nupkg MinBlazor
```

Or run it straight from the repo without installing:

```
dotnet run --project src/MinBlazor -- run path/to/Index.razor
```

## Layout

- `src/MinBlazor.Razor/` — the Razor-dialect front-end (lexer → parser → transformer →
  emitter → compiler) as a reusable, I/O-free library.
- `src/MinBlazor/` — the CLI: resolves the component closure, scaffolds the project,
  and runs the dev server.
- `tests/` — TUnit tests for the library.
