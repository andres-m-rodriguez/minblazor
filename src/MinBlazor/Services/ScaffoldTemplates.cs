using Microsoft.CodeAnalysis.CSharp;
using MinBlazor.Parser;

namespace MinBlazor.Services;

internal static class ScaffoldTemplates
{
    public const string RootNamespace = "MinBlazorApp";

    public const string HeadPlaceholder = "<!--minblazor:head-->";

    public static string Csproj(string packageVersion, IEnumerable<PackageReference> packages, IReadOnlyDictionary<string, string> properties)
    {
        var userPackages = string.Concat(
            packages.Select(package =>
                package.Version is null
                    ? $"\n    <PackageReference Include=\"{package.Name}\" />"
                    : $"\n    <PackageReference Include=\"{package.Name}\" Version=\"{package.Version}\" />"));

        var userProperties = string.Concat(
            properties
                .OrderBy(property => property.Key, StringComparer.Ordinal)
                .Select(property => $"\n    <{property.Key}>{property.Value}</{property.Key}>"));

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

    public static string BuildOptions(IReadOnlyDictionary<string, string> options)
    {
        var consts = string.Concat(
            options
                .OrderBy(option => option.Key, StringComparer.Ordinal)
                .Select(option => $"    public const string {option.Key} = {SymbolDisplay.FormatLiteral(option.Value, true)};\n"));

        return $"namespace {RootNamespace};\n\npublic static class BuildOptions\n{{\n{consts}}}\n";
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

    public static string LaunchSettings(int port) =>
        $$"""
            {
              "profiles": {
                "minblazor": {
                  "commandName": "Project",
                  "launchBrowser": false,
                  "applicationUrl": "http://localhost:{{port}}",
                  "environmentVariables": {
                    "ASPNETCORE_ENVIRONMENT": "Development"
                  }
                }
              }
            }
            """;

    private static readonly string[] BaseUsings =
    [
        "System.Net.Http",
        "System.Net.Http.Json",
        "Microsoft.AspNetCore.Components.Forms",
        "Microsoft.AspNetCore.Components.Routing",
        "Microsoft.AspNetCore.Components.Web",
        "Microsoft.AspNetCore.Components.WebAssembly.Http",
        "Microsoft.JSInterop",
        RootNamespace,
    ];

    public static string Imports(IEnumerable<string> folderNamespaces)
    {
        var usings = BaseUsings.Concat(folderNamespaces).Distinct().Select(ns => $"@using {ns}");

        return string.Join('\n', usings) + "\n";
    }

    public const string IndexHtml = """
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
            <!--minblazor:head-->
        </head>

        <body>
            <div id="app"><span class="mb-loading">Loading...</span></div>

            <div id="blazor-error-ui">
                An unhandled error has occurred.
                <a href="" class="reload">Reload</a>
            </div>

            <script src="_framework/blazor.webassembly.js"></script>
        </body>

        </html>
        """;
}
