namespace Aviator.Acars.Entities;

public class Acarsdec
{
    public App app { get; set; }
    public string assstat { get; set; }
    public string block_id { get; set; }
    public int channel { get; set; }
    public int error { get; set; }
    public string flight { get; set; }
    public double freq { get; set; }
    public string label { get; set; }
    public double level { get; set; }
    public string mode { get; set; }
    public string msgno { get; set; }
    public string station_id { get; set; }
    public string tail { get; set; }
    public string text { get; set; }
    public double timestamp { get; set; }
}

public class App
{
    public string name { get; set; }
    public string ver { get; set; }
}