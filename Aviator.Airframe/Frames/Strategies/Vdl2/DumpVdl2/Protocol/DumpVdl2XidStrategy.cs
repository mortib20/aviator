using System.Text.Json;
using Aviator.Airframe.Frames.Entities;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames.Strategies.Vdl2.DumpVdl2.Protocol;

public class DumpVdl2XidStrategy(ILogger<DumpVdl2XidStrategy> logger) : IDumpVdl2ProtocolStrategy
{
    public ProtocolType ProtocolType => ProtocolType.Xid;
    public bool CanHandleProtocol(JsonElement protocol)
    {
        return
            protocol.TryGetProperty("xid", out var xid)
            && xid.ValueKind == JsonValueKind.Object;
    }

    public Task<object> HandleProtocolAsync(JsonElement protocol, CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope(nameof(DumpVdl2XidStrategy));
        logger.LogDebug("Handling xid here");

        Position? position = null;

        if (protocol.TryGetProperty("xid", out var xidProp)
            && xidProp.ValueKind == JsonValueKind.Object)
        {
            if (xidProp.TryGetProperty("vdl_params", out var vdlParams)
                && vdlParams.ValueKind == JsonValueKind.Array)
            {
                foreach (var param in vdlParams.EnumerateArray())
                {
                    if (param.TryGetProperty("name", out var nameProp)
                        && nameProp.GetString() == "ac_location"
                        && param.TryGetProperty("value", out var valueProp)
                        && valueProp.ValueKind == JsonValueKind.Object)
                    {
                        if (valueProp.TryGetProperty("loc", out var locProp)
                            && locProp.TryGetProperty("lat", out var latProp)
                            && locProp.TryGetProperty("lon", out var lonProp)
                            && latProp.TryGetDecimal(out var lat)
                            && lonProp.TryGetDecimal(out var lon))
                        {
                            decimal alt = 0;
                            if (valueProp.TryGetProperty("alt", out var altProp)
                                && altProp.TryGetDecimal(out var parsedAlt))
                            {
                                alt = parsedAlt;
                            }

                            position = Position.Create(
                                realPosition: true,
                                latitude: lat,
                                longitude: lon,
                                altitude: alt
                            );

                            break;
                        }
                    }
                }
            }
        }

        return Task.FromResult<object>(new Xid
        {
            Position = position
        });
    }
}