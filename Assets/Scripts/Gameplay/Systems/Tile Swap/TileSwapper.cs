using System;
using System.Collections;
using UnityEngine;

public class TileSwapper : MonoBehaviour, ITileSwapper
{
    private Parameters _parameters;

    private void Awake()
    {
        _parameters = GameManager.Instance.parameters;
    }

    public void SwapTiles(in Transform[,] board, Coordinate coord1, Coordinate coord2, Action callback)
    {
        StartCoroutine(_SwapTiles(board, coord1, coord2, callback));
    }

    private IEnumerator _SwapTiles(Transform[,] board, Coordinate coord1, Coordinate coord2, Action callback)
    {
        var elapsedTime = 0f;
        var duration = _parameters.swapDuration;
        var parent1 = board[coord1.xCoord, coord1.yCoord];
        var parent2 = board[coord2.xCoord, coord2.yCoord];
        var tile1 = parent1.GetChild(0);
        var tile2 = parent2.GetChild(0);
        var pos1 = tile1.position;
        var pos2 = tile2.position;
        tile1.parent = parent2;
        tile2.parent = parent1;
        while (elapsedTime < duration)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            var progress = elapsedTime / duration;
            //position
            tile1.position = Vector3.Lerp(pos1, pos2, progress) +
                             (Vector3.back * _parameters.swapZPosition.Evaluate(progress));
            tile2.position = Vector3.Lerp(pos2, pos1, progress) +
                             (Vector3.forward * _parameters.swapZPosition.Evaluate(progress));
            //scale
            tile1.localScale = Vector3.one * _parameters.swapScale.Evaluate(progress);
            tile2.localScale = Vector3.one * _parameters.swapScale.Evaluate(progress);
        }

        tile1.position = pos2;
        tile2.position = pos1;
        tile1.GetComponent<Tile>().UpdateCoordinates(coord2);
        tile2.GetComponent<Tile>().UpdateCoordinates(coord1);

        callback?.Invoke();
    }
}
