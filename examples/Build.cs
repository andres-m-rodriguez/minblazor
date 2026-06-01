using MinBlazor.Build;

public static class Build
{
    public static void BeforeCompile(BeforeCompileContext ctx)
    {
        ctx.AddOption("BuildStamp", "demo");
        ctx.AddStaticAsset("build/info.txt", "generated at build time");
        ctx.AddSourceDirectory("Models");
    }

    public static void AfterCompile(AfterCompileContext ctx)
    {
        ctx.AddHeadTag("<meta name=\"generator\" content=\"minblazor\" />");
        ctx.Log($"build hooks ran for {ctx.Compilation.Components.Count} components");
    }
}
