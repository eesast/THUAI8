using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonDontDestory<T> : MonoBehaviour
    where T : MonoBehaviour
{
    private static T instance;
    public static T Instance => instance;


    protected void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
