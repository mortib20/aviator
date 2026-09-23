namespace Aviator.Main.Frontend;

/// <summary>
/// Per-circuit browser UTC offset. <c>ToLocalTime()</c> on the server would use the
/// container's zone (UTC in Docker), so timestamps are shifted by the offset the
/// browser reports once the circuit is interactive.
/// </summary>
public sealed class ClientTime
{
    private TimeSpan? _offset;

    public event Action? Changed;

    public bool IsKnown => _offset.HasValue;

    public void SetOffset(TimeSpan offset)
    {
        if (_offset == offset) return;
        _offset = offset;
        Changed?.Invoke();
    }

    public DateTimeOffset ToLocal(DateTimeOffset value) =>
        _offset is { } o ? value.ToOffset(o) : value.ToUniversalTime();

    public string Format(DateTimeOffset value, string format) =>
        ToLocal(value).ToString(format) + (IsKnown ? "" : "Z");
}
