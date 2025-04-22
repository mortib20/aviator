using Aviator.Acars.Database;
using Aviator.Acars.Entities;
using Aviator.Acars.Network;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars.Handlers;

public class BasicAcarsHandler(ILogger<BasicAcarsHandler> logger, IAcarsOutputManager outputManager, IAcarsDatabase database)
{
    public async Task HandleAsync(byte[] frameBytes, SourceType sourceType, CancellationToken cancellationToken = default)
    {
        using var loggerScope =  logger.BeginScope(sourceType.ToString());
        
        await SendToOutputOfTypeAsync(sourceType, frameBytes, cancellationToken).ConfigureAwait(false);
        await InsertIntoDatabaseAsync(frameBytes, cancellationToken).ConfigureAwait(false);
    }

    private async Task SendToOutputOfTypeAsync(SourceType sourceType, byte[] frameBytes, CancellationToken cancellationToken = default)
    {
        try
        {
            await outputManager.SendToOutputOfTypeAsync(sourceType, frameBytes, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to send frame to output.");
            throw;
        }
    }
    
    private async Task InsertIntoDatabaseAsync(byte[] bytes, CancellationToken cancellationToken = default)
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
}