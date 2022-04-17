using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialSize;
    
    private List<GameObject> _freeObjects;
    private List<GameObject> _usedObjects;

    private void Awake()
    {
        _freeObjects = new List<GameObject>();
        _usedObjects = new List<GameObject>();
        for (int i = 0; i < initialSize; i++)
        {
            var obj = Instantiate(prefab, transform);
            _freeObjects.Add(obj);
            obj.SetActive(false);
        }
    }

    public GameObject GetObject()
    {
        GameObject obj;
        if (_freeObjects.Count > 0)
        {
            obj = _freeObjects[0];
            _freeObjects.Remove(obj);
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, transform);
        }
        
        _usedObjects.Add(obj);
        return obj;
    }

    public void ReleaseObject(GameObject obj)
    {
        if (_usedObjects.Remove(obj))
        {
            _freeObjects.Add(obj);
        }
        else
        {
            throw new ArgumentException($"Object not in used list: {obj}");
        }
    }
}
