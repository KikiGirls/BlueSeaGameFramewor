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
        
        private PlayerTurnState currentPlayerTurnState = PlayerTurnState.None;
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
        /// 当前玩家回合状态
        /// </summary>
        public PlayerTurnState CurrentPlayerTurnState 
        { 
            get => currentPlayerTurnState;
            private set => currentPlayerTurnState = value;
        }
        
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
        /// 游戏是否已结束
        /// </summary>
        public bool IsGameEnded => GameState == GameState.GameEnded;
        
        /// <summary>
        /// 游戏是否正在进行中
        /// </summary>
        public bool IsGameActive => GameState == GameState.PlayerTurn && !IsPaused;
        
        /// <summary>
        /// 构造函数，初始化所有属性
        /// </summary>
        /// <param name="maxTurns">最大回合数</param>
        /// <param name="turnTimeLimit">回合时间限制</param>
        public GameModel(int maxTurns = DefaultMaxTurns, double turnTimeLimit = DefaultTurnTimeLimit)
        {
            GameState = GameState.WaitingForStart;
            CurrentPlayerTurnState = PlayerTurnState.None;
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
        public void StartGame(PlayerName firstPlayer = PlayerName.None)
        {
            if (GameState != GameState.WaitingForStart)
                return;
                
            GameState = GameState.GameStarted;
            CurrentPlayer = firstPlayer;
            CurrentTurnTime = TurnTimeLimit;
            CurrentTurn = 1;
            IsPaused = false;
            isGameTimePaused = false;
            CurrentPlayerTurnState = PlayerTurnState.FirstPhaseAction;
        }
        
        /// <summary>
        /// 开始玩家回合
        /// </summary>
        /// <param name="player">��前玩家</param>
        public void StartPlayerTurn(PlayerName player)
        {
            if (GameState != GameState.GameStarted && GameState != GameState.PlayerTurn)
                return;
                
            GameState = GameState.PlayerTurn;
            CurrentPlayer = player;
            CurrentTurnTime = TurnTimeLimit;
            CurrentPlayerTurnState = PlayerTurnState.FirstPhaseAction;
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
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            IsPaused = true;
        }
        
        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void ResumeGame()
        {
            IsPaused = false;
        }
        
        /// <summary>
        /// 结束当前玩家回合，进入下一个玩家回合
        /// </summary>
        /// <returns>是否成功进行到下一回合</returns>
        public bool NextTurn()
        {
            if (!IsGameActive)
                return false;

            // 递增回合数
            CurrentTurn++;
            
            // 重置回合时间
            CurrentTurnTime = TurnTimeLimit;
            
            // 重置玩家回合状态
            CurrentPlayerTurnState = PlayerTurnState.FirstPhaseAction;

            // 检查是否达到最大回合数
            if (CurrentTurn > MaxTurns)
            {
                EndGame();
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 切换到下一个玩家回合阶段
        /// </summary>
        /// <returns>是否成功切换到下一阶段</returns>
        public bool NextPhase()
        {
            if (!IsGameActive)
                return false;
                
            switch (CurrentPlayerTurnState)
            {
                case PlayerTurnState.FirstPhaseAction:
                    CurrentPlayerTurnState = PlayerTurnState.SecondPhaseBattle;
                    return true;
                case PlayerTurnState.SecondPhaseBattle:
                    CurrentPlayerTurnState = PlayerTurnState.SecondPhaseAction;
                    return true;
                case PlayerTurnState.SecondPhaseAction:
                    // 阶段结束，准备下一回合
                    return NextTurn();
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// 更新当前回合时间
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        /// <returns>回合时间是否已耗尽</returns>
        public bool UpdateGameModelTurnTime(float deltaTime)
        {
            if (!IsGameActive || IsGameTimePaused)
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
            CurrentPlayerTurnState = PlayerTurnState.None;
            CurrentPlayer = PlayerName.None;
        }
        
        /// <summary>
        /// 重置游戏到初始状态
        /// </summary>
        public void ResetGame()
        {
            GameState = GameState.WaitingForStart;
            CurrentPlayerTurnState = PlayerTurnState.None;
            CurrentTurn = 1;
            CurrentTurnTime = 0.0;
            IsPaused = false;
            CurrentPlayer = PlayerName.None;
            isGameTimePaused = false;
        }
        
        /// <summary>
        /// 设置最大回合数
        /// </summary>
        /// <param name="maxTurns">最大回合数</param>
        public void SetMaxTurns(int maxTurns)
        {
            if (maxTurns > 0)
                MaxTurns = maxTurns;
        }
        
        /// <summary>
        /// 设置回合时间限制
        /// </summary>
        /// <param name="timeLimit">时间限制（秒）</param>
        public void SetTurnTimeLimit(double timeLimit)
        {
            if (timeLimit > 0)
                TurnTimeLimit = timeLimit;
        }
    }

