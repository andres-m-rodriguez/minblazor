using MinBlazor.Build;
using MinBlazor.Compiler;
using MinBlazor.Parser;

namespace MinBlazor.Scaffold;

public sealed class ScaffoldGenerator
{
    private readonly Emitter _emitter = new();

    public static (ReadOnlyMemory<char> Csproj, ReadOnlyMemory<char> Imports) GenerateShadowProject(
        string blazorPackageVersion,
        IEnumerable<PackageReference> packages,
        IReadOnlyList<string> dllRefs,
        bool hasBuildCs,
        IReadOnlyList<string> sourceDirectories
    )
    {
        var pkgList = packages.ToList();

        var packageRefs = string.Concat(
            pkgList.Select(p =>
                p.Version is null
                    ? $"\n    <PackageReference Include=\"{p.Name}\" />"
                    : $"\n    <PackageReference Include=\"{p.Name}\" Version=\"{p.Version}\" />"
            )
        );

        var dllRefItems = string.Concat(
            dllRefs.Select(dll =>
                $"\n    <Reference Include=\"{Path.GetFileNameWithoutExtension(dll)}\">"
                + $"\n      <HintPath>refs/{Path.GetFileName(dll)}</HintPath>"
                + $"\n    </Reference>"
            )
        );

        var buildCsItem = hasBuildCs ? "\n    <Compile Include=\"../Build.cs\" />" : "";

        var sourceDirItems = string.Concat(
            sourceDirectories.SelectMany(dir =>
                new[]
                {
                    $"\n    <Compile Include=\"{dir.Replace('\\', '/')}/**/*.cs\" />",
                    $"\n    <Content Include=\"{dir.Replace('\\', '/')}/**/*.razor\" />",
                }
            )
        );

        var csproj = $"""
            <Project Sdk="Microsoft.NET.Sdk.Razor">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <RootNamespace>MinBlazorApp</RootNamespace>
                <BaseIntermediateOutputPath>obj\</BaseIntermediateOutputPath>
                <BaseOutputPath>bin\</BaseOutputPath>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
              </PropertyGroup>
              <ItemGroup>
                <Content Include="../**/*.razor" />{buildCsItem}{sourceDirItems}
              </ItemGroup>
              <ItemGroup>{dllRefItems}
                <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="{blazorPackageVersion}" />{packageRefs}
              </ItemGroup>
            </Project>
            """;

        var imports = """
            @using System.Net.Http
            @using System.Net.Http.Json
            @using Microsoft.AspNetCore.Components.Forms
            @using Microsoft.AspNetCore.Components.Routing
            @using Microsoft.AspNetCore.Components.Web
            @using Microsoft.AspNetCore.Components.WebAssembly.Http
            @using Microsoft.JSInterop
            @using MinBlazorApp
            """;

        return (csproj.AsMemory(), imports.AsMemory());
    }

    public ScaffoldContent Generate(
        Compilation compilation,
        BuildOutputs? build,
        ScaffoldOptions options
    )
    {
        var entries = new List<ScaffoldEntry>();
        var headTags = new List<string>();

        EmitComponent(entries, headTags, compilation.Entry);
        foreach (var component in compilation.Components)
            EmitComponent(entries, headTags, component);

        if (build is not null)
        {
            headTags.AddRange(build.HeadTags);

            foreach (var source in build.Sources)
                entries.Add(new SourceFile(source.FileName, source.Code.AsMemory()));

            if (build.Options.Count > 0)
                entries.Add(
                    new SourceFile(
                        "BuildOptions.cs",
                        Templates.BuildOptions(build.Options).AsMemory()
                    )
                );

            foreach (var asset in build.Assets)
                entries.Add(new StaticAsset(asset.Path, asset.Contents));
        }

        var properties = build?.Properties ?? new Dictionary<string, string>();
        var head = string.Join('\n', headTags.OrderBy(t => t, StringComparer.Ordinal));

        entries.Add(
            new ProjectFile(
                Templates
                    .Csproj(options.BlazorPackageVersion, compilation.Packages, properties)
                    .AsMemory()
            )
        );
        entries.Add(
            new SourceFile(
                "Program.cs",
                Templates.Program(compilation.Entry.Name, options.HasDependencies).AsMemory()
            )
        );
        entries.Add(
            new SourceFile("_Imports.razor", Templates.Imports(options.PackageUsings).AsMemory())
        );
        var hotReloadScript = options.HotReloadPort is int port
            ? Templates.HotReloadScript(port)
            : null;
        entries.Add(new HostPage(Templates.IndexHtml(head, hotReloadScript).AsMemory()));

        return new ScaffoldContent(entries);
    }

    private void EmitComponent(
        List<ScaffoldEntry> entries,
        List<string> headTags,
        CompiledComponent component
    )
    {
        entries.Add(
            new ComponentFile(component.Name, _emitter.Emit(component.Document).AsMemory())
        );
        headTags.AddRange(_emitter.EmitHostTags(component.HostTags));
    }
}
