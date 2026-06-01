using System.Security.Cryptography;
using System.Text;
using MinBlazor.Build.Models;
using MinBlazor.Index;
using MinBlazor.Models;
using MinBlazor.Razor;
using MinBlazor.Razor.Models;
using MinBlazor.Services;

namespace MinBlazor.Cli;

public sealed record Compiled(
    Compilation Compilation,
    BuildScript? Script,
    IReadOnlyList<Diagnostic> Diagnostics
);

internal sealed record ResolvedRegistry(ComponentRegistry Registry, BuildScript? Script);

public sealed class Pipeline(IOutput output)
{
    // Indexes the folder and runs Build.cs BeforeCompile, then compiles the entry and its
    // dependency closure. No scaffold is written and dotnet is not invoked.
    public Result<Compiled> Compile(string razorPath, string scaffoldDir)
    {
        var resolved = Resolve(razorPath, scaffoldDir);
        if (!resolved.IsSuccess)
            return Result<Compiled>.Fail(resolved.Error!);

        var entryName = ComponentName.From(Path.GetFileNameWithoutExtension(razorPath));
        var entrySource = File.ReadAllText(razorPath);

        var diagnostics = new Diagnostics();
        var compilation = new Compiler(resolved.Value!.Registry, diagnostics).Compile(
            entryName,
            entrySource
        );

        return Result<Compiled>.Ok(
            new Compiled(compilation, resolved.Value!.Script, diagnostics.Items)
        );
    }

    // Runs Build.cs BeforeCompile, scans the folder and package assemblies, and returns a
    // populated ComponentTable. No razor parsing or BFS — this is the pre-compilation step
    // that builds the full component index before the compiler runs.
    public Result<ComponentTable> Prebuild(string razorPath)
    {
        var sourceDir = Path.GetDirectoryName(razorPath)!;
        var scaffoldDir = CacheDirectory(razorPath);

        var table = new ComponentTable();

        foreach (var (name, _) in new FolderSourceProvider(sourceDir).GetComponents())
            table.Add(new IndexedComponent(name, ComponentKind.Source, Namespace: null));

        var scriptResult = new BuildScriptLoader(sourceDir, scaffoldDir, output.Info).Load();
        if (!scriptResult.IsSuccess)
            return Result<ComponentTable>.Fail(scriptResult.Error!);

        var script = scriptResult.Value;

        if (script is not null)
        {
            var before = script.RunBeforeCompile();
            if (!before.IsSuccess)
                return Result<ComponentTable>.Fail(before.Error!);

            foreach (var dir in script.Outputs.SourceDirectories)
            foreach (var (name, _) in new FolderSourceProvider(dir).GetComponents())
                table.Add(new IndexedComponent(name, ComponentKind.Source, Namespace: null));

            foreach (var component in script.Outputs.Components)
                table.Add(
                    new IndexedComponent(component.Name, ComponentKind.Virtual, Namespace: null)
                );
        }

        var binDir = FindBuildOutput(sourceDir);
        if (binDir is not null)
        {
            var packageNames = FolderPackages(sourceDir)
                .Concat(script?.Outputs.Packages.Select(p => p.Name) ?? [])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var scanner = new AssemblyScanner(new BinDirectoryAssemblyProvider(binDir));
            foreach (var component in scanner.Scan(packageNames))
                table.Add(component);
        }

        return Result<ComponentTable>.Ok(table);
    }

    // Every component a file can use: the .razor in its folder, Build.cs virtual components,
    // and the components from any #:package (scanned from the scaffold build output). Used by
    // the language server for component completion.
    public Result<IReadOnlyList<string>> AvailableComponents(string razorPath)
    {
        var scaffoldDir = CacheDirectory(razorPath);

        var resolved = Resolve(razorPath, scaffoldDir);
        if (!resolved.IsSuccess)
            return Result<IReadOnlyList<string>>.Fail(resolved.Error!);

        var names = new SortedSet<string>(resolved.Value!.Registry.Names, StringComparer.Ordinal);

        var sourceDir = Path.GetDirectoryName(razorPath)!;
        var binDir = FindBuildOutput(sourceDir);
        if (binDir is not null)
            foreach (
                var component in PackageComponents
                    .Scan(binDir, FolderPackages(sourceDir))
                    .Components
            )
                names.Add(component);

        return Result<IReadOnlyList<string>>.Ok(names.ToList());
    }

    // The package assemblies live in a built scaffold's bin. The file being edited may not be
    // the entry that was built, so accept any built scaffold belonging to this folder.
    private static string? FindBuildOutput(string sourceDir)
    {
        foreach (
            var file in Directory.EnumerateFiles(sourceDir, "*.razor", SearchOption.AllDirectories)
        )
        {
            var binDir = Path.Combine(CacheDirectory(file), "bin", "Debug", "net10.0");
            if (Directory.Exists(binDir))
                return binDir;
        }

        return null;
    }

