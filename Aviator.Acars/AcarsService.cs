using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Aviator.Acars.Database;
using Aviator.Acars.Entities;
using Aviator.Acars.Entities.Converter;
using Aviator.Acars.Metrics;
using Aviator.Acars.Network;
using Aviator.Acars.Utils;
using Aviator.Global.Service;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars;

public class AcarsService(ILogger<AcarsService> logger, IAcarsInputManager inputManager, IAcarsOutputManager outputManager, IAcarsMetrics metrics, IAcarsDatabase database, IHubContext<AcarsHub> acarsHub)
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
        if (AirframeParser.TryParseBytesToJson(bytes, out var jsonAcars))
        {
            return;
        }
        
        if (AirframeParser.TryGetSourceType(jsonAcars, out var sourceType))
        {
            return;
        }
        
        

        await outputManager.SendToOutputOfTypeAsync((SourceType)sourceType!, bytes, cancellationToken).ConfigureAwait(false);

        await InsertIntoDatabaseAsync(bytes, cancellationToken);

        // Advanced handling below...
        
        var airFrame = AirFrameConverter.FromType(bytes, (SourceType)sourceType!);

        if (airFrame is null)
        {
            return;
        }

        if (FrameTypeFinder.HasAcars(jsonAcars))
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

    private async Task InsertIntoDatabaseAsync(byte[] bytes, CancellationToken cancellationToken)
    {
        try
        {
            await database.InsertAsync(bytes, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save bytes in database!");
        }
    }

    private static void WriteInvalidJsonToFile(byte[] bytes)
    {
        var logPath = Path.Combine(Environment.CurrentDirectory, "logs/json");
        if (!Directory.Exists(logPath))
        {
            Directory.CreateDirectory(logPath);
        }

        var filename = $"{DateTime.Now:s}.json";
        File.WriteAllBytes(Path.Combine(logPath, filename), bytes);
    }
}