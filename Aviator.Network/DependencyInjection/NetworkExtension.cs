using Aviator.Network.Input;
using Aviator.Network.Output;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Aviator.Network.DependencyInjection;

public static class NetworkExtension
{
    public static WebApplicationBuilder AddNetworkUtilities(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<InputBuilder>();
        builder.Services.AddSingleton<OutputBuilder>();

        return builder;
    }
}