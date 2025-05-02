using Microsoft.Extensions.DependencyInjection;

namespace Aviator.Global.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddAllImplementations<TInterface>(this IServiceCollection services)
    {
        var interfaceType = typeof(TInterface);

        var implementations = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t));

        foreach (var impl in implementations)
        {
            services.AddSingleton(interfaceType, impl); // oder AddTransient/AddScoped
        }
    }
}