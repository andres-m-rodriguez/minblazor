using System.Diagnostics;
using MinBlazor.Core;

namespace MinBlazor.Services;

public sealed class Builder
{
    public Result<string> Build(string projectDir, IDiagnostics? diagnostics = null)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = projectDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                ArgumentList = { "build", "-c", "Debug", "--nologo" },
            },
        };

        process.OutputDataReceived += (_, e) => { if (e.Data is not null) diagnostics?.Info(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) diagnostics?.Info(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        if (process.ExitCode != 0)
            return Result<string>.Fail($"dotnet build failed (exit code {process.ExitCode}).");

        var manifest = FindManifest(projectDir);
        return manifest is null
            ? Result<string>.Fail("Build succeeded but no static web assets manifest was found.")
            : Result<string>.Ok(manifest);
    }

    private static string? FindManifest(string projectDir)
    {
        var binDir = Path.Combine(projectDir, "bin");
        if (!Directory.Exists(binDir))
            return null;

        return Directory
            .EnumerateFiles(binDir, "*.staticwebassets.runtime.json", SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }
}
