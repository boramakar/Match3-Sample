using UnityEngine;

public interface IBoardGenerator
{
    public void FillBoard(in Transform[,] board, in Coordinate dimensions, in ObjectPool tilePool);
    public TileType RandomTileType();
}
