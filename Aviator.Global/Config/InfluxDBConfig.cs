namespace Aviator.Global.Config;

public class InfluxDbConfig
{
    public const string Section = "InfluxDB";

    public required bool Enabled { get; set; } = false;
    public required string Url { get; set; }
    public required string Bucket { get; set; }
    public required string Organization { get; set; }
    public required string Token { get; set; }
}