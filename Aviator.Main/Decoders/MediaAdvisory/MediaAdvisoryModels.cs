namespace Aviator.Main.Decoders.MediaAdvisory;

public sealed class MediaAdvisoryDecodedMessage : IDecodedMessage
{
    public string DecoderType => "MediaAdvisory";
    public bool LinkEstablished { get; init; }
    /// <summary>The medium this advisory is about (human-readable name).</summary>
    public string Medium { get; init; } = "";
    /// <summary>UTC time of the event as HH:mm:ss.</summary>
    public string Time { get; init; } = "";
    /// <summary>All currently available media (human-readable names).</summary>
    public List<string> AvailableMedia { get; init; } = [];
    public string? FreeText { get; init; }
}
