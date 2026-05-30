namespace MinBlazor.Services;

internal static class ScaffoldTemplates
{
    public const string RootNamespace = "MinBlazorApp";

    public static string Csproj(string packageVersion) => $"""
        <Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">

          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
            <RootNamespace>{RootNamespace}</RootNamespace>
            <NoWarn>$(NoWarn);CS1998</NoWarn>
          </PropertyGroup>

          <ItemGroup>
            <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="{packageVersion}" />
            <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="{packageVersion}" PrivateAssets="all" />
          </ItemGroup>

        </Project>
        """;

    public static string Program(string componentName) => $$"""
        using Microsoft.AspNetCore.Components.Web;
        using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<global::{{RootNamespace}}.{{componentName}}>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");
        await builder.Build().RunAsync();
        """;

    public static string LaunchSettings(int port) => $$"""
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
        var usings = BaseUsings
            .Concat(folderNamespaces)
            .Distinct()
            .Select(ns => $"@using {ns}");

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
