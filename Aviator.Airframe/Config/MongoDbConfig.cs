namespace Aviator.Acars.Config;

public class MongoDbConfig
{
    public const string Section = "MongoDB";
    public bool Enabled { get; set; }
    public string? ConnectionString { get; set; }
    public string? Database { get; set; }
    public string? Collection { get; set; }
}