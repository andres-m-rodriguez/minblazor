namespace MinBlazor.Cli;

public enum PipelineStep
{
    Prebuild = 0,
    Shadow = 1,
    Compile = 2,
    Scaffold = 3,
    Build = 4,
    Serve = 5,
}
