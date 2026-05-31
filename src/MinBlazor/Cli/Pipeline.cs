using System.Security.Cryptography;
using System.Text;
using MinBlazor.Build.Models;
using MinBlazor.Models;
using MinBlazor.Razor;
using MinBlazor.Razor.Models;
using MinBlazor.Services;

namespace MinBlazor.Cli;

public sealed class Pipeline
{
    private readonly IOutput _output;

    public Pipeline(IOutput output) => _output = output;

    // Compiles the entry and its dependency closure, runs Build.cs, and writes the
    // scaffold project. Returns the scaffold directory, ready to build or serve.
    public Result<string> Prepare(string razorPath, bool clean)
    {
        var sourceDir = Path.GetDirectoryName(razorPath)!;
        var scaffoldDir = CacheDirectory(razorPath);

        if (clean && Directory.Exists(scaffoldDir))
        {
            _output.Info("Cleaning cache");
            Directory.Delete(scaffoldDir, recursive: true);
        }

        var entrySource = File.ReadAllText(razorPath);
        var entryName = ComponentName.From(Path.GetFileNameWithoutExtension(razorPath));

        var registry = new ComponentRegistry();
        var indexed = FolderIndexer.Index(sourceDir, registry);
        if (!indexed.IsSuccess)
            return Result<string>.Fail(indexed.Error!);

        var scriptResult = BuildScript.Load(sourceDir, scaffoldDir, _output.Info);
        if (!scriptResult.IsSuccess)
            return Result<string>.Fail(scriptResult.Error!);

        var script = scriptResult.Value;

        if (script is not null)
        {
            var before = script.RunBeforeCompile();
            if (!before.IsSuccess)
                return Result<string>.Fail(before.Error!);

            foreach (var component in script.Outputs.Components)
            {
                var added = registry.Add(component.Name, component.Source);
                if (!added.IsSuccess)
                    return Result<string>.Fail(added.Error!);
            }
        }

        var diagnostics = new Diagnostics();
        var compilation = new Compiler(registry, diagnostics).Compile(entryName, entrySource);

        foreach (var diagnostic in diagnostics.Items)
            _output.Info($"{diagnostic.Severity}: {diagnostic.Message}");

        if (script is not null)
        {
            var after = script.RunAfterCompile(BuildInfo(compilation));
            if (!after.IsSuccess)
                return Result<string>.Fail(after.Error!);

            var duplicate = script
                .Outputs.Sources.GroupBy(source => source.FileName, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group => group.Count() > 1);

            if (duplicate is not null)
                return Result<string>.Fail($"Two build source files are named '{duplicate.Key}'. Source file names must be unique.");
        }

        new Scaffold().Write(scaffoldDir, sourceDir, compilation, AppInfo.DefaultPort, script?.Outputs);

        return Result<string>.Ok(scaffoldDir);
    }

    private static string CacheDirectory(string razorPath)
    {
        var path = Path.GetFullPath(razorPath);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(path)))[..16].ToLowerInvariant();
        return Path.Combine(Path.GetTempPath(), "minblazor", hash);
    }

    private static CompilationInfo BuildInfo(Compilation compilation)
    {
        var components = new List<string> { compilation.Entry.Name };
        components.AddRange(compilation.Components.Select(component => component.Name));
        return new CompilationInfo(compilation.Entry.Name, components, compilation.Packages);
    }
}
