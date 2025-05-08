using UnityEngine;

public abstract class ManagerSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance;

    public virtual void Awake()
    {
        SetInstance(true);
    }

    protected void SetInstance(bool ddol)
    {
        if (Instance == null)
        {
            Instance = this as T;
            gameObject.tag = "Singleton";
            if(ddol)
                DontDestroyOnLoad(gameObject);
        }
        else if (gameObject.tag != "Singleton")
        {
            Destroy(gameObject);
        }
    }

    public void DestroyInstance(bool withGameObject = false)
    {
        Instance = null;
        if(withGameObject) Destroy(gameObject);
    }
}
