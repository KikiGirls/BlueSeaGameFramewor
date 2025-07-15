# BlueSeaGameFramework - 单例模式

## 介绍

该框架提供了一个通用的单例模式实现，分为 `Singleton<T>` 和 `MonoSingleton<T>` 两种形式，分别适用于普通的类和 MonoBehaviour 类型的类。

### `Singleton<T>` 类

用于没有继承自 MonoBehaviour 的单例类，支持延迟加载，线程安全。

### `MonoSingleton<T>` 类

适用于 MonoBehaviour 的单例类，可以选择是否在场景加载时保持不销毁，并且可以在 Unity 编辑器模式下自动创建。

---

## 接口说明

### `Singleton<T>`

#### 属性

- `Instance`  
  获取 `T` 类型的单例实例。如果实例尚未创建，会在第一次访问时进行创建。

#### 构造函数

- `protected Singleton()`  
  保护构造函数，防止外部直接实例化。

### `MonoSingleton<T> : MonoBehaviour`

#### 属性

- `isDontDestroyOnLoad`  
  是否在场景加载时保持该单例对象不被销毁。默认为 `true`，如果设置为 `false`，则在场景加载时会销毁该对象。

- `Instance`  
  获取 `T` 类型的单例实例。如果实例尚未创建，会在第一次访问时进行创建。若应用程序正在退出，则返回 `null`。

#### 方法

- `protected virtual void Awake()`  
  用于初始化单例实例，如果该实例还不存在，则会将当前对象设置为单例实例。根据 `isDontDestroyOnLoad` 设置是否销毁该对象。

- `protected virtual void OnApplicationQuit()`  
  在应用程序退出时被调用，标记单例实例正在退出。

---

## 使用方法

### `Singleton<T>`

#### 示例

```csharp
public class GameManager
{
    public void Initialize()
    {
        // 获取 GameManager 的单例实例
        GameManager manager = Singleton<GameManager>.Instance;
    }
}

public class GameManager : MonoSingleton<GameManager>
{
    // 在 Awake 中会自动初始化单例实例
    protected override void Awake()
    {
        base.Awake(); // 调用父类的 Awake 方法
        // 初始化游戏逻辑
    }
}

public class AudioManager : MonoSingleton<AudioManager>
{
    public void PlaySound(string clipName)
    {
        // 播放音效代码
    }
}

// 使用方式
AudioManager.Instance.PlaySound("explosion");


public class DataManager : Singleton<DataManager>
{
    // 私有构造函数
    protected DataManager() {}
    
    public void SaveData(string key, object value)
    {
        // 存储逻辑
    }
}

// 调用示例
DataManager.Instance.SaveData("score", 100);
```