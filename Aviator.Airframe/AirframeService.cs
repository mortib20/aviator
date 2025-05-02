using System.Text.Json;
using System.Text.Json.Nodes;
using Aviator.Acars.Entities;
using Aviator.Acars.Entities.Converter;
using Aviator.Acars.Entities.Decoder.Hfdl;
using Aviator.Acars.Handlers;
using Aviator.Acars.Handlers.Parsers;
using Aviator.Acars.Handlers.PositionStuff;
using Aviator.Acars.Metrics;
using Aviator.Acars.Network;
using Aviator.Global.Service;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars;

public class AirframeService(ILogger<AirframeService> logger, IAcarsInputManager inputManager, IAcarsMetrics metrics, IHubContext<AirframeHub> acarsHub, BasicAcarsHandler basicAcarsHandler, AcarsPositionState acarsPositionState)
    : AviatorBackgroundService(logger)
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var inputTask = inputManager.StartAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (!inputManager.ChannelReader.TryRead(out var bytes))
                {
                    await Task.Delay(1, stoppingToken).ConfigureAwait(false);
                    continue;
                }

                await HandleBytes(bytes, stoppingToken).ConfigureAwait(false);
            }

            await inputTask;
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured while running Input...");
        }
    }

    private async Task HandleBytes(byte[] bytes, CancellationToken cancellationToken)
    {
        if (!AirframeParser.TryParseBytesToJson(bytes, out var jsonAcars))
        {
            logger.LogInformation("Failed to parse bytes to json.");
            return;
        }
        
        await basicAcarsHandler.HandleAsync(bytes, (SourceType)sourceType, cancellationToken).ConfigureAwait(false);
        
        // Advanced handling below...

        
        var airFrame = AirFrameConverter.FromType(bytes, (SourceType)sourceType!);

        if (airFrame is null)
        {
            return;
        }
        
        if (FrameTypeFinder.HasAcars(jsonAcars))
        {
            airFrame.FrameType = FrameType.Acars;
            
            var basicAcars = AcarsConverter.BasicAcarsFromType(bytes, airFrame.SourceType);
            await acarsHub.Clients.All.SendAsync("receiveAcarsFrame", JsonSerializer.Serialize(basicAcars), cancellationToken).ConfigureAwait(false);

            if (Position.HasAdscPosition(jsonAcars))
            {
                var position = Position.FromAcarsAdscFrame(jsonAcars);
                logger.LogInformation("Got a position adsc {Lat} {Lon} {Reg} {Date}", position.Lat, position.Lon, position.Reg, position.DateTime);
                await acarsPositionState.AddPositionAsync(position, cancellationToken).ConfigureAwait(false);
            }
        }

        if (FrameTypeFinder.HasXid(jsonAcars))
        {
            airFrame.FrameType = FrameType.Xid;

            if (Position.HasXidPosition(jsonAcars))
            {
                var position = Position.FromXidAcLocationFrame(jsonAcars);
                logger.LogInformation("Got a position xid {Lat} {Lon} {Reg} {Date}", position.Lat, position.Lon, position.Reg, position.DateTime);
                await acarsPositionState.AddPositionAsync(position, cancellationToken).ConfigureAwait(false);
            }
        }

        await metrics.IncreaseAsync(airFrame, cancellationToken).ConfigureAwait(false);
    }
    
    public static bool TryParseBytesToJson(byte[] bytes, out JsonNode acarsFrame)
}