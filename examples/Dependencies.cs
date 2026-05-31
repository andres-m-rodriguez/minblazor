using Microsoft.Extensions.DependencyInjection;

namespace MinBlazorApp;

public interface IGreeter
{
    string Greet(string name);
}

public sealed class Greeter : IGreeter
{
    public string Greet(string name) => $"Hello, {name}!";
}

public static class Dependencies
{
    public static void Configure(IServiceCollection services) =>
        services.AddSingleton<IGreeter, Greeter>();
}
