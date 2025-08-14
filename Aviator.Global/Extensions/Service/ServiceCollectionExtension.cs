using Microsoft.Extensions.DependencyInjection;

namespace Aviator.Global.Extensions.Service
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAllImplementations<TInterface>(this IServiceCollection services, ServiceLifetime lifetime)
        {
            var interfaceType = typeof(TInterface);
        
            var implementations = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t is { IsClass: true, IsAbstract: false } && interfaceType.IsAssignableFrom(t));

            foreach (var impl in implementations)
            {
                switch (lifetime)
                {
                    case ServiceLifetime.Singleton:
                        services.AddSingleton(interfaceType, impl);
                        break;
                    case ServiceLifetime.Scoped:
                        services.AddScoped(interfaceType, impl);
                        break;
                    case ServiceLifetime.Transient:
                        services.AddTransient(interfaceType, impl);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
                }
            }

            // Adding all to a list so we can use them later
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton<List<TInterface>>(sp => sp.GetServices<TInterface>().ToList());
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped<List<TInterface>>(sp => sp.GetServices<TInterface>().ToList());
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient<List<TInterface>>(sp => sp.GetServices<TInterface>().ToList());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }
        }
    }
}