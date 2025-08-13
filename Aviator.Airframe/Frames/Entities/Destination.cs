namespace Aviator.Airframe.Frames.Entities;

public enum DestinationType
{
    Unknown,
    Ground,
    Aircraft
}

public class Destination
{
    public required string Address { get; set; }
    public required DestinationType DestinationType { get; set; }

    public override string ToString()
    {
        return $"{nameof(Address)}: {Address}, {nameof(DestinationType)}: {DestinationType}";
    }
}