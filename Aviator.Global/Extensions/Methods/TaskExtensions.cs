using Microsoft.Extensions.Logging;

namespace Aviator.Global.Extensions.Methods;

public static class TaskExtensions
{
    public static void FireAndForget(
        this Task task, 
        ILogger logger, 
        string description)
    {
        _ = ForgetAction(task, logger, description);
    }

    private static async Task ForgetAction(Task task, ILogger logger, string description)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in FireAndForget task: {Description}", description);
        }
    }
}