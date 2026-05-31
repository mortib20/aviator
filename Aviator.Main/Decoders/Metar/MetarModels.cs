using Aviator.Main.Decoders;

namespace Aviator.Main.Decoders.Metar;

public sealed class MetarDecodedMessage : IDecodedMessage
{
    public string DecoderType => "METAR";
    public List<MetarReport> Reports { get; init; } = [];
}

public sealed class MetarReport
{
    public string RawText { get; init; } = "";
    public bool IsSpeci { get; init; }
    public string StationIcao { get; init; } = "";
    public DateTime? ObservationTime { get; init; }
    public MetarWind? Wind { get; init; }
    public int? VisibilityMeters { get; init; }
    public bool Cavok { get; init; }
    public List<MetarCloud> Clouds { get; init; } = [];
    public List<string> WeatherGroups { get; init; } = [];
    public double? Temperature { get; init; }
    public double? DewPoint { get; init; }
    public int? Qnh { get; init; }
    public string? Trend { get; init; }
}

public sealed class MetarWind
{
    public int? DirectionDegrees { get; init; }
    public int SpeedKnots { get; init; }
    public int? GustKnots { get; init; }
    public bool IsVariable => DirectionDegrees is null;
    public bool IsCalm => SpeedKnots == 0 && !IsVariable;
}

public sealed class MetarCloud
{
    public string Coverage { get; init; } = "";
    public int? AltitudeFt { get; init; }
    public string? Type { get; init; }
}
