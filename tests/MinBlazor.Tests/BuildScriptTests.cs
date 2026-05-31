using MinBlazor.Build.Models;
using MinBlazor.Services;

namespace MinBlazor.Tests;

public class BuildScriptTests
{
    private const string Script = """
        using MinBlazor.Build;

        public static class Build
        {
            public static void BeforeCompile(BuildContext ctx)
            {
                ctx.AddOption("Greeting", "hi");
                ctx.AddComponent("Generated", "<p>generated</p>");
                ctx.AddPackage("Humanizer", "2.14.1");
            }

            public static void AfterCompile(BuildContext ctx)
            {
                ctx.AddHeadTag("<meta name=\"x\" content=\"y\" />");
                ctx.Log($"components: {ctx.Compilation!.Components.Count}");
            }
        }
        """;

    [Test]
    public async Task RunsBothHooksAndCollectsOutputs()
    {
        var dir = NewDir();
        File.WriteAllText(Path.Combine(dir, BuildScript.FileName), Script);

        var loaded = BuildScript.Load(dir, dir, _ => { });
        await Assert.That(loaded.IsSuccess).IsTrue();
        var script = loaded.Value!;
        await Assert.That(script).IsNotNull();

        await Assert.That(script.RunBeforeCompile().IsSuccess).IsTrue();
        await Assert.That(script.Outputs.Options["Greeting"]).IsEqualTo("hi");
        await Assert.That(script.Outputs.Components.Single().Name).IsEqualTo("Generated");
        await Assert.That(script.Outputs.Packages.Single().Name).IsEqualTo("Humanizer");

        var info = new CompilationInfo("Index", ["Index", "Generated"], []);
        await Assert.That(script.RunAfterCompile(info).IsSuccess).IsTrue();
        await Assert.That(script.Outputs.HeadTags.Single()).Contains("<meta");
    }

    [Test]
    public async Task NoBuildFileLoadsToNull()
    {
        var loaded = BuildScript.Load(NewDir(), NewDir(), _ => { });

        await Assert.That(loaded.IsSuccess).IsTrue();
        await Assert.That(loaded.Value).IsNull();
    }

    [Test]
    public async Task CompileErrorFails()
    {
        var dir = NewDir();
        File.WriteAllText(Path.Combine(dir, BuildScript.FileName), "this is not valid c#");

        var loaded = BuildScript.Load(dir, dir, _ => { });

        await Assert.That(loaded.IsSuccess).IsFalse();
        await Assert.That(loaded.Error).Contains(BuildScript.FileName);
    }

    [Test]
    public async Task AddComponentInAfterCompileIsIgnoredWithWarning()
    {
        var dir = NewDir();
        File.WriteAllText(Path.Combine(dir, BuildScript.FileName), """
            using MinBlazor.Build;

            public static class Build
            {
                public static void AfterCompile(BuildContext ctx) => ctx.AddComponent("Late", "<p/>");
            }
            """);

        var logs = new List<string>();
        var script = BuildScript.Load(dir, dir, logs.Add).Value!;

        await Assert.That(script.RunAfterCompile(new CompilationInfo("Index", ["Index"], [])).IsSuccess).IsTrue();
        await Assert.That(script.Outputs.Components).IsEmpty();
        await Assert.That(logs.Any(line => line.Contains("BeforeCompile"))).IsTrue();
    }

    private static string NewDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "minblazor-tests", Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(dir);
        return dir;
    }
}
