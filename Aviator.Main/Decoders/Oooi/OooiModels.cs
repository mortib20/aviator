namespace Aviator.Main.Decoders.Oooi;

public sealed class OooiDecodedMessage : IDecodedMessage
{
    public string DecoderType => "OOOI";
    public List<OooiEvent> Events { get; init; } = [];
    /// <summary>Human-readable meaning of the ARINC 620 Q-label, if the message used one.</summary>
    public string? LabelMeaning { get; init; }
}

public sealed class OooiEvent
{
    /// <summary>OUT, OFF, ON or IN</summary>
    public string Phase { get; init; } = "";
    /// <summary>UTC time as HH:mm, if present in the message</summary>
    public string? Time { get; init; }
}
