using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using Aviator.Acars.Entities.Hfdl;
using Aviator.Acars.Entities.Vdl2;
using Type = Aviator.Acars.Entities.Hfdl.Type;

namespace Aviator.Acars.Entities;

public abstract class AcarsConverter
{
    public static BasicAcars? BasicAcarsFromType(byte[] buffer, [DisallowNull] SourceType? sourceType)
    {
        switch (sourceType)
        {
            case SourceType.Aero:
                var jaero = JsonSerializer.Deserialize<Aero.Aero>(buffer);
                if (jaero is null) break;
                return ConvertAero(jaero);
            case SourceType.Vdl2:
                var vdl2 = JsonSerializer.Deserialize<DumpVdl2>(buffer);
                if (vdl2 is null) break;
                return ConvertDumpVdl2(vdl2);
            case SourceType.Hfdl:
                var hfdl = JsonSerializer.Deserialize<DumpHfdl>(buffer);
                if (hfdl is null) break;
                return ConvertDumpHfdl(hfdl);
            case SourceType.Acars:
                var acars = JsonSerializer.Deserialize<Acarsdec>(buffer);
                if (acars is null) break;
                return ConvertAcarsdec(acars);
            case SourceType.Iridium:
                var iridium = JsonSerializer.Deserialize<IridiumAcars>(buffer);
                if (iridium is null) break;
                return ConvertIridium(iridium);
            default:
                throw new ArgumentOutOfRangeException(sourceType.ToString());
        }

        return null;
    }

    static long RoundToFirstFourDigits(long num)
    {
        // Get the number of digits in the number
        int numberOfDigits = (int)Math.Floor(Math.Log10(num) + 1);

        // Calculate the power of 10 needed to round off the digits after the first four
        int power = numberOfDigits - 4;

        if (power > 0)
        {
            long factor = (long)Math.Pow(10, power);
            return (num / factor) * factor; // Keep only the first four digits
        }

        // If the number has 4 or fewer digits, return as is
        return num;
    }

    private static BasicAcars ConvertAero(Aero.Aero aero)
    {
        return new BasicAcars
        {
            Type = AcarsType.Aero.ToString(),
            Channel = aero.app.ver,
            Station = aero.station,
            Timestamp = aero.t.sec,
            Address = aero.isu.dst.addr,
            Flight = string.Empty,
            Label = aero.isu.acars.label,
            Registration = aero.isu.acars.reg,
            Text = aero.isu.acars.msg_text
        };
    }

    private static BasicAcars ConvertDumpVdl2(DumpVdl2 vdl2)
    {
        return new BasicAcars
        {
            Type = AcarsType.Vdl2.ToString(),
            Channel = vdl2.vdl2.freq.ToString(),
            Address = vdl2.vdl2.avlc.src.addr,
            Flight = vdl2.vdl2.avlc.acars.flight,
            Label = vdl2.vdl2.avlc.acars.label,
            Registration = vdl2.vdl2.avlc.acars.reg,
            Station = vdl2.vdl2.station,
            Timestamp = vdl2.vdl2.t.sec,
            Text = vdl2.vdl2.avlc.acars.msg_text
        };
    }

    private static BasicAcars ConvertDumpHfdl(DumpHfdl hfdl)
    {
        return new BasicAcars
        {
            Type = AcarsType.Hfdl.ToString(),
            Station = hfdl.hfdl.station,
            Channel = hfdl.hfdl.freq.ToString(),
            Label = hfdl.hfdl.lpdu.hfnpdu.acars.label,
            Text = hfdl.hfdl.lpdu.hfnpdu.acars.msg_text,
            Registration = hfdl.hfdl.lpdu.hfnpdu.acars.reg,
            Flight = hfdl.hfdl.lpdu.hfnpdu.acars.flight,
            Address = hfdl.hfdl.lpdu.dst.ac_info?.icao ?? string.Empty,
            Timestamp = hfdl.hfdl.t.sec
        };
    }

    private static BasicAcars ConvertAcarsdec(Acarsdec acars)
    {
        return new BasicAcars
        {
            Type = AcarsType.Acars.ToString(),
            Station = acars.station_id,
            Channel = $"{acars.freq.ToString(CultureInfo.InvariantCulture).Replace(".", "")}000",
            Label = acars.label,
            Text = acars.text,
            Registration = acars.tail,
            Flight = acars.flight,
            Address = string.Empty,
            Timestamp = (int)acars.timestamp
        };
    }

    private static BasicAcars ConvertIridium(IridiumAcars iridiumAcars)
    {
        return new BasicAcars
        {
            Type = AcarsType.Iridium.ToString(),
            Station = iridiumAcars.source.station_id,
            Channel = RoundToFirstFourDigits(iridiumAcars.freq).ToString(),
            Label = iridiumAcars.acars.label,
            Text = iridiumAcars.acars.text,
            Registration = iridiumAcars.acars.tail,
            Flight = "",
            Address = "",
            Timestamp = DateTime.Parse(iridiumAcars.acars.timestamp, null, DateTimeStyles.RoundtripKind).Millisecond
        };
    }
}
