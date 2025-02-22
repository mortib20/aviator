namespace Aviator.Acars.Entities.Hfdl;

public class DumpHfdl
{
    public Hfdl hfdl { get; set; }
}

public class Hfdl
{
    public App app { get; set; }
    public int bit_rate { get; set; }
    public int freq { get; set; }
    public double freq_skew { get; set; }
    public Lpdu lpdu { get; set; }
    public double noise_level { get; set; }
    public double sig_level { get; set; }
    public string slot { get; set; }
    public string station { get; set; }
    public T t { get; set; }
}

public class App
{
    public string name { get; set; }
    public string ver { get; set; }
}

public class Lpdu
{
    public Dst dst { get; set; }
    public bool err { get; set; }
    public Hfnpdu hfnpdu { get; set; }
    public Src src { get; set; }
    public Type type { get; set; }
}

public class Dst
{
    public int id { get; set; }
    public string type { get; set; }
    public AcInfo? ac_info { get; set; }
}

public class AcInfo
{
    public string? icao { get; set; }
}

public class Hfnpdu
{
    public Acars acars { get; set; }
    public bool err { get; set; }
    public Type1 type { get; set; }
    public Pos? pos { get; set; }
}

public class Acars
{
    public string? ack { get; set; }
    public string blk_id { get; set; }
    public bool crc_ok { get; set; }
    public bool err { get; set; }
    public string flight { get; set; }
    public string label { get; set; }
    public Media_adv media_adv { get; set; }
    public string mode { get; set; }
    public bool more { get; set; }
    public string msg_num { get; set; }
    public string msg_num_seq { get; set; }
    public string msg_text { get; set; }
    public string reg { get; set; }
}

public class Pos
{
    public double? lat { get; set; }
    public double? lon { get; set; }

    public bool HasPos()
    {
        return lat is not null && lon is not null;
    }
}

public class Media_adv
{
    public Current_link current_link { get; set; }
    public bool err { get; set; }
    public Links_avail[] links_avail { get; set; }
    public int version { get; set; }
}

public class Current_link
{
    public string code { get; set; }
    public string descr { get; set; }
    public bool established { get; set; }
    public Time time { get; set; }
}

public class Time
{
    public int hour { get; set; }
    public int min { get; set; }
    public int sec { get; set; }
}

public class Links_avail
{
    public string code { get; set; }
    public string descr { get; set; }
}

public class Type1
{
    public int id { get; set; }
    public string name { get; set; }
}

public class Src
{
    public int id { get; set; }
    public string type { get; set; }
}

public class Type
{
    public int id { get; set; }
    public string name { get; set; }
}

public class T
{
    public int sec { get; set; }
    public int usec { get; set; }
}