using System.Collections.Generic;
using QFramework;
    /// <summary>
    /// 游戏计时暂停接口
    /// 提供暂停和恢复游戏计时的功能
    /// </summary>
    public interface IGameTimePause
    {
        /// <summary>
        /// 游戏计时是否暂停
        /// </summary>
        bool IsGameTimePaused { get; }
        
        /// <summary>
        /// 暂停游戏计时
        /// </summary>
        void PauseGameTime();
        
        /// <summary>
        /// 恢复游戏计时
        /// </summary>
        void ResumeGameTime();
        
        /// <summary>
        /// 切换游戏计时暂停状态
        /// </summary>
        void ToggleGameTimePause();
    }

    /// <summary>
    /// 回合制游戏模型类
    /// 管理游戏的核心状态，包括当前回合、玩家顺序、游戏状态等
    /// </summary>
    public class GameModel : IGameTimePause
    {
        private const int DefaultMaxTurns = 100;
        
        private const double DefaultTurnTimeLimit = 30.0;
        
        private bool isGameTimePaused;
        
        /// <summary>
        /// 游戏计时是否暂停
        /// </summary>
        public bool IsGameTimePaused => isGameTimePaused;
        
        /// <summary>
        /// 当前游戏状态
        /// </summary>
        public GameState GameState { get; private set; }
        
        /// <summary>
        /// 当前回合数
        /// </summary>
        public int CurrentTurn { get; private set; }
        
        /// <summary>
        /// 最大回合数
        /// </summary>
        public int MaxTurns { get; private set; }
        
        
        /// <summary>
        /// 每回合时间限制（秒）
        /// </summary>
        public double TurnTimeLimit { get; private set; }
        
        /// <summary>
        /// 当前回合剩余时间
        /// </summary>
        public double CurrentTurnTime { get; private set; }
        
        /// <summary>
        /// 游戏是否暂停
        /// </summary>
        public bool IsPaused { get; private set; }
        
        /// <summary>
        /// 获得当前行动的玩家
        /// </summary>
        public PlayerName CurrentPlayer { get; private set; }
        
        /// <summary>
        /// 构造函数，初始化所有属性
        /// </summary>
        /// <param name="maxTurns">最大回合数</param>
        /// <param name="turnTimeLimit">回合时间限制</param>
        public GameModel(int maxTurns = DefaultMaxTurns, double turnTimeLimit = DefaultTurnTimeLimit)
        {
            GameState = GameState.WaitingForStart;
            CurrentTurn = 1;
            MaxTurns = maxTurns > 0 ? maxTurns : DefaultMaxTurns;
            TurnTimeLimit = turnTimeLimit > 0 ? turnTimeLimit : DefaultTurnTimeLimit;
            CurrentTurnTime = 0.0;
            IsPaused = false;
            CurrentPlayer = PlayerName.None;
            isGameTimePaused = false;
        }
        
        /// <summary>
        /// 开始游戏
        /// </summary>
        /// <param name="firstPlayer">第一个玩家</param>
        public void StartGame()
        {
            if (GameState != GameState.WaitingForStart)
                return;
                
            GameState = GameState.PlayerTurn;
            CurrentPlayer = PlayerName.Lili;
            CurrentTurnTime = TurnTimeLimit;
            CurrentTurn = 1;
            isGameTimePaused = false;
        }
        
        
        /// <summary>
        /// 暂停游戏计时
        /// </summary>
        public void PauseGameTime()
        {
            isGameTimePaused = true;
        }
        
        /// <summary>
        /// 恢复游戏计时
        /// </summary>
        public void ResumeGameTime()
        {
            isGameTimePaused = false;
        }
        
        /// <summary>
        /// 切换游戏计时暂停状态
        /// </summary>
        public void ToggleGameTimePause()
        {
            isGameTimePaused = !isGameTimePaused;
        }
        
        /// <summary>
        /// 结束当前玩家回合，进入下一个玩家回合
        /// </summary>
        /// <returns>是否成功进行到下一回合</returns>
        public bool NextTurn()
        {

            // 递增回合数
            CurrentTurn++;
            
            // 重置回合时间
            CurrentTurnTime = TurnTimeLimit;
            
            CurrentPlayer = GetNextPlayer(CurrentPlayer);

            // 检查是否达到最大回合数
            if (CurrentTurn > MaxTurns)
            {
                EndGame();
                return false;
            }
            
            return true;
        }
  
        
        /// <summary>
        /// 更新当前回合时间
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        /// <returns>回合时间是否已耗尽</returns>
        public bool UpdateGameModelTurnTime(float deltaTime)
        {
            if (IsGameTimePaused)
                return false;
                
            CurrentTurnTime -= deltaTime;
            
            // 检查时间是否耗尽
            if (CurrentTurnTime <= 0)
            {
                CurrentTurnTime = 0;
                return true; // 时间耗尽
            }
            
            return false;
        }
        
        
        /// <summary>
        /// 结束游戏
        /// </summary>
        public void EndGame()
        {
            GameState = GameState.GameEnded;
            CurrentPlayer = PlayerName.None;
        }
        
        private PlayerName GetNextPlayer(PlayerName currentPlayer)
        {
            switch (currentPlayer)
            {
                case PlayerName.None:
                case PlayerName.Yilaiyasi:
                    return PlayerName.Lili;
                case PlayerName.Lili:
                    return PlayerName.Makusi;
                case PlayerName.Makusi:
                    return PlayerName.Timi;
                case PlayerName.Timi:
                    return PlayerName.Yilaiyasi;
                default:
                    return PlayerName.Lili;
            }
        }
    }

