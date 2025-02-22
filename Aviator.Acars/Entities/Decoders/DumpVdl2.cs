namespace Aviator.Acars.Entities.Vdl2;

public class DumpVdl2
{
    public Vdl2 vdl2 { get; set; }
}

public class Vdl2
{
    public App app { get; set; }
    public Avlc avlc { get; set; }
    public int burst_len_octets { get; set; }
    public int freq { get; set; }
    public double freq_skew { get; set; }
    public int hdr_bits_fixed { get; set; }
    public int idx { get; set; }
    public double noise_level { get; set; }
    public int octets_corrected_by_fec { get; set; }
    public double sig_level { get; set; }
    public string station { get; set; }
    public T t { get; set; }
}

public class App
{
    public string name { get; set; }
    public string ver { get; set; }
}

public class Avlc
{
    public Acars acars { get; set; }
    public string cr { get; set; }
    public Dst dst { get; set; }
    public string frame_type { get; set; }
    public bool poll { get; set; }
    public int rseq { get; set; }
    public Src src { get; set; }
    public int sseq { get; set; }
}

public class Acars
{
    public string ack { get; set; }
    public string blk_id { get; set; }
    public bool crc_ok { get; set; }
    public bool err { get; set; }
    public string flight { get; set; }
    public string label { get; set; }
    public string mode { get; set; }
    public bool more { get; set; }
    public string msg_num { get; set; }
    public string msg_num_seq { get; set; }
    public string msg_text { get; set; }
    public string reg { get; set; }
}

public class Dst
{
    public string addr { get; set; }
    public string type { get; set; }
}

public class Src
{
    public string addr { get; set; }
    public string status { get; set; }
    public string type { get; set; }
}

public class T
{
    public int sec { get; set; }
    public int usec { get; set; }
}