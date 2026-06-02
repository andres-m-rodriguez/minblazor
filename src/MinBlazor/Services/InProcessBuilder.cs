using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using MinBlazor.Compiler;
using MinBlazor.Core;
using MinBlazor.Parser;

namespace MinBlazor.Services;

public sealed class InProcessBuilder
{
    public Result<string> Build(
        string scaffoldDir,
        Compilation compilation,
        IReadOnlyDictionary<string, string> buildProperties,
        string blazorPackageVersion,
        IDiagnostics diagnostics)
    {
        var projectCollection = new Microsoft.Build.Evaluation.ProjectCollection();
        var root = ProjectRootElement.Create(projectCollection);

        // Virtual path gives MSBuild the base directory for glob resolution
        // without writing the file to disk
        root.FullPath = Path.Combine(scaffoldDir, "App.csproj");
        root.Sdk = "Microsoft.NET.Sdk.BlazorWebAssembly";

        var props = root.AddPropertyGroup();
        props.AddProperty("TargetFramework", "net10.0");
        props.AddProperty("Nullable", "enable");
        props.AddProperty("ImplicitUsings", "enable");
        props.AddProperty("RootNamespace", "MinBlazorApp");
        props.AddProperty("NoWarn", "$(NoWarn);CS1998");
        props.AddProperty("Configuration", "Debug");
        foreach (var (k, v) in buildProperties)
            props.AddProperty(k, v);

        var pkgs = root.AddItemGroup();
        pkgs.AddItem("PackageReference", "Microsoft.AspNetCore.Components.WebAssembly",
            [new KeyValuePair<string, string>("Version", blazorPackageVersion)]);
        foreach (var pkg in compilation.Packages)
            pkgs.AddItem("PackageReference", pkg.Name,
                pkg.Version is null ? [] : [new KeyValuePair<string, string>("Version", pkg.Version)]);

        var instance = new ProjectInstance(root);
        var logger = new DiagnosticsLogger(diagnostics);
        var parameters = new BuildParameters(projectCollection)
        {
            Loggers = [logger],
            EnableNodeReuse = false,
        };

        var result = BuildManager.DefaultBuildManager.Build(
            parameters,
            new BuildRequestData(instance, ["Restore", "Build"]));

        if (result.OverallResult == BuildResultCode.Failure)
            return Result<string>.Fail("MSBuild failed (see output above).");

        var manifest = FindManifest(scaffoldDir);
        return manifest is null
            ? Result<string>.Fail("Build succeeded but no static web assets manifest was found.")
            : Result<string>.Ok(manifest);
    }

    private static string? FindManifest(string scaffoldDir)
    {
        var binDir = Path.Combine(scaffoldDir, "bin");
        if (!Directory.Exists(binDir)) return null;
        return Directory
            .EnumerateFiles(binDir, "*.staticwebassets.runtime.json", SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }

    private sealed class DiagnosticsLogger(IDiagnostics diagnostics) : ILogger
    {
        public LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Minimal;
        public string? Parameters { get; set; }

        public void Initialize(IEventSource eventSource)
        {
            eventSource.MessageRaised += (_, e) => { if (e.Message is not null) diagnostics.Info(e.Message); };
            eventSource.WarningRaised += (_, e) => { if (e.Message is not null) diagnostics.Warning(e.Message); };
            eventSource.ErrorRaised += (_, e) => { if (e.Message is not null) diagnostics.Error(e.Message); };
        }

        public void Shutdown() { }
    }
}

