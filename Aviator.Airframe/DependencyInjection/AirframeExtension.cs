using Aviator.Airframe.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aviator.Airframe.DependencyInjection;

public static class AirframeExtension
{
    public static WebApplicationBuilder AddAirframeExtension(this WebApplicationBuilder builder)
    {
        var acarsConfig = builder.Configuration.GetSection(AcarsConfig.Section).Get<AcarsConfig>();
        ArgumentNullException.ThrowIfNull(acarsConfig);
        
        builder.Services.AddHostedService<AirframeService>();
        
        return builder;
    }
}