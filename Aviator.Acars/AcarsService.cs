using System.Text.Json;
using System.Text.Json.Nodes;
using Aviator.Acars.Database;
using Aviator.Acars.Entities;
using Aviator.Acars.Entities.Converter;
using Aviator.Acars.Metrics;
using Aviator.Acars.Network;
using Aviator.Global.Service;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars;

public class AcarsService(ILogger<AcarsService> logger, IAcarsInputManager inputManager, IAcarsOutputManager outputManager, IAcarsMetrics metrics, IAcarsDatabase database, IHubContext<AcarsHub> acarsHub)
    : AviatorBackgroundService(logger)
{
    private const int MinBytes = 128;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await inputManager.StartInputAsync(OnReceivedAsync, stoppingToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured while running Input");
        }
    }

    private async Task OnReceivedAsync(byte[] bytes, CancellationToken cancellationToken)
    {
        if (bytes.Length < MinBytes)
        {
            logger.LogWarning("Received payload to small!");
            return;
        }

        JsonNode jsonAcars;
        try
        {
            jsonAcars = JsonNode.Parse(bytes) ?? throw new InvalidOperationException();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Invalid JSON payload, Ignoring... (But we will save it for debug later)");
            var logPath = Path.Combine(Environment.CurrentDirectory, "logs/json");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            var filename = $"{DateTime.Now:s}.json";
            await File.WriteAllBytesAsync(Path.Combine(logPath, filename), bytes, cancellationToken).ConfigureAwait(false);
            return;
        }

        SourceType sourceType;
        try
        {
            sourceType = SourceTypeFinder.Detect(jsonAcars) ?? throw new InvalidOperationException();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to detect Frame type, ignoring...");
            return;
        }

        try
        {
            await outputManager.WriteToTypeAsync(sourceType, bytes, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured while trying to write to output of {AcarsType}", sourceType);
        }

        try
        {
            await database.InsertAsync(bytes, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save bytes in database!");
        }

        var airFrame = AirFrameConverter.FromType(bytes, sourceType);


        if (airFrame is null)
        {
            return;
        }

        if (SourceTypeFinder.HasAcars(jsonAcars))
        {
            airFrame.FrameType = FrameType.Acars;
        }

        await metrics.IncreaseAsync(airFrame, cancellationToken).ConfigureAwait(false);

        if (airFrame.FrameType == FrameType.Acars)
        {
            var basicAcars = AcarsConverter.BasicAcarsFromType(bytes, airFrame.SourceType);
            await acarsHub.Clients.All.SendAsync("receiveAcarsFrame", JsonSerializer.Serialize(basicAcars), cancellationToken).ConfigureAwait(false);
        }
    }
}