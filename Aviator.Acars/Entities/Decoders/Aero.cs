namespace Aviator.Acars.Entities.Aero;

public class Aero
{
    public App app { get; set; }
    public Isu isu { get; set; }
    public string station { get; set; }
    public T t { get; set; }
}

public class App
{
    public string name { get; set; }
    public string ver { get; set; }
}

public class Isu
{
    public Acars acars { get; set; }
    public Dst dst { get; set; }
    public string qno { get; set; }
    public string refno { get; set; }
    public Src src { get; set; }
}

public class Acars
{
    public string ack { get; set; }
    public Arinc622 arinc622 { get; set; }
    public string blk_id { get; set; }
    public string label { get; set; }
    public string mode { get; set; }
    public string msg_text { get; set; }
    public string reg { get; set; }
}

public class Arinc622
{
    public string air_addr { get; set; }
    public Cpdlc cpdlc { get; set; }
    public bool crc_ok { get; set; }
    public string gs_addr { get; set; }
    public string msg_type { get; set; }
}

public class Cpdlc
{
    public Atc_uplink_msg atc_uplink_msg { get; set; }
    public bool err { get; set; }
}

public class Atc_uplink_msg
{
    public Atc_uplink_msg_element_id atc_uplink_msg_element_id { get; set; }
    public Header header { get; set; }
}

public class Atc_uplink_msg_element_id
{
    public string choice { get; set; }
    public string choice_label { get; set; }
    public Data data { get; set; }
}

public class Data
{
    public string free_text { get; set; }
}

public class Header
{
    public int msg_id { get; set; }
    public Timestamp timestamp { get; set; }
}

public class Timestamp
{
    public int hour { get; set; }
    public int min { get; set; }
    public int sec { get; set; }
}

public class Dst
{
    public string addr { get; set; }
    public string type { get; set; }
}

public class Src
{
    public string addr { get; set; }
    public string type { get; set; }
}

public class T
{
    public int sec { get; set; }
    public int usec { get; set; }
}