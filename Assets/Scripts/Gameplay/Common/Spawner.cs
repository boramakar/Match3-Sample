using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] [ReadOnly] private int _column;
    private int spawnQueue;
    private WaitForSeconds _waitSpawn;
    private Parameters _parameters;

    private static Object _mutex;
    private static int _activeSpawnerCount = 0;
    private static WaitUntil _waitAllSpawners = new WaitUntil(() => _activeSpawnerCount == 0);

    private void Awake()
    {
        _parameters = GameManager.Instance.parameters;
        spawnQueue = 0;
        _waitSpawn = new WaitForSeconds(_parameters.slideDuration + _parameters.postSlideDelay);
        _mutex = new Object();
    }

    private void OnEnable()
    {
        EventManager.OnQueueSpawn += QueueSpawn;
        EventManager.OnSpawn += Spawn;
    }

    private void OnDisable()
    {
        EventManager.OnQueueSpawn -= QueueSpawn;
        EventManager.OnSpawn -= Spawn;
    }

    private void QueueSpawn(int column)
    {
        if (column != _column) return;
        
        Debug.Log($"SpawnQueued: {column}");
        spawnQueue++;
    }

    public void SetColumn(int column)
    {
        _column = column;
    }

    private void Spawn()
    {
        if (spawnQueue == 0) return;
        
        lock (_mutex)
            _activeSpawnerCount++;
        StartCoroutine(_Spawn());
        StartCoroutine(_EnableInput());
    }

    private IEnumerator _EnableInput()
    {
        yield return _waitAllSpawners;
        EventManager.SpawnComplete();
    }

    private IEnumerator _Spawn()
    {
        Debug.Log($"Spawning: {_column} | {spawnQueue}");
        while (spawnQueue > 0)
        {
            --spawnQueue;
            EventManager.ShiftColumn(_column);
            EventManager.SpawnTile(_column);
            yield return _waitSpawn;
        }

        lock (_mutex)
        {
            _activeSpawnerCount--;
        }
    }
}