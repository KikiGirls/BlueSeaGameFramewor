using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BlueSeaGameFramewor.Core.Utilities.Singleton;

/// <summary>
/// 泛型单例基类（非MonoBehaviour）
/// </summary>
/// <typeparam name="T">单例类型</typeparam>
public class Singleton<T> where T : new()
{
    // 使用Lazy实现线程安全的延迟初始化
    private static readonly Lazy<T> instance = new Lazy<T>(() => new T());
    
    /// <summary>
    /// 单例实例访问器
    /// </summary>
    public static T Instance => instance.Value;
    
    /// <summary>
    /// 受保护的构造函数，防止外部实例化
    /// </summary>
    protected Singleton(){}  
}

/// <summary>
/// 基于MonoBehaviour的单例基类
/// </summary>
/// <typeparam name="T">单例类型</typeparam>
[DefaultExecutionOrder(-1000)] // 设置默认执行顺序，确保比其他脚本先执行
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    [Header("单例设置")]
    [Tooltip("是否在场景加载时保持不被销毁")]
    public bool isDontDestroyOnLoad = true; // 是否跨场景保留
    
    private static T instance; // 单例实例
    private static bool _applicationIsQuitting = false; // 标记应用是否正在退出

    /// <summary>
    /// 单例实例访问器
    /// </summary>
    public static T Instance
    {
        get
        {
            // 如果应用正在退出，返回null避免重新创建实例
            if (_applicationIsQuitting) {
                Debug.LogWarning($"应用程序退出中，拒绝访问 {typeof(T)} 单例");
                return null;
            }

            // 如果实例不存在，尝试在场景中查找
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                
                #if UNITY_EDITOR
                // 编辑器特殊处理
                if (instance == null && !Application.isPlaying)
                {
                    // 编辑器模式下自动创建实例
                    Debug.Log($"创建 {typeof(T)} 单例实例");
                    var go = new GameObject($"{typeof(T).Name} (Singleton)");
                    instance = go.AddComponent<T>();
                    if (Application.isPlaying)
                    {
                        // 运行时设置DontDestroyOnLoad
                        if ((instance as MonoSingleton<T>).isDontDestroyOnLoad)
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

                // 如果仍然找不到实例，报错
                if (instance == null)
                {
                    Debug.LogError($"找不到 {typeof(T).Name} 的实例，请在场景中创建一个！");
                }
            }
            
            return instance;
        }
    }

    /// <summary>
    /// Awake生命周期方法，初始化单例
    /// </summary>
    protected virtual void Awake()
    {
        if (instance == null)
        {
            // 设置实例并处理DontDestroyOnLoad
            instance = this as T;
            if (isDontDestroyOnLoad && Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        } 
        else if (instance != this)
        {
            // 销毁重复实例
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

    /// <summary>
    /// 应用退出时清理单例实例
    /// </summary>
    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
        if (instance == this)
        {
            instance = null;
        }
    }
}