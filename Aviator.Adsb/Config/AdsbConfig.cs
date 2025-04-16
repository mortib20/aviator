namespace Aviator.Adsb.Config;

public class AdsbConfig
{
    public const string Section = "Adsb";

    public required bool Enabled { get; set; }
    public required string StatsPath { get; set; }
}