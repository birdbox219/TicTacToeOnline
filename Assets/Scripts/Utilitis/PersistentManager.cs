using UnityEngine;

public class PersistentManager : MonoBehaviour
{
    public static PersistentManager Instance { get; private set; }

    private void Awake()
    {
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        
        DontDestroyOnLoad(gameObject);
    }
}