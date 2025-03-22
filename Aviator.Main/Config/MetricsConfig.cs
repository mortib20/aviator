using Aviator.Acars.Config;

namespace Aviator.Main.Config;

public class MetricsConfig
{
    public const string Section = "Metrics";
    public InfluxDbConfig? InfluxDb { get; set; }
}