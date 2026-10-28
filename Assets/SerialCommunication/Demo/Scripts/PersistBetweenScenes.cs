using UnityEngine;

public class PersistBetweenScenes : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}