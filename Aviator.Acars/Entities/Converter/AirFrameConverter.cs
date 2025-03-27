using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using Aviator.Acars.Entities.Hfdl;
using Aviator.Acars.Entities.Vdl2;

namespace Aviator.Acars.Entities.Converter;

public abstract class AirFrameConverter
{
    public static AirFrame? FromType(byte[] buffer, [DisallowNull] SourceType? frameType)
    {
        switch (frameType)
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
                throw new ArgumentOutOfRangeException(frameType.ToString());
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

    private static AirFrame ConvertAero(Aero.Aero aero)
    {
        return new AirFrame
        {
            SourceType = SourceType.Aero,
            Channel = aero.app.ver,
            Station = aero.station,
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(aero.t.sec)
        };
    }

    private static AirFrame ConvertDumpVdl2(DumpVdl2 vdl2)
    {
        return new AirFrame
        {
            SourceType = SourceType.Vdl2,
            Channel = vdl2.vdl2.freq.ToString(),
            Station = vdl2.vdl2.station,
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(vdl2.vdl2.t.sec),
            NoiseLevel = vdl2.vdl2.noise_level,
            SigLevel = vdl2.vdl2.sig_level
        };
    }

    private static AirFrame ConvertDumpHfdl(DumpHfdl hfdl)
    {
        return new AirFrame
        {
            SourceType = SourceType.Hfdl,
            Station = hfdl.hfdl.station,
            Channel = hfdl.hfdl.freq.ToString(),
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(hfdl.hfdl.t.sec),
            NoiseLevel = hfdl.hfdl.noise_level,
            SigLevel = hfdl.hfdl.sig_level
        };
    }

    private static AirFrame ConvertAcarsdec(Acarsdec acars)
    {
        return new AirFrame
        {
            SourceType = SourceType.Acars,
            Station = acars.station_id,
            Channel = $"{acars.freq.ToString(CultureInfo.InvariantCulture).Replace(".", "")}000",
            Timestamp = DateTimeOffset.FromUnixTimeSeconds((int)acars.timestamp),
            SigLevel = acars.level
        };
    }

    private static AirFrame ConvertIridium(IridiumAcars iridiumAcars)
    {
        return new AirFrame
        {
            SourceType = SourceType.Iridium,
            Station = iridiumAcars.source.station_id,
            Channel = RoundToFirstFourDigits(iridiumAcars.freq).ToString(),
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(DateTime.Parse(iridiumAcars.acars.timestamp, null, DateTimeStyles.RoundtripKind).Second),
            SigLevel = iridiumAcars.level
        };
    }
}
