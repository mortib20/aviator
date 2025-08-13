using Aviator.Airframe.Config;
using Aviator.Airframe.Frames;
using Aviator.Airframe.Frames.Strategies;
using Aviator.Airframe.Frames.Strategies.AeroL.Jaero.Protocol;
using Aviator.Airframe.Frames.Strategies.Hfdl.DumpHfdl.Protocol;
using Aviator.Airframe.Frames.Strategies.Vdl2.DumpVdl2.Protocol;
using Aviator.Airframe.Metrics;
using Aviator.Airframe.Network;
using Aviator.Airframe.Network.Implementation;
using Aviator.Airframe.SignalR;
using Aviator.Global.DependencyInjection;
using Aviator.Network.Input;
using Aviator.Network.Output;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.DependencyInjection;

public static class AirframeExtension
{
    public static WebApplicationBuilder AddAirframeExtension(this WebApplicationBuilder builder)
    {
        var acarsConfig = builder.Configuration.GetSection(AirframeConfig.Section).Get<AirframeConfig>();
        ArgumentNullException.ThrowIfNull(acarsConfig);
        
        // Inject input manager
        builder.Services.AddSingleton<IAirframeInputManager, AirframeInputManager>(sp =>
        {
            var inputBuilder = sp.GetRequiredService<InputBuilder>();
            
            var logger = sp.GetRequiredService<ILogger<AirframeInputManager>>();
            var input = inputBuilder.Create(acarsConfig.Input);
            
            return new AirframeInputManager(logger, input);
        });

        builder.Services.AddSingleton<IAirframeOutputManager, AirframeOutputManager>(sp =>
        {
            var outputsDictionary = CreateOutputDictionary(sp, acarsConfig.Outputs);
            return new AirframeOutputManager(sp.GetRequiredService<ILogger<AirframeOutputManager>>(), outputsDictionary);
        });
        
        // Inject all decoder specific protocol strategies
        builder.Services.AddAllImplementations<IDumpVdl2ProtocolStrategy>(ServiceLifetime.Singleton);
        builder.Services.AddAllImplementations<IJaeroProtocolStrategy>(ServiceLifetime.Singleton);
        builder.Services.AddAllImplementations<IDumpHfdlProtocolStrategy>(ServiceLifetime.Singleton);
        
        // Inject all decoder strategies
        builder.Services.AddAllImplementations<IDecoderStrategy>(ServiceLifetime.Singleton);

        builder.Services.AddSingleton<AirframeHub>();
        builder.Services.AddSingleton<AirframeMetrics>();
        
        builder.Services.AddSingleton<AirframeHandler>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<AirframeHandler>>();
            var decoderStrategies = sp.GetRequiredService<List<IDecoderStrategy>>();
            var outputManager = sp.GetRequiredService<IAirframeOutputManager>();
            var airframeMetrics = sp.GetRequiredService<AirframeMetrics>();
            var airframeHub = sp.GetRequiredService<IHubContext<AirframeHub>>();
            return new AirframeHandler(logger, decoderStrategies, outputManager, airframeMetrics, airframeHub);
        });
        
        builder.Services.AddHostedService<AirframeService>();
        
        return builder;
    }
    
    private static Dictionary<FrameType, List<IOutput>> CreateOutputDictionary(IServiceProvider s, List<OutputEndpointConfig> acarsConfig)
    {
        var outputBuilder = s.GetRequiredService<OutputBuilder>();
        var outputsTuple = acarsConfig
            .Select(a => (a.Types, outputBuilder.Create(a.Protocol, a.Host, a.Port))).ToList();

        var frameTypes = Enum.GetValues<FrameType>().ToList();

        var outputDictionary = frameTypes
            .ToDictionary<FrameType, FrameType, List<IOutput>>(
                frameType => frameType,
                frameType => outputsTuple.Where(b => b.Types.Contains(frameType)).Select(o => o.Item2).ToList()
                );

        var logger = s.GetRequiredService<ILogger<FrameType>>();
        
        foreach (var (key, value) in outputDictionary)
        {
            if (value.Count == 0)
            {
                logger.LogInformation("{Type} disabled, no output set.", key);
                continue;
            }
            logger.LogInformation("Sending {Type} to {Outputs}.", key, string.Join(", ", value.Select(output => output.EndPoint)));
        }
        
        return outputDictionary;
    }
}