using Microsoft.CodeAnalysis.CSharp;
using MinBlazor.Parser;

namespace MinBlazor.Scaffold;

internal static class Templates
{
    public const string RootNamespace = "MinBlazorApp";
    public const string HeadPlaceholder = "<!--minblazor:head-->";

    public static string Csproj(
        string packageVersion,
        IEnumerable<PackageReference> packages,
        IReadOnlyDictionary<string, string> properties
    )
    {
        var userPackages = string.Concat(
            packages.Select(p =>
                p.Version is null
                    ? $"\n    <PackageReference Include=\"{p.Name}\" />"
                    : $"\n    <PackageReference Include=\"{p.Name}\" Version=\"{p.Version}\" />"
            )
        );

        var userProperties = string.Concat(
            properties
                .OrderBy(p => p.Key, StringComparer.Ordinal)
                .Select(p => $"\n    <{p.Key}>{p.Value}</{p.Key}>")
        );

        return $"""
            <Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">

              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <Nullable>enable</Nullable>
                <ImplicitUsings>enable</ImplicitUsings>
                <RootNamespace>{RootNamespace}</RootNamespace>
                <NoWarn>$(NoWarn);CS1998</NoWarn>{userProperties}
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="{packageVersion}" />{userPackages}
              </ItemGroup>

            </Project>
            """;
    }

    public static string Program(string componentName, bool hasDependencies)
    {
        var usings = hasDependencies ? $"\nusing {RootNamespace};" : "";
        var configure = hasDependencies ? "\nDependencies.Configure(builder.Services);" : "";

        return $$"""
            using Microsoft.AspNetCore.Components.Web;
            using Microsoft.AspNetCore.Components.WebAssembly.Hosting;{{usings}}

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<global::{{RootNamespace}}.{{componentName}}>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");{{configure}}
            await builder.Build().RunAsync();
            """;
    }

    public static string Imports(IEnumerable<string> packageUsings)
    {
        var all = new[]
        {
            "System.Net.Http",
            "System.Net.Http.Json",
            "Microsoft.AspNetCore.Components.Forms",
            "Microsoft.AspNetCore.Components.Routing",
            "Microsoft.AspNetCore.Components.Web",
            "Microsoft.AspNetCore.Components.WebAssembly.Http",
            "Microsoft.JSInterop",
            RootNamespace,
        }
            .Concat(packageUsings)
            .Distinct();

        return string.Join('\n', all.Select(ns => $"@using {ns}")) + "\n";
    }

    public static string BuildOptions(IReadOnlyDictionary<string, string> options)
    {
        var consts = string.Concat(
            options
                .OrderBy(o => o.Key, StringComparer.Ordinal)
                .Select(o =>
                    $"    public const string {o.Key} = {SymbolDisplay.FormatLiteral(o.Value, true)};\n"
                )
        );

        return $"namespace {RootNamespace};\n\npublic static class BuildOptions\n{{\n{consts}}}\n";
    }

    public static string HotReloadScript(int port) =>
        $$"""
            <script>
            const _mbws = new WebSocket('ws://localhost:{{port}}/_minblazor/ws');
            _mbws.onmessage = e => {
              const msg = JSON.parse(e.data);
              if (msg.type === 'rebuild-started' || msg.type === 'build-failed') {
                let el = document.getElementById('__mb_hrl__');
                if (!el) {
                  el = document.createElement('div');
                  el.id = '__mb_hrl__';
                  el.style.cssText = 'position:fixed;bottom:1rem;right:1rem;background:#18181b;color:#fff;padding:.35rem .75rem;border-radius:6px;font:13px/1.5 monospace;z-index:99999;box-shadow:0 2px 8px rgba(0,0,0,.4);';
                  document.body.appendChild(el);
                }
                el.style.background = msg.type === 'build-failed' ? '#b91c1c' : '#18181b';
                el.textContent = msg.type === 'build-failed' ? 'Compiler error' : 'Rebuilding...';
                return;
              }
              if (msg.type === 'reload') { location.reload(); return; }
              if (msg.type === 'css') {
                let s = document.getElementById('__mb_live__');
                if (!s) { s = document.createElement('style'); s.id = '__mb_live__'; document.head.appendChild(s); }
                s.textContent = msg.content;
              }
            };
            _mbws.onclose = () => setTimeout(() => location.reload(), 500);
            </script>
            """;

    public static string IndexHtml(string head, string? hotReloadScript = null) =>
        $$"""
            <!DOCTYPE html>
            <html lang="en">

            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>minblazor</title>
                <base href="/" />
                <style>
                    html, body { font-family: system-ui, sans-serif; margin: 0; }
                    #app { padding: 1rem 1.5rem; }
                    .mb-loading { color: #888; }
                    #blazor-error-ui {
                        display: none; position: fixed; bottom: 0; left: 0; right: 0;
                        background: #b32121; color: #fff; padding: .6rem 1rem; z-index: 1000;
                    }
                    #blazor-error-ui .reload { color: #fff; text-decoration: underline; }
                </style>
                {{head}}
            </head>

            <body>
                <div id="app"><span class="mb-loading">Loading...</span></div>

                <div id="blazor-error-ui">
                    An unhandled error has occurred.
                    <a href="" class="reload">Reload</a>
                </div>

                <script src="_framework/blazor.webassembly.js"></script>
                {{hotReloadScript}}
            </body>

            </html>
            """;
}
