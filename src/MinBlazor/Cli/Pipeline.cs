using System.Security.Cryptography;
using System.Text;
using MinBlazor.Build.Models;
using MinBlazor.Models;
using MinBlazor.Razor;
using MinBlazor.Razor.Models;
using MinBlazor.Services;

namespace MinBlazor.Cli;

public sealed record Compiled(Compilation Compilation, BuildScript? Script, IReadOnlyList<Diagnostic> Diagnostics);

public sealed class Pipeline
{
    private readonly IOutput _output;

    public Pipeline(IOutput output) => _output = output;

    // Indexes the folder, runs Build.cs BeforeCompile, and compiles the entry and its
    // dependency closure. No scaffold is written and dotnet is not invoked.
    public Result<Compiled> Compile(string razorPath, string scaffoldDir)
    {
        var sourceDir = Path.GetDirectoryName(razorPath)!;

        var entrySource = File.ReadAllText(razorPath);
        var entryName = ComponentName.From(Path.GetFileNameWithoutExtension(razorPath));

        var registry = new ComponentRegistry();
        var indexed = FolderIndexer.Index(sourceDir, registry);
        if (!indexed.IsSuccess)
            return Result<Compiled>.Fail(indexed.Error!);

        var scriptResult = BuildScript.Load(sourceDir, scaffoldDir, _output.Info);
        if (!scriptResult.IsSuccess)
            return Result<Compiled>.Fail(scriptResult.Error!);

        var script = scriptResult.Value;

        if (script is not null)
        {
            var before = script.RunBeforeCompile();
            if (!before.IsSuccess)
                return Result<Compiled>.Fail(before.Error!);

            foreach (var component in script.Outputs.Components)
            {
                var added = registry.Add(component.Name, component.Source);
                if (!added.IsSuccess)
                    return Result<Compiled>.Fail(added.Error!);
            }
        }

        var diagnostics = new Diagnostics();
        var compilation = new Compiler(registry, diagnostics).Compile(entryName, entrySource);

        return Result<Compiled>.Ok(new Compiled(compilation, script, diagnostics.Items));
    }

    // Compiles, runs Build.cs AfterCompile, and writes the scaffold project. Returns the
    // scaffold directory, ready to build or serve.
    public Result<string> Prepare(string razorPath, bool clean)
    {
        var scaffoldDir = CacheDirectory(razorPath);

        if (clean && Directory.Exists(scaffoldDir))
        {
            _output.Info("Cleaning cache");
            Directory.Delete(scaffoldDir, recursive: true);
        }

        var compiled = Compile(razorPath, scaffoldDir);
        if (!compiled.IsSuccess)
            return Result<string>.Fail(compiled.Error!);

        var result = compiled.Value!;

        foreach (var diagnostic in result.Diagnostics)
            _output.Info($"{diagnostic.Severity}: {diagnostic.Message}");

        if (result.Script is not null)
        {
            var after = result.Script.RunAfterCompile(BuildInfo(result.Compilation));
            if (!after.IsSuccess)
                return Result<string>.Fail(after.Error!);

            var duplicate = result
                .Script.Outputs.Sources.GroupBy(source => source.FileName, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group => group.Count() > 1);

            if (duplicate is not null)
                return Result<string>.Fail($"Two build source files are named '{duplicate.Key}'. Source file names must be unique.");
        }

        var sourceDir = Path.GetDirectoryName(razorPath)!;
        new Scaffold().Write(scaffoldDir, sourceDir, result.Compilation, AppInfo.DefaultPort, result.Script?.Outputs);

        return Result<string>.Ok(scaffoldDir);
    }

    public static string CacheDirectory(string razorPath)
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
