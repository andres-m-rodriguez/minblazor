using System.Diagnostics;
using MinBlazor.Core;
using MinBlazor.Parser;
using MinBlazor.Scaffold;

namespace MinBlazor.Cli.Steps;

public sealed class ShadowStep : IPipelineStep
{
    private const string ShadowFileName = "minblazor.csproj";

    public PipelineStep Step => PipelineStep.Shadow;

    public Result Execute(PipelineContext context)
    {
        var sourceDir = Path.GetDirectoryName(context.RazorPath)!;
        var shadowPath = Path.Combine(sourceDir, ShadowFileName);

        var entryDoc = new RazorParser(File.ReadAllText(context.RazorPath)).Parse();
        var packages = new Scanner().Packages(entryDoc);

        var content = ScaffoldGenerator.GenerateShadowProject(AppInfo.BlazorPackageVersion, packages);

        var changed = !File.Exists(shadowPath) ||
                      File.ReadAllText(shadowPath) != content.ToString();
        if (!changed)
            return Result.Ok();

        File.WriteAllText(shadowPath, content.ToString());

        return Restore(sourceDir, context.Diagnostics);
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
                ArgumentList = { "restore", ShadowFileName, "--nologo" },
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
