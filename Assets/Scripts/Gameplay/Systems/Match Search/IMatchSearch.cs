using System.Collections.Generic;

public interface IMatchSearch
{
    public bool Search(Coordinate coordinate, TileType tileType, out List<Coordinate> finalList);
}
