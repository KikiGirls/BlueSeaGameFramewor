using BlueSeaGameFramework.server;
using BlueSeaGameFramework.server.Network;
using QFramework;

namespace BlueSeaGameFramework.server.GameLoop
{
    /// <summary>
    /// 游戏系统类
    /// 负责管理游戏的整体逻辑，包括玩家初始化状态追踪、游戏开始控制等
    /// 继承自QFramework的AbstractSystem，作为游戏的核心系统组件
    /// </summary>
    public class GameSystem : AbstractSystem
    {
        #region 字段和属性
        
        /// <summary>
        /// 调试���息前缀，用于日志输出的标识
        /// </summary>
        private readonly string _debugPrefix = "[GameSystem]";

        /// <summary>
        /// 玩家初始化状态字典
        /// Key: 玩家名称枚举
        /// Value: 是否已完成初始化（true=已初始化，false=未初始化）
        /// 用于追踪所有玩家的客户端��始化进度
        /// </summary>
        public Dictionary<PlayerName, bool> InitPlayerStatus = new Dictionary<PlayerName, bool>
        {
            { PlayerName.Lili, false },        // 玩家Lili的初始化状态
            { PlayerName.Makusi, false },      // 玩家Makusi的初始化状态
            { PlayerName.Timi, false },        // 玩家Timi的初始化状态
            { PlayerName.Yilaiyasi, false }    // 玩家Yilaiyasi的初始化状态
        };

        /// <summary>
        /// 游戏数据模型
        /// 存储游戏运行时的核心数据
        /// </summary>
        public GameModel? gameModel;
        
        /// <summary>
        /// 游戏服务器时间循环控制器
        /// 用于管理游戏的主循环和时间相关���能
        /// </summary>
        private GameServerTimeLoop? _timeLoop;
        
        #endregion

        #region 系统生命周期
        
        /// <summary>
        /// 系统初始化方法
        /// 在系统创建时调用，用于设置网络事件监听器和初始化游戏状态
        /// </summary>
        protected override void OnInit()
        {
            Debug.Log($"{_debugPrefix} 初始化游戏系统...");
            
            // 初始化游戏数据模型
            gameModel = new GameModel();
            
            // 初始化时间循环控制器
            _timeLoop = new GameServerTimeLoop(60); // 设置为60FPS
            
            RegisterEvents();
            Debug.Log($"{_debugPrefix} 游戏系统初始化完成");
        }

        /// <summary>
        /// 系统销毁时的清理工作
        /// </summary>
        protected override void OnDeinit()
        {
            Debug.Log($"{_debugPrefix} 正在销毁游戏系统...");
            
            // 停止游戏循环
            StopGameLoop();
            
            // 移除可能还存在的事件监听器
            NetworkManager.Instance.Dispose();
            
            Debug.Log($"{_debugPrefix} 游戏系统已销毁");
        }
        
        #endregion

        #region 游戏循环管理
        
        /// <summary>
        /// 启动游戏循环
        /// 在所有玩��初始化完成后调用
        /// </summary>
        private void StartGameTimeLoop()
        {
            if (_timeLoop != null && !_timeLoop.IsRunning)
            {
                // 订阅游戏更新事件
                _timeLoop.OnUpdate += OnGameUpdate;
                _timeLoop.OnFpsChanged += OnFpsChanged;
                
                // 启动游戏循环
                _timeLoop.Start();
                
                Debug.Log($"{_debugPrefix} 游戏循环已启动，目标帧率: {_timeLoop.TargetFrameRate} FPS");
            }
        }

        /// <summary>
        /// 停止游戏循环
        /// 在游戏结束时调用
        /// </summary>
        private void StopGameLoop()
        {
            if (_timeLoop != null && _timeLoop.IsRunning)
            {
                // 取消事件订阅
                _timeLoop.OnUpdate -= OnGameUpdate;
                _timeLoop.OnFpsChanged -= OnFpsChanged;
                
                // 停止��戏循环
                _timeLoop.Stop();
                _timeLoop.Dispose();
                
                Debug.Log($"{_debugPrefix} 游戏循环已停止");
            }
        }

        /// <summary>
        /// 游戏更新回调
        /// 每帧调用，处理游戏逻辑更新
        /// </summary>
        /// <param name="deltaTime">上一帧的时间间隔（秒）</param>
        private void OnGameUpdate(double deltaTime)
        {
            // 在这里添加游戏逻辑更新
            // 例如：更新游戏状态、处理玩家输入、更新AI等
            
            // 更新游戏模型
            if (gameModel == null)
            {
                Debug.LogError($"{_debugPrefix} 游戏模型未初始化，无法更新游戏状态");
                return;
            }
            if (gameModel.UpdateGameModelTurnTime((float)deltaTime))
            {
                ToNextTurn();
            }
        }

        private void ToNextTurn()
        {
            gameModel.NextTurn();
            var turnChangeEvent = new PlayerTurnChangeEventMsg
            {
                NewPlayerName = gameModel.CurrentPlayer,
                TurnNumber = gameModel.CurrentTurn
            };
            NetworkManager.Instance.BroadcastToAll(MessageId.PlayerTurnChangeEventMsg, turnChangeEvent);
            Debug.Log($"{_debugPrefix} 轮到玩家 {gameModel.CurrentPlayer}");
        }

