using System;

public static class EventManager
{
    //Input Control
    public static event Action<bool> OnChangeInput;
    public static void ChangeInput(bool isEnabled)
    {
        OnChangeInput?.Invoke(isEnabled);
    }
    
    //Gameplay Events
    public static event Action<Coordinate, Coordinate> OnSwapTile;
    public static void SwapTile(Coordinate coord1, Coordinate coord2)
    {
        OnSwapTile?.Invoke(coord1, coord2);
    }

    public static event Action<int> OnQueueSpawn;
    public static void AddSpawn(int columnID)
    {
        OnQueueSpawn?.Invoke(columnID);
    }

    public static event Action OnSpawn;
    public static void Spawn()
    {
        OnSpawn?.Invoke();
    }

    public static event Action<int> OnShiftColumn;
    public static void ShiftColumn(int column)
    {
        OnShiftColumn?.Invoke(column);
    }

    public static event Action<int> OnSpawnTile;
    public static void SpawnTile(int column)
    {
        OnSpawnTile?.Invoke(column);
    }
    
    //Data events
    public static event Func<Coordinate> OnGetDimensions;
    public static Coordinate GetDimensions()
    {
        return OnGetDimensions?.Invoke();
    }

    public static event Func<int, int, Tile> OnGetTile;
    public static Tile GetTile(int xCoord, int yCoord)
    {
        return OnGetTile?.Invoke(xCoord, yCoord);
    }

    public static event Action OnSpawnComplete;
    public static void SpawnComplete()
    {
        OnSpawnComplete?.Invoke();
    }
}
