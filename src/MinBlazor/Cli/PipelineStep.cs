namespace MinBlazor.Cli;

public enum PipelineStep
{
    Prebuild = 0,
    Shadow   = 1,
    Compile  = 2,
    Scaffold = 3,
    Restore  = 4,
    Build    = 5,
    Serve    = 6,
}
