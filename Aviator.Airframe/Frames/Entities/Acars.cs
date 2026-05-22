namespace Aviator.Airframe.Frames.Entities;

public class Acars
{
    public required string Label { get; set; }
    public required string Registration { get; set; }
    public string FlightNumber { get; set; } = "";
    public string MessageNumber { get; set; } = "";
    public string Text { get; set; } = "";
}