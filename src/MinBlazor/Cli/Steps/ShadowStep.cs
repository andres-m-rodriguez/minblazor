using System.Diagnostics;
using MinBlazor.Core;
using MinBlazor.Parser;

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

        var content = Generate(packages);

        var changed = !File.Exists(shadowPath) || File.ReadAllText(shadowPath) != content;
        if (!changed)
            return Result.Ok();

        File.WriteAllText(shadowPath, content);

        return Restore(sourceDir, context.Diagnostics);
    }

    private static string Generate(IReadOnlyList<PackageReference> packages)
    {
        var packageRefs = string.Concat(packages.Select(p =>
            p.Version is null
                ? $"\n    <PackageReference Include=\"{p.Name}\" />"
                : $"\n    <PackageReference Include=\"{p.Name}\" Version=\"{p.Version}\" />"));

        return $"""
            <Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <RootNamespace>MinBlazorApp</RootNamespace>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="{AppInfo.BlazorPackageVersion}" />{packageRefs}
              </ItemGroup>
            </Project>
            """;
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
