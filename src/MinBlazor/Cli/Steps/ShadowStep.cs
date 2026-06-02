using System.Diagnostics;
using MinBlazor.Build;
using MinBlazor.Core;
using MinBlazor.Parser;
using MinBlazor.Scaffold;

namespace MinBlazor.Cli.Steps;

public sealed class ShadowStep : IPipelineStep
{
    private const string ShadowDir = "_minblazor";
    private const string ShadowCsproj = "minblazor.csproj";
    private const string ShadowImports = "_Imports.razor";
    private const string RefsDir = "refs";
    private const string BuildFile = "Build.cs";

    public PipelineStep Step => PipelineStep.Shadow;

    public Result Execute(PipelineContext context)
    {
        var sourceDir = Path.GetDirectoryName(context.RazorPath)!;
        var shadowFolder = Path.Combine(sourceDir, ShadowDir);
        var refsFolder = Path.Combine(shadowFolder, RefsDir);
        var csprojPath = Path.Combine(shadowFolder, ShadowCsproj);
        var importsPath = Path.Combine(shadowFolder, ShadowImports);

        var entryDoc = new RazorParser(File.ReadAllText(context.RazorPath)).Parse();
        var packages = new Scanner().Packages(entryDoc);

        var dllPaths = GetMinBlazorDlls();
        var hasBuildCs = File.Exists(Path.Combine(sourceDir, BuildFile));
        var sourceDirectories = context.Script?.Outputs.SourceDirectories
            ?? (IReadOnlyList<string>)[];

        var (csproj, imports) = ScaffoldGenerator.GenerateShadowProject(
            AppInfo.BlazorPackageVersion, packages, dllPaths, hasBuildCs, sourceDirectories);

        var csprojStr = csproj.ToString();
        var importsStr = imports.ToString();

        var csprojChanged = !File.Exists(csprojPath) || File.ReadAllText(csprojPath) != csprojStr;
        var importsChanged = !File.Exists(importsPath) || File.ReadAllText(importsPath) != importsStr;

        if (!csprojChanged && !importsChanged && DllsUpToDate(dllPaths, refsFolder))
            return Result.Ok();

        Directory.CreateDirectory(shadowFolder);
        Directory.CreateDirectory(refsFolder);
        AddToGitignore(sourceDir);

        CopyDlls(dllPaths, refsFolder);

        if (csprojChanged)
            File.WriteAllText(csprojPath, csprojStr);
        if (importsChanged)
            File.WriteAllText(importsPath, importsStr);

        return csprojChanged ? Restore(shadowFolder, context.Diagnostics) : Result.Ok();
    }

    private static IReadOnlyList<string> GetMinBlazorDlls() =>
    [
        typeof(BuildContext).Assembly.Location,
        typeof(MinBlazor.Core.Result).Assembly.Location,
        typeof(RazorParser).Assembly.Location,
    ];

    private static bool DllsUpToDate(IReadOnlyList<string> sources, string refsFolder)
    {
        foreach (var src in sources)
        {
            var dest = Path.Combine(refsFolder, Path.GetFileName(src));
            if (!File.Exists(dest)) return false;
            if (File.GetLastWriteTimeUtc(src) > File.GetLastWriteTimeUtc(dest)) return false;
        }
        return true;
    }

    private static void CopyDlls(IReadOnlyList<string> sources, string refsFolder)
    {
        foreach (var src in sources)
        {
            var dest = Path.Combine(refsFolder, Path.GetFileName(src));
            if (!File.Exists(dest) || File.GetLastWriteTimeUtc(src) > File.GetLastWriteTimeUtc(dest))
                File.Copy(src, dest, overwrite: true);
        }
    }

    private static void AddToGitignore(string sourceDir)
    {
        var gitignorePath = Path.Combine(sourceDir, ".gitignore");
        if (File.Exists(gitignorePath))
        {
            var existing = File.ReadAllText(gitignorePath);
            if (existing.Contains(ShadowDir)) return;
            File.AppendAllText(gitignorePath, $"\n{ShadowDir}/\n");
        }
        else
        {
            File.WriteAllText(gitignorePath, $"{ShadowDir}/\n");
        }
    }

    private static Result Restore(string workingDir, IDiagnostics diagnostics)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                ArgumentList = { "restore", ShadowCsproj, "--nologo" },
            },
        };

        process.OutputDataReceived += (_, e) => { if (e.Data is not null) diagnostics.Info(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) diagnostics.Info(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        return process.ExitCode == 0
            ? Result.Ok()
            : Result.Fail($"dotnet restore failed for shadow project (exit code {process.ExitCode}).");
    }
}
