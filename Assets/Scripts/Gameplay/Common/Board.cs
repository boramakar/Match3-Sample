using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Board : SerializedMonoBehaviour
{
    [SerializeField] private Coordinate dimensions;
    [SerializeField] private GameObject tileBackgroundPrefab;
    [SerializeField] private GameObject spawnerPrefab;
    [SerializeField] private Camera mainCamera;

    [TableMatrix(Transpose = true, SquareCells = true)] [ReadOnly] [SerializeField]
    private Transform[,] _board;

    private Parameters _parameters;
    private float _xOrigin;
    private float _yOrigin;
    private Transform _rowParent;
    private ObjectPool _tilePool;
    private List<Coordinate> _lastAffectedTiles;

    //Dependencies
    private IBoardGenerator _boardGenerator;
    private ITileSwapper _tileSwapper;
    private IMatchSearch _matchSearch;

    private void Awake()
    {
        //Dependencies
        _boardGenerator = GetComponent<IBoardGenerator>();
        _tileSwapper = GetComponent<ITileSwapper>();
        _matchSearch = GetComponent<IMatchSearch>();

        //Local data
        _parameters = GameManager.Instance.parameters;
        _tilePool = GetComponent<ObjectPool>();
        _rowParent = transform.GetChild(0);
        _lastAffectedTiles = new List<Coordinate>();
        CalculateOrigins();
    }

    private void Start()
    {
        _boardGenerator.FillBoard(in _board, in dimensions, in _tilePool);
    }

    private void OnEnable()
    {
        EventManager.OnSwapTile += SwapTiles;
        EventManager.OnGetTile += GetTile;
        EventManager.OnGetDimensions += GetDimensions;
        EventManager.OnShiftColumn += ShiftColumn;
        EventManager.OnSpawnTile += SpawnTile;
        EventManager.OnSpawnComplete += SearchMatch;
    }

    private void OnDisable()
    {
        EventManager.OnSwapTile -= SwapTiles;
        EventManager.OnGetTile -= GetTile;
        EventManager.OnGetDimensions -= GetDimensions;
        EventManager.OnShiftColumn -= ShiftColumn;
        EventManager.OnSpawnTile -= SpawnTile;
        EventManager.OnSpawnComplete -= SearchMatch;
    }

    private void RemoveTiles(List<Coordinate> matchList)
    {
        foreach (var coordinate in matchList)
        {
            var tileParent = _board[coordinate.xCoord, coordinate.yCoord];
            //Ignore already removed tiles in case the list contains duplicates
            if (tileParent.childCount == 0) continue;
            AddAffectedCoordinate(coordinate.xCoord, coordinate.yCoord);
            var tile = tileParent.GetChild(0).GetComponent<Tile>();
            _tilePool.ReleaseObject(tile.gameObject);
            tile.Disappear();
            tile.transform.parent = transform;
            EventManager.AddSpawn(coordinate.yCoord);
        }

        //Debug.Break();
        EventManager.Spawn();
    }

    private void SwapTiles(Coordinate coord1, Coordinate coord2)
    {
        if (!Utils.IsInsideBounds(coord2) || _board[coord2.xCoord, coord2.yCoord].childCount == 0)
        {
            _board[coord1.xCoord, coord1.yCoord].GetChild(0).GetComponent<Tile>().FailSwap(coord2 - coord1);
        }
        else
            _tileSwapper.SwapTiles(_board, coord1, coord2, () => SearchMatch(coord1, coord2));
    }

    private void SpawnTile(int column)
    {
        var tileTransform = _tilePool.GetObject().transform;
        var row = dimensions.xCoord - 1;
        tileTransform.parent = _board[row, column];
        tileTransform.localPosition = Vector3.zero;
        var tile = tileTransform.GetComponent<Tile>();
        tile.Initialize(new Coordinate(row, column), _boardGenerator.RandomTileType());
        tile.Appear();
    }

    private void ShiftColumn(int column)
    {
        RecursiveShift(dimensions.xCoord - 1, column);
    }

    //Shifts one by one instead of making the tiles fall to the bottom
    private void RecursiveShift(int row, int column)
    {
        AddAffectedCoordinate(row, column);   
        var tileParent = _board[row, column];
        if (row < 1 || tileParent.childCount == 0) return;
        if (_board[row - 1, column].childCount != 0)
        {
            RecursiveShift(row - 1, column);
        }

        var tile = tileParent.GetChild(0).GetComponent<Tile>();
        var tileTransform = tile.transform;
        var newRow = row - 1;
        tileTransform.parent = _board[newRow, column];
        tile.UpdateCoordinates(new Coordinate(newRow, column));
        tile.MoveToPosition();
    }

    private void AddAffectedCoordinate(int row, int column)
    {
        var affectedCoordinate = new Coordinate(row, column);
        //The coordinates in this list should be unique but it would be nice to avoid the search cost before each add
        //For every unique item, the list will be iterated until the end
        //Although it should be short enough it's still a significant amount of extra cost
        if(!_lastAffectedTiles.Contains(affectedCoordinate))
            _lastAffectedTiles.Add(affectedCoordinate);
    }

    //For searching for a match after a swap
    //Can undo swap if no match is found
    private void SearchMatch(Coordinate coord1, Coordinate coord2)
    {
        var tile1 = _board[coord1.xCoord, coord1.yCoord].GetChild(0);
        var tile2 = _board[coord2.xCoord, coord2.yCoord].GetChild(0);
        var match1 =
            _matchSearch.Search(coord1, tile1.GetComponent<Tile>().GetTileType(), out var finalList1);
        var match2 =
            _matchSearch.Search(coord2, tile2.GetComponent<Tile>().GetTileType(), out var finalList2);
        if (match1 || match2)
        {
            finalList1.AddRange(finalList2);
            RemoveTiles(finalList1);
        }
        else
        {
            _tileSwapper.SwapTiles(_board, coord1, coord2, null);
            StartCoroutine(Utils.DelayedAction(_parameters.swapDuration, () => EventManager.ChangeInput(true)));
        }
    }

    //For searching for a match after
    //Bug: Sometimes there exists a match that this function fails to find.
    private void SearchMatch()
    {
        var isMatch = false;
        var removeList = new List<Coordinate>();
        foreach (var coordinate in _lastAffectedTiles)
        {
            var tile = _board[coordinate.xCoord, coordinate.yCoord];
            if (tile.childCount == 0) continue; //Stuff might get matched and removed, ignore such tiles
            
            var match = _matchSearch.Search(coordinate, tile.GetChild(0).GetComponent<Tile>().GetTileType(),
                out var matchList);
            isMatch = isMatch || match;
            if (match)
            {
                removeList.AddRange(matchList);
                
                //We get duplicates here as well, we either clean it here or deal with it in RemoveTiles
                //I prefer to deal with it here to not risk introducing a hidden bug in RemoveTiles
                // foreach (var coord in matchList)
                // {
                //     if (!removeList.Contains(coord))
                //         removeList.Add(coord);
                // }
            }
        }

        if (isMatch)
        {
            RemoveTiles(removeList);
        }
        else
        {
            _lastAffectedTiles.Clear();
            EventManager.ChangeInput(true);
        }
    }

    #region Data Events

    private Tile GetTile(int xCoord, int yCoord)
    {
        return _board[xCoord, yCoord].GetChild(0).GetComponent<Tile>();
    }

    private Coordinate GetDimensions()
    {
        return dimensions;
    }

    #endregion

    #region Helpers

    private void CalculateOrigins()
    {
        _xOrigin = (dimensions.xCoord - 1) / 2f;
        _yOrigin = (dimensions.yCoord - 1) / 2f;
    }

    private void ClearBoard()
    {
        while (transform.GetChild(0).childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).GetChild(0).gameObject);
        }

        while (transform.GetChild(1).childCount > 0)
        {
            DestroyImmediate(transform.GetChild(1).GetChild(0).gameObject);
        }
    }

    #endregion

    #region Editor Scripts

    [Button]
    void InitializeBoard()
    {
        ClearBoard();
        CalculateOrigins();

        _parameters ??= GameManager.Instance.parameters;
        _board = new Transform[dimensions.xCoord, dimensions.yCoord];
        _rowParent = transform.GetChild(0);
        for (int i = 0; i < dimensions.xCoord; i++)
        {
            var row = new GameObject($"Row {i}");
            var rowTransform = row.transform;
            rowTransform.parent = _rowParent;
            rowTransform.localPosition = Vector3.up * (i - _yOrigin) * _parameters.tileSize;
            for (int j = 0; j < dimensions.yCoord; j++)
            {
                var col = Instantiate(tileBackgroundPrefab, rowTransform);
                col.transform.localPosition = Vector3.right * (j - _xOrigin) * _parameters.tileSize;
                col.name = $"Col {j}";
                _board[i, j] = col.transform;
                col.GetComponent<SpriteRenderer>().color = _parameters.tileBackgroundColor;
            }

            if (i != dimensions.xCoord - 1) continue;

            var spawnerParent = transform.GetChild(1);
            spawnerParent.localPosition = rowTransform.localPosition;
            for (int j = 0; j < dimensions.yCoord; j++)
            {
                var spawner = Instantiate(spawnerPrefab, spawnerParent);
                spawner.transform.localPosition = Vector3.right * (j - _xOrigin) * _parameters.tileSize;
                spawner.name = $"Col {j}";
                spawner.GetComponent<Spawner>().SetColumn(j);
            }
        }

        mainCamera.orthographicSize = _parameters.cameraSizePerTile * dimensions.xCoord;
    }

    #endregion
}