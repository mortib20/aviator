using System.Text.RegularExpressions;

namespace Aviator.Main.Decoders.MediaAdvisory;

/// <summary>
/// Decodes label SA media advisory messages: which datalink medium the
/// aircraft just gained or lost, and which media are currently available.
/// Format: version, E(stablished)/L(ost), medium, HHMMSS, available media,
/// optionally "/" + free text.
/// </summary>
public sealed partial class MediaAdvisoryDecoder : IMessageDecoder
{
    public string Name => "Media Advisory";

    private static readonly Dictionary<char, string> MediaNames = new()
    {
        ['V'] = "VHF ACARS",
        ['S'] = "SATCOM",
        ['H'] = "HF",
        ['G'] = "GlobalStar SATCOM",
        ['C'] = "ICO SATCOM",
        ['2'] = "VDL Mode 2",
        ['X'] = "Inmarsat Aero",
        ['I'] = "Iridium SATCOM",
    };

    [GeneratedRegex(@"^0([EL])([VSHGC2XI])(\d{6})([VSHGC2XI]*)(?:/(.*))?$", RegexOptions.Singleline)]
    private static partial Regex AdvisoryRegex();

    public bool CanDecode(string label, string text) =>
        label == "SA" && AdvisoryRegex().IsMatch(text.Trim());

    public IDecodedMessage Decode(string label, string text)
    {
        var m = AdvisoryRegex().Match(text.Trim());
        var time = m.Groups[3].Value;

        return new MediaAdvisoryDecodedMessage
        {
            LinkEstablished = m.Groups[1].Value == "E",
            Medium = MediaName(m.Groups[2].Value[0]),
            Time = $"{time[..2]}:{time[2..4]}:{time[4..]}",
            AvailableMedia = m.Groups[4].Value.Select(MediaName).ToList(),
            FreeText = m.Groups[5].Success && m.Groups[5].Value.Length > 0
                ? m.Groups[5].Value.Trim()
                : null,
        };
    }

    private static string MediaName(char code) =>
        MediaNames.GetValueOrDefault(code, code.ToString());
}
