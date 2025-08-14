using Aviator.Global.Config.Implementation;

namespace Aviator.Global.Config;

public class MetricsConfig
{
    public const string Section = "Metrics";
    public InfluxDbConfig? InfluxDb { get; init; }
    public QuestDbConfig? QuestDb { get; init; }
}