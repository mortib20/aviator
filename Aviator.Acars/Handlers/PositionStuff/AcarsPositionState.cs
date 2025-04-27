using Microsoft.AspNetCore.SignalR;

namespace Aviator.Acars.Handlers.PositionStuff;

public class AcarsPositionState
{
    private Timer _deleteTimer;
    private readonly IHubContext<AcarsHub> _acarsHub;

    public AcarsPositionState(IHubContext<AcarsHub> acarsHub)
    {
        _acarsHub = acarsHub;
        _deleteTimer = new Timer(DeleteOldPositions);
        _deleteTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(10));
    }

    public List<Position> Positions { get; set; } = [];

    public async Task AddPositionAsync(Position position, CancellationToken cancellationToken = default)
    {
        Positions.Add(position);

        await _acarsHub.Clients.Group("Position").SendAsync("Position", position, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private void DeleteOldPositions(object? o)
    {
        var t = Positions.Where(p => p.DateTime > DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)));

        Positions = t.ToList();
    }
}