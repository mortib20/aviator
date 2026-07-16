using System.Text.RegularExpressions;

namespace Aviator.Main.Decoders.Oooi;

/// <summary>
/// Decodes OOOI (Out of the gate, Off the ground, On the ground, In the gate)
/// movement events, either via ARINC 620 Q-labels or OUT/OFF/ON/IN time
/// patterns in the message text.
/// </summary>
public sealed partial class OooiDecoder : IMessageDecoder
{
    public string Name => "OOOI";

    // ARINC 620 movement report labels
    private static readonly Dictionary<string, string> QLabels = new(StringComparer.Ordinal)
    {
        ["QA"] = "OUT report (fuel)",
        ["QB"] = "OFF report",
        ["QC"] = "ON report",
        ["QD"] = "IN report (fuel)",
        ["QE"] = "OUT report (fuel, destination)",
        ["QF"] = "OFF report (destination)",
        ["QG"] = "OUT report (return to gate)",
        ["QH"] = "OUT report",
        ["QK"] = "Landing report",
        ["QL"] = "Arrival report",
        ["QM"] = "Arrival information",
        ["QN"] = "Diversion report",
        ["QP"] = "OUT report",
        ["QQ"] = "OFF report",
        ["QR"] = "ON report",
        ["QS"] = "IN report",
        ["QT"] = "OUT/return IN report",
    };

    // "OUT/0902", "OFF 0915Z", "ON:1042" — 4-digit UTC time after the phase keyword
    [GeneratedRegex(@"\b(OUT|OFF|ON|IN)\s*[/: ]\s*(\d{4})Z?\b")]
    private static partial Regex EventRegex();

    public bool CanDecode(string label, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        if (QLabels.ContainsKey(label)) return true;

        var events = ParseEvents(text);
        // Without a Q-label require OUT or OFF to be present — "ON"/"IN"
        // alone are too common in free text to be a reliable signal
        return events.Count > 0 && events.Any(e => e.Phase is "OUT" or "OFF");
    }

    public IDecodedMessage Decode(string label, string text)
    {
        return new OooiDecodedMessage
        {
            Events = ParseEvents(text),
            LabelMeaning = QLabels.GetValueOrDefault(label),
        };
    }

    private static List<OooiEvent> ParseEvents(string text)
    {
        var events = new List<OooiEvent>();
        foreach (Match m in EventRegex().Matches(text))
        {
            var time = m.Groups[2].Value;
            var hh = int.Parse(time[..2]);
            var mm = int.Parse(time[2..]);
            if (hh > 23 || mm > 59) continue;

            var phase = m.Groups[1].Value;
            if (events.Any(e => e.Phase == phase)) continue;
            events.Add(new OooiEvent { Phase = phase, Time = $"{hh:D2}:{mm:D2}" });
        }
        return events;
    }
}
