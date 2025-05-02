namespace Aviator.Acars.Database;

public interface IAcarsDatabase
{
    public Task InsertAsync(byte[] bytes, CancellationToken cancellationToken = default);
}