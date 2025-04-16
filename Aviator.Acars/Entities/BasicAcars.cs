namespace Aviator.Acars.Entities;

public class BasicAcars
{
    public string Type { get; set; } = string.Empty;
    public string Station { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Registration { get; set; } = string.Empty;
    public string Flight { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public long Timestamp { get; set; }
}

public enum SourceType
{
    Acars,
    Vdl2,
    Hfdl,
    Aero,
    Iridium
}

public enum FrameType
{
    Undefined,
    Acars,
    X25,
    Xid
}

public class AirFrame
{
    public SourceType SourceType { get; set; }
    public FrameType FrameType { get; set; } = FrameType.Undefined;
    public required string Station { get; set; }
    public required string Channel { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    public double SigLevel { get; set; } = 0;
    public double NoiseLevel { get; set; } = 0;
}