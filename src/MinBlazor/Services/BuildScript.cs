using System.Collections;
using System.Reflection;
using MinBlazor.Build;
using MinBlazor.Build.Models;
using MinBlazor.Core;
using MinBlazor.Parser;

namespace MinBlazor.Services;

public sealed class BuildScript
{
    public const string FileName = "Build.cs";

    private readonly BuildOutputs _outputs = new();
    private readonly MethodInfo? _beforeCompile;
    private readonly MethodInfo? _afterCompile;
    private readonly string _sourceDir;
    private readonly string _outputDir;
    private readonly Action<string> _log;
    private readonly IReadOnlyDictionary<string, string> _environment;

    internal BuildScript(
        MethodInfo? before,
        MethodInfo? after,
        string sourceDir,
        string outputDir,
        Action<string> log
    )
    {
        _beforeCompile = before;
        _afterCompile = after;
        _sourceDir = sourceDir;
        _outputDir = outputDir;
        _log = log;
        _environment = ReadEnvironment();
    }

    public BuildOutputs Outputs => _outputs;

    public Result RunBeforeCompile()
    {
        if (_beforeCompile is null)
            return Result.Ok();

        var context = new BeforeCompileContext(
            _outputs,
            _sourceDir,
            _outputDir,
            _environment,
            _log
        );
        return Invoke(_beforeCompile, context);
    }

    public Result RunAfterCompile(CompilationInfo compilation)
    {
        if (_afterCompile is null)
            return Result.Ok();

        var context = new AfterCompileContext(
            _outputs,
            _sourceDir,
            _outputDir,
            _environment,
            compilation,
            _log
        );
        return Invoke(_afterCompile, context);
    }

    private Result Invoke(MethodInfo method, BuildContext context)
    {
        try
        {
            if (method.Invoke(null, [context]) is Task task)
                task.GetAwaiter().GetResult();
        }
        catch (TargetInvocationException ex)
        {
            return Result.Fail(
                $"{FileName} {method.Name} failed: {ex.InnerException?.Message ?? ex.Message}"
            );
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
}
