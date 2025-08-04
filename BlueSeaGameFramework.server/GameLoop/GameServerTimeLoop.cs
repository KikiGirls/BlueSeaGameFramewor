namespace BlueSeaGameFramework.server.GameLoop;

public class GameServerTimeLoop
{
    private readonly object _lockObject = new object();
    private bool _isRunning = false;
    private Thread? _gameLoopThread;
    
    // 时间相关属性
    private long _startTime;
    private long _currentTime;
    private long _lastFrameTime;
    private double _deltaTime;
    private int _targetFrameRate = 60;
    private long _frameInterval;
    
    // 统计信息
    private int _frameCount;
    private long _lastSecondTime;
    private int _currentFps;
    
    // 事件
    public event Action<double>? OnUpdate;
    public event Action<int>? OnFpsChanged;
    
    public GameServerTimeLoop(int targetFrameRate = 60)
    {
        _targetFrameRate = targetFrameRate;
        _frameInterval = TimeSpan.TicksPerSecond / _targetFrameRate;
        Reset();
    }
    
    /// <summary>
    /// 当前运行时间（毫秒）
    /// </summary>
    public double CurrentTimeMs => (_currentTime - _startTime) / 10000.0;
    
    /// <summary>
    /// 当前运行时间（秒）
    /// </summary>
    public double CurrentTimeSeconds => (_currentTime - _startTime) / 10000000.0;
    
    /// <summary>
    /// 上一帧的时间间隔（秒）
    /// </summary>
    public double DeltaTime => _deltaTime;
    
    /// <summary>
    /// 当前FPS
    /// </summary>
    public int CurrentFps => _currentFps;
    
    /// <summary>
    /// 是否正在运行
    /// </summary>
    public bool IsRunning => _isRunning;
    
    /// <summary>
    /// 总帧数
    /// </summary>
    public int FrameCount => _frameCount;
    
    /// <summary>
    /// 目标帧率
    /// </summary>
    public int TargetFrameRate
    {
        get => _targetFrameRate;
        set
        {
            _targetFrameRate = Math.Max(1, value);
            _frameInterval = TimeSpan.TicksPerSecond / _targetFrameRate;
        }
    }
    
    /// <summary>
    /// 启动游戏循环
    /// </summary>
    public void Start()
    {
        lock (_lockObject)
        {
            if (_isRunning) return;
            
            _isRunning = true;
            Reset();
            
            _gameLoopThread = new Thread(GameLoop)
            {
                Name = "GameServerTimeLoop",
                IsBackground = false
            };
            _gameLoopThread.Start();
        }
    }
    
    /// <summary>
    /// 停止游戏循环
    /// </summary>
    public void Stop()
    {
        lock (_lockObject)
        {
            if (!_isRunning) return;
            
            _isRunning = false;
        }
        
        _gameLoopThread?.Join();
        _gameLoopThread = null;
    }
    
    /// <summary>
    /// 重置时间
    /// </summary>
    private void Reset()
    {
        _startTime = _currentTime = _lastFrameTime = _lastSecondTime = DateTime.UtcNow.Ticks;
        _deltaTime = 0;
        _frameCount = 0;
        _currentFps = 0;
    }
    
    /// <summary>
    /// 主游戏循环
    /// </summary>
    private void GameLoop()
    {
        while (_isRunning)
        {
            var frameStartTime = DateTime.UtcNow.Ticks;
            
            // 更新时间
            UpdateTime(frameStartTime);
            
            // 触发更新事件
            try
            {
                OnUpdate?.Invoke(_deltaTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"游戏循环更新异常: {ex.Message}");
            }
            
            // 更新FPS统计
            UpdateFpsStats(frameStartTime);
            
            // 帧率控制
            ControlFrameRate(frameStartTime);
            
            _frameCount++;
        }
    }
    
    /// <summary>
    /// 更新时间信息
    /// </summary>
    private void UpdateTime(long frameStartTime)
    {
        _currentTime = frameStartTime;
        _deltaTime = (_currentTime - _lastFrameTime) / 10000000.0; // 转换为秒
        _lastFrameTime = _currentTime;
    }
    
    /// <summary>
    /// 更新FPS统计
    /// </summary>
    private void UpdateFpsStats(long frameStartTime)
    {
        if (frameStartTime - _lastSecondTime >= TimeSpan.TicksPerSecond)
        {
            var newFps = (int)(_frameCount * TimeSpan.TicksPerSecond / (frameStartTime - _lastSecondTime));
            if (newFps != _currentFps)
            {
                _currentFps = newFps;
                OnFpsChanged?.Invoke(_currentFps);
            }
            
            _lastSecondTime = frameStartTime;
            _frameCount = 0;
        }
    }
    
    /// <summary>
    /// 控制帧率
    /// </summary>
    private void ControlFrameRate(long frameStartTime)
    {
        var frameEndTime = DateTime.UtcNow.Ticks;
        var frameTime = frameEndTime - frameStartTime;
        var sleepTime = _frameInterval - frameTime;
        
        if (sleepTime > 0)
        {
            var sleepMs = (int)(sleepTime / TimeSpan.TicksPerMillisecond);
            if (sleepMs > 0)
            {
                Thread.Sleep(sleepMs);
            }
        }
    }
    
    /// <summary>
    /// 获取当前UTC时间戳（毫秒）
    /// </summary>
    public static long GetCurrentTimestampMs()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
    
    /// <summary>
    /// 获取当前UTC时间戳（秒）
    /// </summary>
    public static long GetCurrentTimestampSeconds()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
    
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Stop();
    }
}