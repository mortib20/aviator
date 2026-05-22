using Aviator.Airframe.Frames.Entities;

namespace Aviator.Airframe.Database;

public class AirframeEntity
{
    public long Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string FrameType { get; set; } = "";
    public string ProtocolType { get; set; } = "";
    public string Channel { get; set; } = "";
    public string Icao { get; set; } = "";
    public string SourceAddress { get; set; } = "";
    public string SourceType { get; set; } = "";
    public string DestinationAddress { get; set; } = "";
    public string DestinationType { get; set; } = "";
    public double? SignalLevel { get; set; }
    public double? NoiseLevel { get; set; }

    // Position
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public decimal? Altitude { get; set; }
    public bool? RealPosition { get; set; }

    // ACARS payload
    public string? AcarsLabel { get; set; }
    public string? AcarsRegistration { get; set; }
    public string? AcarsFlightNumber { get; set; }
    public string? AcarsMessageNumber { get; set; }
    public string? AcarsText { get; set; }

    public static AirframeEntity FromDomain(Frames.Entities.Airframe airframe)
    {
        var acars = airframe.Protocol as Acars;
        return new AirframeEntity
        {
            Timestamp = airframe.Timestamp,
            FrameType = airframe.FrameType.ToString(),
            ProtocolType = airframe.ProtocolType.ToString(),
            Channel = airframe.Channel,
            Icao = airframe.Icao,
            SourceAddress = airframe.Source.Address,
            SourceType = airframe.Source.SourceType.ToString(),
            DestinationAddress = airframe.Destination.Address,
            DestinationType = airframe.Destination.DestinationType.ToString(),
            SignalLevel = airframe.SignalLevel,
            NoiseLevel = airframe.NoiseLevel,
            Latitude = airframe.Position?.Latitude,
            Longitude = airframe.Position?.Longitude,
            Altitude = airframe.Position?.Altitude,
            RealPosition = airframe.Position?.RealPosition,
            AcarsLabel = acars?.Label,
            AcarsRegistration = acars?.Registration,
            AcarsFlightNumber = string.IsNullOrEmpty(acars?.FlightNumber) ? null : acars.FlightNumber,
            AcarsMessageNumber = string.IsNullOrEmpty(acars?.MessageNumber) ? null : acars.MessageNumber,
            AcarsText = string.IsNullOrEmpty(acars?.Text) ? null : acars.Text,
        };
    }
}
