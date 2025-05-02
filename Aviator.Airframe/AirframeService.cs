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

    private Task HandleBytes(byte[] bytes, CancellationToken cancellationToken)
    {
        try
        {
            using var airframe = JsonDocument.Parse(bytes);

            return airframeHandler.HandleAirframeAsync(airframe.RootElement.Clone(), cancellationToken);
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