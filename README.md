# minblazor

Run a single `.razor` file as a Blazor WebAssembly app.

```
minblazor run Index.razor
```

Requires the .NET 10 SDK and the `wasm-tools` workload.

## Install

```
dotnet pack src/MinBlazor/MinBlazor.csproj -c Release
dotnet tool install -g --add-source ./nupkg MinBlazor
```
