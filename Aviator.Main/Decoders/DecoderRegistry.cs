namespace Aviator.Main.Decoders;

public sealed class DecoderRegistry(IEnumerable<IMessageDecoder> decoders)
{
    private readonly IReadOnlyList<IMessageDecoder> _decoders = decoders.ToList();

    public IDecodedMessage? TryDecode(string label, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        foreach (var decoder in _decoders)
        {
            if (decoder.CanDecode(label, text))
                return decoder.Decode(label, text);
        }
        return null;
    }
}
