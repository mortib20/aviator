namespace Aviator.Global.Config.Implementation;

public class InfluxDbConfig : AbstractMetricConfig
{
    public const string Section = "InfluxDB";
    public required string Host { get; set; }
    public required string Bucket { get; set; }
    public required string Organization { get; set; }
    public required string Token { get; set; }
}