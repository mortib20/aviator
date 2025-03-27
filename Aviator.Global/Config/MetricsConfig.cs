using Aviator.Main.Config;

namespace Aviator.Global.Config;

public class MetricsConfig
{
    public const string Section = "Metrics";
    public InfluxDbConfig? InfluxDb { get; set; }
}