using UnityEngine;
using Object = System.Object;
using BlueSeaGameFramewor.Core.Utilities.Singleton;

namespace BlueSeaGameFramewor.Core.Utilities.ObjectPool;

public class ObjectPool<T> : MonoSingleton<ObjectPool<T>>, IObjectPool<T> where T : new()
{
    public T Get()
    {
        throw new NotImplementedException();
    }

    public void Release(T item)
    {
        throw new NotImplementedException();
    }

    public int AvailableCount { get; }
    public int MaxSize { get; }
    public void Clear()
    {
        throw new NotImplementedException();
    }
}