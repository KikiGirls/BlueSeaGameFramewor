
namespace BlueSeaGameFramewor.Core.Utilities.ObjectPool;
public interface IObjectPool<T>
{
    /// <summary>
    /// 从对象池获取一个对象
    /// </summary>
    T Get();

    /// <summary>
    /// 将对象归还到对象池
    /// </summary>
    void Release(T item);

    /// <summary>
    /// 当前池中可用的对象数量
    /// </summary>
    int AvailableCount { get; }

    /// <summary>
    /// 池的最大容量
    /// </summary>
    int MaxSize { get; }

    /// <summary>
    /// 清空池中的所有对象
    /// </summary>
    void Clear();
    
    
}