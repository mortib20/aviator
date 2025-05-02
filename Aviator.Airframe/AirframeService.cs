using System.Text.Json;
using System.Text.Json.Nodes;
using Aviator.Acars.Entities;
using Aviator.Acars.Entities.Converter;
using Aviator.Acars.Entities.Decoder.Hfdl;
using Aviator.Acars.Frames.Strategies;
using Aviator.Acars.Handlers;
using Aviator.Acars.Handlers.Parsers;
using Aviator.Acars.Handlers.PositionStuff;
using Aviator.Acars.Metrics;
using Aviator.Acars.Network;
using Aviator.Global.Service;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars;

public class AirframeService(ILogger<AirframeService> logger, IAirframeInputManager inputManager, IDecoderStrategy decoderStrategy) : AviatorBackgroundService(logger)
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
        try
        {
            using var acarsFrame = JsonDocument.Parse(bytes);
            
        }
        catch (JsonException jsonException)
        {
            logger.LogWarning(jsonException, "Failed to parse bytes...");
        }
        catch (ArgumentException argumentException)
        {
            logger.LogWarning(argumentException, "JsonDocument.Parse() options contain unsupported options...");
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to handle bytes...");
        }

        // await metrics.IncreaseAsync(airFrame, cancellationToken).ConfigureAwait(false);
    }
}