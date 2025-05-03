namespace Aviator.Airframe.Frames.Entities;

public enum SourceType
{
    Ground,
    Aircraft
}

public class Source
{
    public required string Address { get; set; }
    public required SourceType SourceType { get; set; }

    public override string ToString()
    {
        return $"{nameof(Address)}: {Address}, {nameof(SourceType)}: {SourceType}";
    }
}