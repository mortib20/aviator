namespace Aviator.Main.Frontend;

public class FrontendConfig
{
    public string IframeUrl { get; set; } = "";
    public List<ExternalLink> ExternalLinks { get; set; } = [];
}

public class ExternalLink
{
    public string Title { get; set; } = "";
    public string Url { get; set; } = "#";
    public string Icon { get; set; } = "bi-link-45deg";
}
