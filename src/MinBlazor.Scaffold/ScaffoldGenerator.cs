using MinBlazor.Build;
using MinBlazor.Compiler;
using MinBlazor.Parser;

namespace MinBlazor.Scaffold;

public sealed class ScaffoldGenerator
{
    private readonly Emitter _emitter = new();

    public ScaffoldContent Generate(
        Compilation compilation,
        BuildOutputs? build,
        ScaffoldOptions options)
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
                entries.Add(new SourceFile("BuildOptions.cs", Templates.BuildOptions(build.Options).AsMemory()));

            foreach (var asset in build.Assets)
                entries.Add(new StaticAsset(asset.Path, asset.Contents));
        }

        var properties = build?.Properties ?? new Dictionary<string, string>();
        var head = string.Join('\n', headTags.OrderBy(t => t, StringComparer.Ordinal));

        entries.Add(new ProjectFile(Templates.Csproj(options.BlazorPackageVersion, compilation.Packages, properties).AsMemory()));
        entries.Add(new SourceFile("Program.cs", Templates.Program(compilation.Entry.Name, options.HasDependencies).AsMemory()));
        entries.Add(new SourceFile("_Imports.razor", Templates.Imports(options.PackageUsings).AsMemory()));
        entries.Add(new HostPage(Templates.IndexHtml(head).AsMemory()));

        return new ScaffoldContent(entries);
    }

    private void EmitComponent(List<ScaffoldEntry> entries, List<string> headTags, CompiledComponent component)
    {
        entries.Add(new ComponentFile(component.Name, _emitter.Emit(component.Document).AsMemory()));
        headTags.AddRange(_emitter.EmitHostTags(component.HostTags));
    }
}
