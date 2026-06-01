namespace MinBlazor.Cli;

public enum PipelineStep
{
    Prebuild = 0,
    Compile  = 1,
    Scaffold = 2,
    Build    = 3,
    Serve    = 4,
}
