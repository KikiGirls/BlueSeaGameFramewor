

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BlueSeaGameFramework.Network.Utils;

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
