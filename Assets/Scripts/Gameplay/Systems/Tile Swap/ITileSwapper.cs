using System;
using UnityEngine;

public interface ITileSwapper
{
    public void SwapTiles(in Transform[,] board, Coordinate coord1, Coordinate coord2, Action callback);
}
