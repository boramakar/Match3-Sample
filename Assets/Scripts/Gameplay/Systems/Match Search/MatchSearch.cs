using System.Collections.Generic;
using UnityEngine;

public class MatchSearch : MonoBehaviour, IMatchSearch
{
    //Brute Force
    public bool Search(Coordinate coordinate, TileType tileType, out List<Coordinate> finalList)
    {
        List<Coordinate> listUp = new List<Coordinate>();
        List<Coordinate> listDown = new List<Coordinate>();
        List<Coordinate> listLeft = new List<Coordinate>();
        List<Coordinate> listRight = new List<Coordinate>();
        var direction = new Coordinate(1, 0);
        RecursiveSearch(coordinate + direction, direction, tileType, ref listUp);
        direction.xCoord = -1;
        RecursiveSearch(coordinate + direction, direction, tileType, ref listDown);
        direction.xCoord = 0;
        direction.yCoord = 1;
        RecursiveSearch(coordinate + direction, direction, tileType, ref listRight);
        direction.yCoord = -1;
        RecursiveSearch(coordinate + direction, direction, tileType, ref listLeft);

        finalList = new List<Coordinate>();
        if (listUp.Count + listDown.Count >= 2)
        {
            finalList.AddRange(listUp);
            finalList.AddRange(listDown);
        }
        if (listLeft.Count + listRight.Count >= 2)
        {
            finalList.AddRange(listLeft);
            finalList.AddRange(listRight);
        }

        if (finalList.Count < 2) return false;
        
        finalList.Add(coordinate);
        return true;

    }

    //Basic assumption: We can only make N-consecutive tiles in the cardinal direction of the swapped tiles
    //Approach: Look vertically, record consecutive tiles of swapped type in a list including the swapped tile.
    //If list size is greater than 3, we have a match.
    //Repeat the process horizontally. Add all found tiles together in a new list (can be done on first list as well)
    //Make sure there are no duplicates
    //Remove all tiles in the list and trigger the spawner of the tile's column for each tile if it exists.
    //Another check should be done to handle new matched made by falling tiles.
    //Repeat until no more matches remain and reenable user input.
    //A basic improvement would be to not search in the direction of swap.
    void RecursiveSearch(Coordinate coordinate, Coordinate direction, TileType tileType, ref List<Coordinate> result)
    {
        //Invalid cell
        if(!Utils.IsInsideBounds(coordinate)) return;
        //Different type
        if (EventManager.GetTile(coordinate.xCoord, coordinate.yCoord).GetTileType() != tileType)
            return;
        
        result.Add(coordinate);
        RecursiveSearch(coordinate + direction, direction, tileType, ref result);
    }
}
