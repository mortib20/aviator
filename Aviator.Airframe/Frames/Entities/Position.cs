namespace Aviator.Airframe.Frames.Entities;

public class Position
{
    // Is probably the real position
    public bool RealPosition { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? Altitude { get; set; }

    public static Position Create(bool realPosition, decimal latitude, decimal longitude, decimal? altitude)
    {
        return new Position
        {
            RealPosition = realPosition,
            Latitude = latitude,
            Longitude = longitude,
            Altitude = altitude
        };
    }
}