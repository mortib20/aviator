using System.Text.Json.Serialization;

namespace Aviator.Airframe.Frames.Entities;

public class Airframe
{
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required FrameType FrameType { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ProtocolType ProtocolType { get; set; }
    public required string Channel { get; set; }
    public required string Icao { get; set; }
    public required Source Source { get; set; }
    public required Destination Destination { get; set; }
    public double? SignalLevel { get; set; }
    public double? NoiseLevel { get; set; }
    
    public object? Protocol { get; set; }
    public Position? Position { get; set; }
    
    

    public static Airframe Create(FrameType frameType,
        ProtocolType protocolType,
        string channel,
        Source source,
        Destination destination,
        double? signalLevel = null,
        double? noiseLevel = null,
        object? protocol = null,
        Position? position = null,
        string? icao = null)
    {
        return new Airframe
        {
            FrameType = frameType,
            ProtocolType = protocolType,
            Channel = channel,
            Icao = icao ?? string.Empty,
            Source = source,
            Destination = destination,
            SignalLevel = signalLevel,
            NoiseLevel = noiseLevel,
            Protocol = protocol,
            Position = position
        };
    }

    // public static Airframe Create(FrameType frameType, ProtocolType protocolType, string channel, Source source, Destination destination, double signalLevel, double noiseLevel)
    // {
    //     return new Airframe
    //     {
    //         FrameType = frameType,
    //         ProtocolType = protocolType,
    //         Channel = channel,
    //         Source = source,
    //         Destination = destination,
    //         SignalLevel = signalLevel,
    //         NoiseLevel = noiseLevel,
    //     };
    // }

    public override string ToString()
    {
        return $"{nameof(FrameType)}: {FrameType}, {nameof(ProtocolType)}: {ProtocolType}, {nameof(Channel)}: {Channel}, {nameof(Source)}: {Source}, {nameof(Destination)}: {Destination}, {nameof(SignalLevel)}: {SignalLevel}, {nameof(NoiseLevel)}: {NoiseLevel}";
    }
}