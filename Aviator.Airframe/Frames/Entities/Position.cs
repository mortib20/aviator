namespace Aviator.Airframe.Frames.Entities;

public class Position
{
    // Is probably the real position
    public bool RealPosition { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Altitude { get; set; }

    public static Position Create(bool realPosition, double latitude, double longitude, double altitude)
    {
        return new Position()
        {
            RealPosition = realPosition,
            Latitude = latitude,
            Longitude = longitude,
            Altitude = altitude
        };
    }
}