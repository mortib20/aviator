// ReSharper disable InconsistentNaming

using Aviator.Acars.Entities.Decoder.Vdl2;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace Aviator.Acars.Entities.Decoders.Iridium;

public class IridiumAcars
{
    public App app { get; set; }

    public IrSource source { get; set; }
    public IrAcars acars { get; set; }

    public int freq { get; set; }
    public float level { get; set; }
    public string header { get; set; }
}

public class IrSource
{
    public string transport { get; set; }
    public string protocol { get; set; }
    public string station_id { get; set; }
}

public class IrAcars
{
    public string timestamp { get; set; }
    public int errors { get; set; }
    public string link_direction { get; set; }
    public bool block_end { get; set; }
    public string mode { get; set; }
    public string tail { get; set; }
    public string label { get; set; }
    public string block_id { get; set; }
    public string ack { get; set; }
    public string text { get; set; }
}