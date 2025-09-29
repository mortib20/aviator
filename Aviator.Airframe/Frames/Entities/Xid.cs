namespace Aviator.Airframe.Frames.Entities;

public class Xid
{
    public Position? Position { get; set; }

    public static Xid Create(Position position)
    {
        return new Xid
        {
            Position = position
        };
    }
}