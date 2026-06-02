namespace MinBlazor.Cli;

public enum PipelineStep
{
    Prebuild     = 0,
    Shadow       = 1,
    Compile      = 2,
    AfterCompile = 3,
    Scaffold     = 4,
    Restore      = 5,
    Build        = 6,
    Serve        = 7,
}
