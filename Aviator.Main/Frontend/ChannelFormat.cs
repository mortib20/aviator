using System.Globalization;

namespace Aviator.Main.Frontend;

public static class ChannelFormat
{
    /// <summary>
    /// Decoders store channels in mixed units (acarsdec/dumpvdl2: Hz, iridium-toolkit: MHz prefix).
    /// Normalise anything that looks like Hz to MHz for display.
    /// </summary>
    public static string Mhz(string? channel)
    {
        if (string.IsNullOrWhiteSpace(channel)) return "";
        if (!double.TryParse(channel, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            return channel;

        var mhz = value >= 1_000_000 ? value / 1_000_000 : value;
        return mhz.ToString("0.000###", CultureInfo.InvariantCulture) + " MHz";
    }
}
