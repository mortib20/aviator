namespace Aviator.Global.Config.Implementation;

public class QuestDbConfig : AbstractMetricConfig
{
    public const string Section = "QuestDb";
    public string? Host { get; init; }
}