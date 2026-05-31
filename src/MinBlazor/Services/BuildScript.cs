using System.Collections;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using MinBlazor.Build;
using MinBlazor.Build.Models;
using MinBlazor.Models;
using MinBlazor.Razor.Models;

namespace MinBlazor.Services;

public sealed class BuildScript
{
    public const string FileName = "Build.cs";
    private const string TypeName = "Build";

    private readonly BuildOutputs _outputs = new();
    private readonly MethodInfo? _beforeCompile;
    private readonly MethodInfo? _afterCompile;
    private readonly string _sourceDir;
    private readonly string _outputDir;
    private readonly Action<string> _log;
    private readonly IReadOnlyDictionary<string, string> _environment;

    private BuildScript(MethodInfo? before, MethodInfo? after, string sourceDir, string outputDir, Action<string> log)
    {
        _beforeCompile = before;
        _afterCompile = after;
        _sourceDir = sourceDir;
        _outputDir = outputDir;
        _log = log;
        _environment = ReadEnvironment();
    }

    public BuildOutputs Outputs => _outputs;

    public static Result<BuildScript?> Load(string sourceDir, string outputDir, Action<string> log)
    {
        var path = Path.Combine(sourceDir, FileName);
        if (!File.Exists(path))
            return Result<BuildScript?>.Ok(null);

        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(path), path: path);
        var compilation = CSharpCompilation.Create(
            "MinBlazorBuildScript",
            [tree],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        using var stream = new MemoryStream();
        var emit = compilation.Emit(stream);
        if (!emit.Success)
            return Result<BuildScript?>.Fail(FormatErrors(path, emit));

        var assembly = Assembly.Load(stream.ToArray());
        var buildType = assembly.GetTypes().FirstOrDefault(type => type.Name == TypeName);
        if (buildType is null)
            return Result<BuildScript?>.Fail($"{FileName} must define a static class named '{TypeName}'.");

        var before = buildType.GetMethod("BeforeCompile", BindingFlags.Public | BindingFlags.Static);
        var after = buildType.GetMethod("AfterCompile", BindingFlags.Public | BindingFlags.Static);
        if (before is null && after is null)
            return Result<BuildScript?>.Fail($"{FileName}: '{TypeName}' has no BeforeCompile or AfterCompile method.");

        return Result<BuildScript?>.Ok(new BuildScript(before, after, sourceDir, outputDir, log));
    }

    public Result RunBeforeCompile() =>
        Invoke(_beforeCompile, compilation: null, allowComponents: true);

    public Result RunAfterCompile(CompilationInfo compilation) =>
        Invoke(_afterCompile, compilation, allowComponents: false);

    private Result Invoke(MethodInfo? method, CompilationInfo? compilation, bool allowComponents)
    {
        if (method is null)
            return Result.Ok();

        var context = new BuildContext(_outputs, _sourceDir, _outputDir, _environment, compilation, allowComponents, _log);
        try
        {
            if (method.Invoke(null, [context]) is Task task)
                task.GetAwaiter().GetResult();
        }
        catch (TargetInvocationException ex)
        {
            return Result.Fail($"{FileName} {method.Name} failed: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Fail($"{FileName} {method.Name} failed: {ex.Message}");
        }

        return Result.Ok();
    }

    private static IReadOnlyDictionary<string, string> ReadEnvironment()
    {
        var environment = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (DictionaryEntry entry in System.Environment.GetEnvironmentVariables())
            if (entry.Key is string key && entry.Value is string value)
                environment[key] = value;

        return environment;
    }

    private static IReadOnlyList<MetadataReference> References()
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is string trusted)
            foreach (var path in trusted.Split(Path.PathSeparator))
                if (path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    paths.Add(path);

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
                paths.Add(assembly.Location);

        // Ensure the build API is referenceable even if not yet loaded.
        paths.Add(typeof(BuildContext).Assembly.Location);
        paths.Add(typeof(PackageReference).Assembly.Location);

        return paths.Select(path => (MetadataReference)MetadataReference.CreateFromFile(path)).ToList();
    }

    private static string FormatErrors(string path, EmitResult emit)
    {
        var builder = new StringBuilder($"Failed to compile {Path.GetFileName(path)}:");
        foreach (var diagnostic in emit.Diagnostics.Where(diagnostic => diagnostic.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error))
            builder.Append("\n  ").Append(diagnostic);

        return builder.ToString();
    }
}
