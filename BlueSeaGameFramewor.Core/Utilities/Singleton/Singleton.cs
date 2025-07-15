using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BlueSeaGameFramewor.Core.Utilities.Singleton;

public class Singleton<T> where T : new()
{
    private static readonly Lazy<T> instance = new Lazy<T>(() => new T());
    public static T Instance => instance.Value;
    
    protected Singleton(){}  
}

[DefaultExecutionOrder(-1000)]
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    

    [Header("单例设置")]
    [Tooltip("是否在场景加载时保持不被销毁")]
    public bool isDontDestroyOnLoad = true;
    
    
    private static T instance;
    private static bool _applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting) {
                Debug.LogWarning($"应用程序退出中，拒绝访问 {typeof(T)} 单例");
                return null;
            }

            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                
                #if UNITY_EDITOR
                if (_instance == null && !Application.isPlaying)
                {
                    // 编辑器模式下自动创建
                    Debug.Log($"创建 {typeof(T)} 单例实例");
                    var go = new GameObject($"{typeof(T).Name} (Singleton)");
                    _instance = go.AddComponent<T>();
                    if (Application.isPlaying)
                    {
                        if ((_instance as EditorFriendlySingleton<T>).isDontDestroyOnLoad)
                        {
                            DontDestroyOnLoad(go);
                        }
                    }
                    else
                    {
                        // 编辑器模式下标记为不保存
                        go.hideFlags = HideFlags.DontSave;
                    }
                }
                #endif

                if (instance == null)
                {
                    Debug.LogError($"找不到 {typeof(T).Name} 的实例，请在场景中创建一个！");
                }
            }
            
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            if (isDontDestroyOnLoad && Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        } else if (instance != this)
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }
            Debug.LogWarning($"检测到重复的 {typeof(T)} 实例，已销毁", instance);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
        if (instance == this)
        {
            instance = null;
        }
    }
    

}