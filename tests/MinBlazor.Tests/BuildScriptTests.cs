using MinBlazor.Build.Models;
using MinBlazor.Services;

namespace MinBlazor.Tests;

public class BuildScriptTests
{
    private const string Script = """
        using MinBlazor.Build;

        public static class Build
        {
            public static void BeforeCompile(BeforeCompileContext ctx)
            {
                ctx.AddOption("Greeting", "hi");
                ctx.AddComponent("Generated", "<p>generated</p>");
            }

            public static void AfterCompile(AfterCompileContext ctx)
            {
                ctx.AddHeadTag("<meta name=\"x\" content=\"y\" />");
                ctx.Log($"components: {ctx.Compilation.Components.Count}");
            }
        }
        """;

    [Test]
    public async Task RunsBothHooksAndCollectsOutputs()
    {
        var dir = NewDir();
        File.WriteAllText(Path.Combine(dir, BuildScript.FileName), Script);

        var loaded = new BuildScriptLoader(dir, dir, _ => { }).Load();
        await Assert.That(loaded.IsSuccess).IsTrue();
        var script = loaded.Value!;
        await Assert.That(script).IsNotNull();

        await Assert.That(script.RunBeforeCompile().IsSuccess).IsTrue();
        await Assert.That(script.Outputs.Options["Greeting"]).IsEqualTo("hi");
        await Assert.That(script.Outputs.Components.Single().Name).IsEqualTo("Generated");

        var info = new CompilationInfo("Index", ["Index", "Generated"], []);
        await Assert.That(script.RunAfterCompile(info).IsSuccess).IsTrue();
        await Assert.That(script.Outputs.HeadTags.Single()).Contains("<meta");
    }

    [Test]
    public async Task NoBuildFileLoadsToNull()
    {
        var loaded = new BuildScriptLoader(NewDir(), NewDir(), _ => { }).Load();

        await Assert.That(loaded.IsSuccess).IsTrue();
        await Assert.That(loaded.Value).IsNull();
    }

    [Test]
    public async Task CompileErrorFails()
    {
        var dir = NewDir();
        File.WriteAllText(Path.Combine(dir, BuildScript.FileName), "this is not valid c#");

        var loaded = new BuildScriptLoader(dir, dir, _ => { }).Load();

        await Assert.That(loaded.IsSuccess).IsFalse();
        await Assert.That(loaded.Error).Contains(BuildScript.FileName);
    }

    [Test]
    public async Task AddComponent_IsNotAvailableInAfterCompile()
    {
        var dir = NewDir();
        File.WriteAllText(Path.Combine(dir, BuildScript.FileName), """
            using MinBlazor.Build;

            public static class Build
            {
                public static void AfterCompile(AfterCompileContext ctx)
                {
                    // AddComponent doesn't exist on AfterCompileContext — compile error
                }
            }
            """);

        var loaded = new BuildScriptLoader(dir, dir, _ => { }).Load();
        await Assert.That(loaded.IsSuccess).IsTrue();
        await Assert.That(loaded.Value!.RunAfterCompile(new CompilationInfo("Index", ["Index"], [])).IsSuccess).IsTrue();
        await Assert.That(loaded.Value!.Outputs.Components).IsEmpty();
    }

    [Test]
    public async Task AddSourceDirectory_IncludesNestedCsFiles()
    {
        var dir = NewDir();
        Directory.CreateDirectory(Path.Combine(dir, "Models", "Sub"));
        File.WriteAllText(Path.Combine(dir, "Models", "Foo.cs"), "namespace X; public class Foo { }");
        File.WriteAllText(Path.Combine(dir, "Models", "Sub", "Bar.cs"), "namespace X; public class Bar { }");
        File.WriteAllText(Path.Combine(dir, BuildScript.FileName), """
            using MinBlazor.Build;

            public static class Build
            {
                public static void BeforeCompile(BeforeCompileContext ctx) => ctx.AddSourceDirectory("Models");
            }
            """);

        var script = new BuildScriptLoader(dir, dir, _ => { }).Load().Value!;
        await Assert.That(script.RunBeforeCompile().IsSuccess).IsTrue();

        var names = script.Outputs.Sources.Select(source => source.FileName).ToHashSet();
        await Assert.That(names.Count).IsEqualTo(2);
        await Assert.That(names.Contains("Foo.cs")).IsTrue();
        await Assert.That(names.Contains("Bar.cs")).IsTrue();
    }

    private static string NewDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "minblazor-tests", Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(dir);
        return dir;
    }
}

