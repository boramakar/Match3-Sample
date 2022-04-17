using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardGenerator : MonoBehaviour, IBoardGenerator
{
    public void FillBoard(in Transform[,] board, in Coordinate dimensions, in ObjectPool tilePool)
    {
        var banList = new List<TileType>();
        //This is required because the update of tileType through Initialize() sometimes doesn't happen fast enough
        //This scheduling problem *SOMETIMES* causes the generator to generate 3 tiles of same type in the same row
        var types = new TileType[dimensions.xCoord, dimensions.yCoord];
        for (int i = 0; i < dimensions.xCoord; i++)
        {
            for (int j = 0; j < dimensions.yCoord; j++)
            {
                var tile = tilePool.GetObject().GetComponent<Tile>();
                var tileType = RandomTileType();
                if (i >= 2 &&
                    (types[i - 1, j] == types[i - 2, j]))
                {
                    banList.Add(types[i - 1, j]);
                }

                if (j >= 2 &&
                    (types[i, j - 1] == types[i, j - 2]))
                {
                    banList.Add(types[i, j - 1]);
                }

                while (banList.Contains(tileType))
                {
                    tileType = RandomTileType();
                }

                types[i, j] = tileType;
                tile.Initialize(new Coordinate(i, j), tileType);
                var tileTransform = tile.transform;
                tileTransform.parent = board[i, j];
                tileTransform.localPosition = Vector3.zero;
                banList.Clear();
            }
        }
    }

    public TileType RandomTileType()
    {
        var randomTile = Random.Range(0, Enum.GetValues(typeof(TileType)).Length);
        var tileType = (TileType) randomTile;
        return tileType;
    }
}