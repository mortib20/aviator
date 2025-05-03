using Microsoft.Extensions.DependencyInjection;

namespace Aviator.Global.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddAllImplementations<TInterface>(this IServiceCollection services)
    {
        var interfaceType = typeof(TInterface);

        var implementations = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false } && interfaceType.IsAssignableFrom(t));

        foreach (var impl in implementations)
        {
            services.AddSingleton(interfaceType, impl); // oder AddTransient/AddScoped
        }

        // Adding all to a list so we can use them later
        services.AddSingleton<List<TInterface>>(sp => sp.GetServices<TInterface>().ToList());
    }
}