using System.Text.Json;
using Aviator.Airframe.Frames;
using Aviator.Airframe.Network;
using Aviator.Global.Service;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe;

public class AirframeService(ILogger<AirframeService> logger, IAirframeInputManager inputManager, AirframeHandler airframeHandler) : AviatorBackgroundService(logger)
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var inputTask = inputManager.StartAsync(stoppingToken);

            await foreach (var bytes in inputManager.ChannelReader.ReadAllAsync(stoppingToken).ConfigureAwait(false))
            {
                try
                {
                    await HandleBytes(bytes, stoppingToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to handle bytes...");
                }
            }

            await inputTask.ConfigureAwait(false);
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

    private Task HandleBytes(byte[] bytes, CancellationToken cancellationToken)
    {
        try
        {
            using var airframe = JsonDocument.Parse(bytes);

            return airframeHandler.HandleAirframeAsync(bytes, airframe.RootElement.Clone(), cancellationToken);
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

        return Task.CompletedTask;
    }
}