using MinBlazor.Core;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using MinBlazor.Build;
using MinBlazor.Build.Models;
using MinBlazor.Models;
using MinBlazor.Parser;

namespace MinBlazor.Services;

public sealed class BuildScriptLoader(string sourceDir, string outputDir, Action<string> log)
{
    private const string TypeName = "Build";

    private readonly string _sourceDir = sourceDir;
    private readonly string _outputDir = outputDir;
    private readonly Action<string> _log = log;

    public Result<BuildScript?> Load()
    {
        var path = Path.Combine(_sourceDir, BuildScript.FileName);
        if (!File.Exists(path))
            return Result<BuildScript?>.Ok(null);

        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(path), path: path);
        var compilation = CSharpCompilation.Create(
            "MinBlazorBuildScript",
            [tree],
            References(),
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable
            )
        );

        using var stream = new MemoryStream();
        var emit = compilation.Emit(stream);
        if (!emit.Success)
            return Result<BuildScript?>.Fail(FormatErrors(path, emit));

        var assembly = Assembly.Load(stream.ToArray());
        var buildType = assembly.GetTypes().FirstOrDefault(t => t.Name == TypeName);
        if (buildType is null)
            return Result<BuildScript?>.Fail(
                $"{BuildScript.FileName} must define a static class named '{TypeName}'."
            );

        var before = buildType.GetMethod(
            "BeforeCompile",
            BindingFlags.Public | BindingFlags.Static
        );
        var after = buildType.GetMethod("AfterCompile", BindingFlags.Public | BindingFlags.Static);
        if (before is null && after is null)
            return Result<BuildScript?>.Fail(
                $"{BuildScript.FileName}: '{TypeName}' has no BeforeCompile or AfterCompile method."
            );

        return Result<BuildScript?>.Ok(
            new BuildScript(before, after, _sourceDir, _outputDir, _log)
        );
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

        paths.Add(typeof(BuildContext).Assembly.Location);
        paths.Add(typeof(PackageReference).Assembly.Location);

        return paths.Select(p => (MetadataReference)MetadataReference.CreateFromFile(p)).ToList();
    }

    private static string FormatErrors(string path, EmitResult emit)
    {
        var builder = new StringBuilder($"Failed to compile {Path.GetFileName(path)}:");
        foreach (
            var diagnostic in emit.Diagnostics.Where(d =>
                d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error
            )
        )
            builder.Append("\n  ").Append(diagnostic);

        return builder.ToString();
    }
}

