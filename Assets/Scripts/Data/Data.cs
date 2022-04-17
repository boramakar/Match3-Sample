using System;

[Serializable]
public class Coordinate
{
    public int xCoord, yCoord;

    public Coordinate(int x, int y)
    {
        xCoord = x;
        yCoord = y;
    }
    
    public static Coordinate operator+(Coordinate lhs, Coordinate rhs)
    {
        return new Coordinate(lhs.xCoord + rhs.xCoord, lhs.yCoord + rhs.yCoord);
    }
    
    public static Coordinate operator-(Coordinate lhs, Coordinate rhs)
    {
        return new Coordinate(lhs.xCoord - rhs.xCoord, lhs.yCoord - rhs.yCoord);
    }
}

[Serializable]
public enum TileType
{
    Blue,
    Green,
    Red,
    Yellow
}
