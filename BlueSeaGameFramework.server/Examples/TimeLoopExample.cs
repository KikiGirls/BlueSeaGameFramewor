using BlueSeaGameFramework.server.GameLoop;

namespace BlueSeaGameFramework.server.Examples;

/// <summary>
/// 服务器时钟使用示例
/// </summary>
public class TimeLoopExample
{
    private GameServerTimeLoop? _timeLoop;
    private int _updateCount = 0;
    
    public void StartExample()
    {
        Console.WriteLine("启动服务器时钟示例...");
        
        // 创建时钟实例，设置目标帧率为30FPS
        _timeLoop = new GameServerTimeLoop(30);
        
        // 订阅更新事件
        _timeLoop.OnUpdate += OnGameUpdate;
        
        // 订阅FPS变化事件
        _timeLoop.OnFpsChanged += OnFpsChanged;
        
        // 启动时钟
        _timeLoop.Start();
        
        Console.WriteLine($"时钟已启动，目标帧率: {_timeLoop.TargetFrameRate} FPS");
        Console.WriteLine("按任意键停止时钟...");
        
        // 等待用户输入
        Console.ReadKey();
        
        // 停止时钟
        StopExample();
    }
    
    private void OnGameUpdate(double deltaTime)
    {
        _updateCount++;
        
        // 每秒输出一次状态信息
        if (_updateCount % 30 == 0)
        {
            Console.WriteLine($"运行时间: {_timeLoop?.CurrentTimeSeconds:F2}s, " +
                            $"帧数: {_timeLoop?.FrameCount}, " +
                            $"当前FPS: {_timeLoop?.CurrentFps}, " +
                            $"DeltaTime: {deltaTime:F4}s");
        }
        
        // 在这里添加游戏逻辑更新
        UpdateGameLogic(deltaTime);
    }
    
    private void OnFpsChanged(int newFps)
    {
        Console.WriteLine($"FPS变化: {newFps}");
    }
    
    private void UpdateGameLogic(double deltaTime)
    {
        // 模拟游戏逻辑处理
        // 例如：更新玩家位置、处理AI、网络消息等
        
        // 模拟一些计算负载
        if (_updateCount % 100 == 0)
        {
            Thread.Sleep(1); // 模拟偶尔的延迟
        }
    }
    
    private void StopExample()
    {
        Console.WriteLine("正在停止时钟...");
        
        if (_timeLoop != null)
        {
            _timeLoop.OnUpdate -= OnGameUpdate;
            _timeLoop.OnFpsChanged -= OnFpsChanged;
            _timeLoop.Stop();
            _timeLoop.Dispose();
            _timeLoop = null;
        }
        
        Console.WriteLine("时钟已停止");
    }
    
    /// <summary>
    /// 动态调整帧率示例
    /// </summary>
    public void DynamicFrameRateExample()
    {
        _timeLoop = new GameServerTimeLoop(60);
        _timeLoop.OnUpdate += (deltaTime) =>
        {
            // 根据服务器负载动态调整帧率
            var currentFps = _timeLoop.CurrentFps;
            
            if (currentFps < _timeLoop.TargetFrameRate * 0.8) // 如果实际FPS低于目标的80%
            {
                // 降低目标帧率以减少负载
                _timeLoop.TargetFrameRate = Math.Max(10, _timeLoop.TargetFrameRate - 5);
                Console.WriteLine($"检测到性能问题，降低目标帧率至: {_timeLoop.TargetFrameRate}");
            }
            else if (currentFps >= _timeLoop.TargetFrameRate && _timeLoop.TargetFrameRate < 60)
            {
                // 如果性能良好，可以尝试提高帧率
                _timeLoop.TargetFrameRate = Math.Min(60, _timeLoop.TargetFrameRate + 1);
                Console.WriteLine($"性能良好，提升目标帧率至: {_timeLoop.TargetFrameRate}");
            }
        };
        
        _timeLoop.Start();
    }
}
