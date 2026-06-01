using Microsoft.Extensions.DependencyInjection;
using MinBlazor.Lsp;
using OmniSharp.Extensions.LanguageServer.Server;

var server = await LanguageServer.From(options =>
    options
        .WithInput(Console.OpenStandardInput())
        .WithOutput(Console.OpenStandardOutput())
        .WithServices(services => services.AddSingleton<DocumentStore>())
        .WithHandler<RazorTextSync>()
        .WithHandler<ComponentCompletionHandler>());

await server.WaitForExit;
