namespace Aviator.Adsb.Entities;

public class AdsbStats
{
    public int AircraftTotal { get; set; }
    public float Gain { get; set; }
    public int MessagesValid { get; set; }
    public int MessagesInvalid { get; set; }
}