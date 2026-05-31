namespace Aviator.Main.Decoders;

public interface IMessageDecoder
{
    string Name { get; }
    bool CanDecode(string label, string text);
    IDecodedMessage Decode(string label, string text);
}