    // Union of #:package directives across every .razor in the folder.
    private static IReadOnlyCollection<string> FolderPackages(string sourceDir)
    {
        var packages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var scanner = new Scanner();

        foreach (var file in Directory.EnumerateFiles(sourceDir, "*.razor", SearchOption.AllDirectories))
        {
            var document = new Parser(new Lexer(File.ReadAllText(file))).Parse();
            foreach (var package in scanner.Packages(document))
                packages.Add(package.Name);
        }

        return packages;
    }

    private Result<ResolvedRegistry> Resolve(string razorPath, string scaffoldDir)
    {
        var sourceDir = Path.GetDirectoryName(razorPath)!;

        var registry = new ComponentRegistry();
        var indexed = FolderIndexer.Index(sourceDir, registry);
        if (!indexed.IsSuccess)
            return Result<ResolvedRegistry>.Fail(indexed.Error!);

        var scriptResult = new BuildScriptLoader(sourceDir, scaffoldDir, output.Info).Load();
        if (!scriptResult.IsSuccess)
            return Result<ResolvedRegistry>.Fail(scriptResult.Error!);

        var script = scriptResult.Value;

        if (script is not null)
        {
            var before = script.RunBeforeCompile();
            if (!before.IsSuccess)
                return Result<ResolvedRegistry>.Fail(before.Error!);

            foreach (var component in script.Outputs.Components)
            {
                var added = registry.Add(component.Name, component.Source);
                if (!added.IsSuccess)
                    return Result<ResolvedRegistry>.Fail(added.Error!);
            }
        }

        return Result<ResolvedRegistry>.Ok(new ResolvedRegistry(registry, script));
    }

    // Compiles, runs Build.cs AfterCompile, and writes the scaffold project. Returns the
    // scaffold directory, ready to build or serve.
    public Result<string> Prepare(string razorPath, bool clean)
    {
        var scaffoldDir = CacheDirectory(razorPath);

        if (clean && Directory.Exists(scaffoldDir))
        {
            output.Info("Cleaning cache");
            Directory.Delete(scaffoldDir, recursive: true);
        }

        var compiled = Compile(razorPath, scaffoldDir);
        if (!compiled.IsSuccess)
            return Result<string>.Fail(compiled.Error!);

        var result = compiled.Value!;

        foreach (var diagnostic in result.Diagnostics)
            output.Info($"{diagnostic.Severity}: {diagnostic.Message}");

        if (result.Script is not null)
        {
            var after = result.Script.RunAfterCompile(BuildInfo(result.Compilation));
            if (!after.IsSuccess)
                return Result<string>.Fail(after.Error!);

            var duplicate = result
                .Script.Outputs.Sources.GroupBy(
                    source => source.FileName,
                    StringComparer.OrdinalIgnoreCase
                )
                .FirstOrDefault(group => group.Count() > 1);

            if (duplicate is not null)
                return Result<string>.Fail(
                    $"Two build source files are named '{duplicate.Key}'. Source file names must be unique."
                );
        }

        var sourceDir = Path.GetDirectoryName(razorPath)!;
        var binDir = Path.Combine(scaffoldDir, "bin", "Debug", "net10.0");
        var usings = Directory.Exists(binDir)
            ? PackageComponents.Scan(binDir, PackageNames(result)).Namespaces
            : (IReadOnlyList<string>)[];

        new Scaffold().Write(
            scaffoldDir,
            sourceDir,
            result.Compilation,
            AppInfo.DefaultPort,
            result.Script?.Outputs,
            usings
        );

        BuildTable(sourceDir, binDir, result);

        return Result<string>.Ok(scaffoldDir);
    }

    private static ComponentTable BuildTable(string sourceDir, string binDir, Compiled result)
    {
        var table = new ComponentTable();

        foreach (var (name, _) in new FolderSourceProvider(sourceDir).GetComponents())
            table.Add(new IndexedComponent(name, ComponentKind.Source, Namespace: null));

        if (result.Script is not null)
            foreach (var component in result.Script.Outputs.Components)
                table.Add(
                    new IndexedComponent(component.Name, ComponentKind.Virtual, Namespace: null)
                );

        if (Directory.Exists(binDir))
        {
            var scanner = new AssemblyScanner(new BinDirectoryAssemblyProvider(binDir));
            foreach (var component in scanner.Scan(PackageNames(result)))
                table.Add(component);
        }

        return table;
    }

    // Every package the app references: #:package directives plus Build.cs AddPackage.
    private static IReadOnlyList<string> PackageNames(Compiled result)
    {
        var names = result.Compilation.Packages.Select(package => package.Name);
        if (result.Script is not null)
            names = names.Concat(result.Script.Outputs.Packages.Select(package => package.Name));

        return names.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    public static string CacheDirectory(string razorPath)
    {
        var path = Path.GetFullPath(razorPath);
        if (OperatingSystem.IsWindows())
            path = path.ToLowerInvariant();

        var hash = Convert
            .ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(path)))[..16]
            .ToLowerInvariant();
        return Path.Combine(Path.GetTempPath(), "minblazor", hash);
    }

    private static CompilationInfo BuildInfo(Compilation compilation)
    {
        var components = new List<string> { compilation.Entry.Name };
        components.AddRange(compilation.Components.Select(component => component.Name));
        return new CompilationInfo(compilation.Entry.Name, components, compilation.Packages);
    }
}

