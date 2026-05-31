using System.Text;
using MinBlazor.Build.Models;
using MinBlazor.Razor.Models;

namespace MinBlazor.Build;

public sealed class BuildContext
{
    private readonly BuildOutputs _outputs;
    private readonly Action<string> _log;
    private readonly bool _allowComponents;

    internal BuildContext(
        BuildOutputs outputs,
        string sourceDirectory,
        string outputDirectory,
        IReadOnlyDictionary<string, string> environment,
        CompilationInfo? compilation,
        bool allowComponents,
        Action<string> log
    )
    {
        _outputs = outputs;
        _log = log;
        _allowComponents = allowComponents;
        SourceDirectory = sourceDirectory;
        OutputDirectory = outputDirectory;
        Environment = environment;
        Compilation = compilation;
    }

    public string SourceDirectory { get; }

    public string OutputDirectory { get; }

    public IReadOnlyDictionary<string, string> Environment { get; }

    public CompilationInfo? Compilation { get; }

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

    public void AddOption(string name, string value) => _outputs.Options[name] = value;

    public void AddComponent(string name, string razorSource)
    {
        if (!_allowComponents)
        {
            _log($"AddComponent('{name}') is only available in BeforeCompile; ignored.");
            return;
        }

        _outputs.Components.Add(new VirtualComponent(name, razorSource));
    }

    public void Log(string message) => _log(message);
}
