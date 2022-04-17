using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    var obj = new GameObject();
                    _instance = obj.AddComponent<GameManager>();
                }
            }

            return _instance;
        }
    }
    public Parameters parameters;
}
