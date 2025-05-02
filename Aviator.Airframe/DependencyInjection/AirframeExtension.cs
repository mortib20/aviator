using Aviator.Airframe.Config;
using Aviator.Airframe.Frames;
using Aviator.Airframe.Frames.Strategies;
using Aviator.Airframe.Network;
using Aviator.Airframe.Network.Implementation;
using Aviator.Global.DependencyInjection;
using Aviator.Network.Input;
using Aviator.Network.Output;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.DependencyInjection;

public static class AirframeExtension
{
    public static WebApplicationBuilder AddAirframeExtension(this WebApplicationBuilder builder)
    {
        var acarsConfig = builder.Configuration.GetSection(AcarsConfig.Section).Get<AcarsConfig>();
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
        
        // Inject all decoder strategies
        builder.Services.AddAllImplementations<IDecoderStrategy>();
        
        builder.Services.AddSingleton<AirframeHandler>(sp =>
        {

            var logger = sp.GetRequiredService<ILogger<AirframeHandler>>();
            var decoderStrategies = sp.GetServices<IDecoderStrategy>().ToList();
            var outputManager = sp.GetRequiredService<IAirframeOutputManager>();
            return new AirframeHandler(logger, decoderStrategies, outputManager);
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
            .ToDictionary<FrameType, FrameType, List<IOutput>>(frameType => frameType, frameType => outputsTuple.Where(b => b.Types.Contains(frameType)).Select(o => o.Item2).ToList());

        var logger = s.GetRequiredService<ILogger<FrameType>>();
        
        foreach (var (key, value) in outputDictionary)
        {
            logger.LogInformation("Sending {Type} to {Outputs}", key, string.Join(", ", value.Select(output => output.EndPoint)));
        }
        return outputDictionary;
    }
}