using System.IO;
using System.Text;
using MinBlazor.Build.Models;
using MinBlazor.Razor.Models;

namespace MinBlazor.Build;

public abstract class BuildContext
{
    private readonly BuildOutputs _outputs;
    private readonly Action<string> _log;

    internal BuildContext(
        BuildOutputs outputs,
        string sourceDirectory,
        string outputDirectory,
        IReadOnlyDictionary<string, string> environment,
        Action<string> log
    )
    {
        _outputs = outputs;
        _log = log;
        SourceDirectory = sourceDirectory;
        OutputDirectory = outputDirectory;
        Environment = environment;
    }

    public string SourceDirectory { get; }
    public string OutputDirectory { get; }
    public IReadOnlyDictionary<string, string> Environment { get; }

    public void AddPackage(string name, string? version = null) =>
        _outputs.Packages.Add(new PackageReference(name, version));

    public void AddProperty(string name, string value) => _outputs.Properties[name] = value;

    public void AddHeadTag(string html) => _outputs.HeadTags.Add(html);

    public void AddStaticAsset(string path, string contents) =>
        _outputs.Assets.Add(new BuildAsset(path, Encoding.UTF8.GetBytes(contents)));

    public void AddStaticAsset(string path, byte[] contents) =>
        _outputs.Assets.Add(new BuildAsset(path, contents));

    public void AddSource(string fileName, string code) =>
        _outputs.Sources.Add(new BuildSource(fileName, code));

    public void AddSourceDirectory(string path)
    {
        var directory = Path.IsPathRooted(path) ? path : Path.Combine(SourceDirectory, path);

        _outputs.SourceDirectories.Add(directory);

        foreach (
            var file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories)
        )
            _outputs.Sources.Add(new BuildSource(Path.GetFileName(file), File.ReadAllText(file)));
    }

    public void AddOption(string name, string value) => _outputs.Options[name] = value;

    public void Log(string message) => _log(message);
}

public sealed class BeforeCompileContext : BuildContext
{
    private readonly BuildOutputs _outputs;

    internal BeforeCompileContext(
        BuildOutputs outputs,
        string sourceDirectory,
        string outputDirectory,
        IReadOnlyDictionary<string, string> environment,
        Action<string> log
    )
        : base(outputs, sourceDirectory, outputDirectory, environment, log)
    {
        _outputs = outputs;
    }

    public void AddComponent(string name, string razorSource) =>
        _outputs.Components.Add(new VirtualComponent(name, razorSource));
}

public sealed class AfterCompileContext : BuildContext
{
    internal AfterCompileContext(
        BuildOutputs outputs,
        string sourceDirectory,
        string outputDirectory,
        IReadOnlyDictionary<string, string> environment,
        CompilationInfo compilation,
        Action<string> log
    )
        : base(outputs, sourceDirectory, outputDirectory, environment, log)
    {
        Compilation = compilation;
    }

    public CompilationInfo Compilation { get; }
}
