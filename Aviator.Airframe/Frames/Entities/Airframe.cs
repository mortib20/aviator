namespace Aviator.Airframe.Frames.Entities;

public class Airframe
{
    public required FrameType FrameType { get; set; }
    public required ProtocolType ProtocolType { get; set; }
    public required string Channel { get; set; }
    public required Source Source { get; set; }
    public required Destination Destination { get; set; }
    public double? SignalLevel { get; set; }
    public double? NoiseLevel { get; set; }
    
    

    public static Airframe Create(FrameType frameType, ProtocolType protocolType, string channel, Source source, Destination destination)
    {
        return new Airframe
        {
            FrameType = frameType,
            ProtocolType = protocolType,
            Channel = channel,
            Source = source,
            Destination = destination,
        };
    }

    public static Airframe Create(FrameType frameType, ProtocolType protocolType, string channel, Source source, Destination destination, double signalLevel, double noiseLevel)
    {
        return new Airframe
        {
            FrameType = frameType,
            ProtocolType = protocolType,
            Channel = channel,
            Source = source,
            Destination = destination,
            SignalLevel = signalLevel,
            NoiseLevel = noiseLevel,
        };
    }

    public override string ToString()
    {
        return $"{nameof(FrameType)}: {FrameType}, {nameof(ProtocolType)}: {ProtocolType}, {nameof(Channel)}: {Channel}, {nameof(Source)}: {Source}, {nameof(Destination)}: {Destination}, {nameof(SignalLevel)}: {SignalLevel}, {nameof(NoiseLevel)}: {NoiseLevel}";
    }
}