        /// <summary>
        /// FPS变化回调
        /// 当帧率发生变化时调用
        /// </summary>
        /// <param name="newFps">新的FPS值</param>
        private void OnFpsChanged(int newFps)
        {
            Debug.Log($"{_debugPrefix} FPS变化: {newFps}");
            
            // 如果FPS过低，可以考虑降低游戏质量或优化性能
            if (newFps < 30)
            {
                Debug.LogWarning($"{_debugPrefix} 服务器性能警告：FPS低于30 ({newFps})");
            }
        }
        
        #endregion

        #region 游戏状态管理
        
        /// <summary>
        /// 启动游戏流程
        /// 当所有玩家初始化完成后调用
        /// </summary>
        private void StartGameLoop()
        {
            Debug.Log($"{_debugPrefix} 所有玩家均已初始化完成，发送游戏开始事件");
            gameModel.StartGame();
            // 创建游戏开始事件并广播给所有客户端
            var playerTurnChangeEventMsg = new PlayerTurnChangeEventMsg()
            {
                NewPlayerName = gameModel.CurrentPlayer,
                TurnNumber = gameModel.CurrentTurn
            };
            NetworkManager.Instance.BroadcastToAll(MessageId.PlayerTurnChangeEventMsg, playerTurnChangeEventMsg);
            // 移除初始化事件监听器，因为初始化阶段已完成
            NetworkManager.Instance.RemoveEventHandler(MessageId.GameInitEventMsg);


            // 启动游戏循环
            StartGameTimeLoop();
        }

        private void RegisterEvents()
        {
            NetworkManager.Instance.AddEventHandler(MessageId.GameInitEventMsg, HandleClientGameInitEventMsg);
            NetworkManager.Instance.AddEventHandler(MessageId.EndcurrentTurnEventMsg, HandleEndcurrentTurnEventMsg);
            NetworkManager.Instance.AddEventHandler(MessageId.PlayerMoveEventMsg, HandlePlayerMoveEventMsg);
            NetworkManager.Instance.AddEventHandler(MessageId.GameTimePauseEventMag, HandleGameTimePauseEventMag);
            NetworkManager.Instance.AddEventHandler(MessageId.GameTimeResumeEventMsg, HandleGameTimeResumeEventMag);
        }

        private void HandleGameTimeResumeEventMag(BufferEntity obj)
        {
            gameModel.ResumeGameTime();
            Debug.Log($"{_debugPrefix} 恢复游戏时间");
            NetworkManager.Instance.Broadcast(obj);
        }

        private void HandleGameTimePauseEventMag(BufferEntity obj)
        {
            gameModel.PauseGameTime();
            Debug.Log($"{_debugPrefix} 暂停游戏时间");
            NetworkManager.Instance.Broadcast(obj);
        }

        #endregion

        #region 网络事件处理器
        
        /// <summary>
        /// 处理客户端游戏初始化事件
        /// 当客户端发送初始化完成消息时调用此方法
        /// 负责追踪所有玩家的初始化状态，当所有玩家都初始化完成后发送游戏开始事件
        /// </summary>
        /// <param name="obj">网络缓冲区实体，包含客户端发送的消息数据</param>
        private void HandleClientGameInitEventMsg(BufferEntity obj)
        {
            Debug.Log($"{_debugPrefix} 处理客户端游戏初始化事件...");

            // 验证消息数据有效性
            if (obj?.ProtocolData == null)
            {
                Debug.LogError($"{_debugPrefix} 收到空的游戏初始化事件");
                return;
            }

            // 使用Protobuf反序列化消息数据
            var playerInitMessage = ProtobufHelper.GetIMessageFormByte<GameInitEventMsg>(obj.ProtocolData);
            if (playerInitMessage == null)
            {
                Debug.LogError($"{_debugPrefix} 无法解析游戏初始化请求");
                return;
            }

            // 检查玩家是否存在于初始化状态字典中，且尚未初始化
            if (InitPlayerStatus.ContainsKey(playerInitMessage.PlayerName) && !InitPlayerStatus[playerInitMessage.PlayerName])
            {
                // 标记该玩家已完成初始化
                InitPlayerStatus[playerInitMessage.PlayerName] = true;

                Debug.Log($"{_debugPrefix} 玩家 {playerInitMessage.PlayerName} 客户端初始化成功");
                
                // 计算并输出当前初始化进度
                var initializedCount = InitPlayerStatus.Count(p => p.Value);  // 已初始化的玩家数量
                var totalCount = InitPlayerStatus.Count;                       // 总玩家数量
                Debug.Log($"{_debugPrefix} 初始化进度: {initializedCount}/{totalCount}");
            }
            else
            {
                // 玩家已经初始化过或者不在预期的玩家列表中
                Debug.LogWarning($"{_debugPrefix} 玩家 {playerInitMessage.PlayerName} 已经初始化或不存在");
            }
            
            // 检查是否所有玩家都已完成初始化
            if (InitPlayerStatus.All(p => p.Value))
            {
                StartGameLoop();
            }
        }
        private void HandleEndcurrentTurnEventMsg(BufferEntity obj)
        {
            ToNextTurn();
        }
        private void HandlePlayerMoveEventMsg(BufferEntity obj)
        {
            NetworkManager.Instance.Broadcast(obj);
        }
        
        
        #endregion
    }
}