namespace Aviator.Airframe.Database;

public interface IAcarsDatabase
{
    public Task InsertAsync(byte[] bytes, CancellationToken cancellationToken = default);
}