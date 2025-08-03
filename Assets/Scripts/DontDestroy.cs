// DontDestroy.cs
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    public static DontDestroy instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
}