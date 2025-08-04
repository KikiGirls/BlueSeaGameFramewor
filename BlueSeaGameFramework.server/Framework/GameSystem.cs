using UnityEngine;
using QFramework;
using Scripts.PlayerSystem;

namespace projectName.GameSystem
{
    /// <summary>
    /// 回合制游戏主循环系统
    /// </summary>
    public class GameSystem : AbstractSystem
    {
        private readonly string debugPrefix = "[GameSystem]";
        
        [Header("游戏状态")] public GameState currentGameState = GameState.WaitingForStart;

        [Header("回合设置")] 
        public int currentTurn = 1;
        public int maxTurns = 100;
        public float turnTimeLimit = 30f;

        [Header("玩家设置")] public PlayerName curPlayerName = PlayerName.Lili;
        public int totalPlayers = PlayerSystem.mPlayerCount; // 假设玩家数量由 PlayerSystem 提供

        private float currentTurnTimer;
        private bool isGamePaused = false;
        
        [Header("本地玩家名称")]
        public PlayerName localPlayerName;

        private int currentPlayerIndex;



        #region 游戏初始化

        /// <summary>
        /// 初始化游戏系统发送初始化事件
        /// </summary>  
        public void InitializeGame(PlayerName _localPlayerName)
        {
            Debug.Log($"{debugPrefix} 开始初始化游戏系统...");
            Debug.Log($"{debugPrefix} 本地玩家名称设置为: {_localPlayerName}");
            
            localPlayerName = _localPlayerName;
            curPlayerName = localPlayerName; // 设置当前玩家为本地玩家
            
            Debug.Log($"{debugPrefix} 当前玩家设置为: {curPlayerName}");
            Debug.Log($"{debugPrefix} 总玩家数: {totalPlayers}");
            Debug.Log($"{debugPrefix} 最大回合数: {maxTurns}");
            Debug.Log($"{debugPrefix} 回合时间限制: {turnTimeLimit}秒");
            
            // 发送游戏初始化事件
            var gameInitializeEvent = new GameInitializeEvent()
            {
                LocalPlayerName = localPlayerName
            };
            this.SendEvent<GameInitializeEvent>(gameInitializeEvent);
            Debug.Log($"{debugPrefix} 游戏初始化事件已发送");
            Debug.Log($"{debugPrefix} 游戏系统初始化完成");
        }

        #endregion

        #region 游戏主循环

        /// <summary>
        /// 更新游戏主循环在运行时每帧调用
        /// </summary>
        public void UpdateGameLoop()
        {
            if (isGamePaused) 
            {
                return;
            }

            switch (currentGameState)
            {
                case GameState.WaitingForStart:
                    HandleWaitingForStart();
                    break;

                case GameState.GameStarted:
                    HandleGameStarted();
                    break;

                case GameState.PlayerTurn:
                    HandlePlayerTurn();
                    break;

                case GameState.GameEnded:
                    HandleGameEnded();
                    break;
                
                default:
                    Debug.LogWarning($"{debugPrefix} 未处理的游戏状态: {currentGameState}");
                    break;
            }
        }

        #endregion

        #region 状态处理方法

        /// <summary>
        /// 处理等待开始状态
        /// </summary>
        private void HandleWaitingForStart()
        {
            Debug.Log($"{debugPrefix} 处理等待开始状态 - 等待玩家输入或其他条件来开始游戏");
        }

        /// <summary>
        /// 处理游戏开始状态
        /// </summary>
        private void HandleGameStarted()
        {
            Debug.Log($"{debugPrefix} 游戏开始 - 第{currentTurn}回合， 当前玩家: {curPlayerName.ToString()}");

            // 开始第一个玩家的回合
            StartPlayerTurn();
        }

        /// <summary>
        /// 处理玩家回合状态
        /// </summary>
        private void HandlePlayerTurn()
        {
            // 上一帧的计时器值
            float lastFrameTimer = currentTurnTimer;
            
            // 更新回合计时器
            currentTurnTimer -= Time.deltaTime;
            
            // 检查是否经过了大约一秒（向下取整）
            if (Mathf.FloorToInt(lastFrameTimer) != Mathf.FloorToInt(currentTurnTimer))
            {
                Debug.Log($"{debugPrefix} 玩家{curPlayerName.ToString()}回合剩余时间: {currentTurnTimer:F1}秒");
            }

            // 检查回合时间是否用完
            if (currentTurnTimer <= 0f)
            {
                Debug.Log($"{debugPrefix} 玩家{curPlayerName.ToString()}回合时间用完，剩余时间: {currentTurnTimer:F2}秒");
                EndCurrentPlayerTurn();
            }
        }
        
        /// <summary>
        /// 处理游戏结束状态
        /// </summary>
        private void HandleGameEnded()
        {
            Debug.Log($"{debugPrefix} ��理游戏结束状态 - 游戏已结束");
        }

        #endregion

        #region 回合管理

        /// <summary>
        /// 开始玩家回合
        /// </summary>
        private void StartPlayerTurn()
        {
            Debug.Log($"{debugPrefix} ��备开始玩家回合 - 玩家: {curPlayerName.ToString()}");
            
            currentTurnTimer = turnTimeLimit;
            ChangeGameState(GameState.PlayerTurn);

            Debug.Log($"{debugPrefix} 玩家{curPlayerName.ToString()}开始回合,现在是第{currentTurn}回合，回合时间限制为{turnTimeLimit}秒");

            // 发送玩家回合开始事件
            this.SendEvent(new PlayerTurnStartedEvent
            {
                PlayerName = curPlayerName,
                TurnNumber = currentTurn,
                TimeLimit = turnTimeLimit
            });
            
            Debug.Log($"{debugPrefix} 玩家回合开始事件已发送");
        }

        /// <summary>
        /// 结束当前玩家回合
        /// </summary>
        public void EndCurrentPlayerTurn()
        {
            Debug.Log($"{debugPrefix} 玩家{curPlayerName.ToString()}回合结束，剩余时间: {currentTurnTimer:F2}秒");

            // ���送玩家回合结束事件
            this.SendEvent(new PlayerTurnEndedEvent
            {
                PlayerName = curPlayerName,
                TurnNumber = currentTurn
            });
            
            Debug.Log($"{debugPrefix} 玩家回合结束事件已发送");
            NextTurn();
        }



        /// <summary>
        /// 获取下一个玩家名称
        /// </summary>
        /// <param name="currentPlayer">当前玩家</param>
        /// <returns>下一个玩家名称</returns>
        private PlayerName GetNextPlayerName(PlayerName currentPlayer)
        {
            int playerCount = totalPlayers;
            int currentIndex = (int)currentPlayer;
            int nextIndex = (currentIndex + 1) % playerCount;
            PlayerName nextPlayer = (PlayerName)nextIndex;
            
            Debug.Log($"{debugPrefix} 计算下一个玩家 - 当前: {currentPlayer}(索引{currentIndex}) -> 下一个: {nextPlayer}(索引{nextIndex})");
            
            return nextPlayer;
        }

        /// <summary>
        /// 进入下一回合
        /// </summary>
        private void NextTurn()
        {
            Debug.Log($"{debugPrefix} ��备进入下一回合 - 当前回合: {currentTurn}");
            
            currentTurn++;
            // 切换到下一个玩家
            PlayerName previousPlayer = curPlayerName;
            curPlayerName = GetNextPlayerName(curPlayerName);
            
            Debug.Log($"{debugPrefix} 玩家切换: {previousPlayer} -> {curPlayerName}");
            
            // 检查是否达到最大回合数
            if (currentTurn > maxTurns)
            {
                Debug.Log($"{debugPrefix} 达到最大回合数({maxTurns})，游戏结束");
                EndGame();
            }
            else
            {
                Debug.Log($"{debugPrefix} 进入第{currentTurn}回合，当前玩家: {curPlayerName}");
                StartPlayerTurn();
            }
        }

        #endregion

        #region 游戏控制

        /// <summary>
        /// 改变游戏状态
        /// </summary>
        /// <param name="newState">新状态</param>
        private void ChangeGameState(GameState newState)
        {
            if (currentGameState == newState) 
            {
                Debug.LogWarning($"{debugPrefix} 尝试切换到相同状态: {newState}，忽略操作");
                return;
            }

            GameState oldState = currentGameState;
            currentGameState = newState;

            Debug.Log($"{debugPrefix} 游戏状态改变: {oldState} -> {newState}");

            // 发送状态改变事件
            this.SendEvent(new GameStateChangedEvent
            {
                OldState = oldState,
                NewState = newState
            });
            
            Debug.Log($"{debugPrefix} 游戏状态改变事件已发送");
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartGame()
        {
            Debug.Log($"{debugPrefix} 尝试开始游戏 - 当前状态: {currentGameState}");
            
            if (currentGameState == GameState.WaitingForStart)
            {
                Debug.Log($"{debugPrefix} 游戏状态符合条件，开始游戏");
                ChangeGameState(GameState.GameStarted);
            }
            else
            {
                Debug.LogWarning($"{debugPrefix} 无法开始游戏，当前状态不是WaitingForStart: {currentGameState}");
            }
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            Debug.Log($"{debugPrefix} 暂停游戏 - 当前状态: {currentGameState}");
            
            isGamePaused = true;
            this.SendEvent<GamePausedEvent>();
            
            Debug.Log($"{debugPrefix} 游戏已暂停，暂停事件已发送");
        }

        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void ResumeGame()
        {
            Debug.Log($"{debugPrefix} 恢复游戏 - 当前状态: {currentGameState}");
            
            isGamePaused = false;
            this.SendEvent<GameResumedEvent>();
            
            Debug.Log($"{debugPrefix} 游戏已恢复，恢复事件已发送");
        }

        /// <summary>
        /// 结束游戏
        /// </summary>
        public void EndGame()
        {
            Debug.Log($"{debugPrefix} 结束游戏 - 最终回合: {currentTurn}");
            
            ChangeGameState(GameState.GameEnded);
            
            int winner = DetermineWinner();
            Debug.Log($"{debugPrefix} 胜利者: {(winner == -1 ? "平局" : $"玩家{winner}")}");

            // 发送游戏结束事件
            this.SendEvent(new GameEndedEvent
            {
                FinalTurn = currentTurn,
                Winner = winner
            });
            
            Debug.Log($"{debugPrefix} 游戏结束事件已发送");
        }

        /// <summary>
        /// 重启游戏
        /// </summary>
        public void RestartGame()
        {
            Debug.Log($"{debugPrefix} 重启游戏 - 重置所有状态");
            Debug.Log($"{debugPrefix} 重置前状态 - 回合: {currentTurn}, 玩家: {curPlayerName}, 暂停: {isGamePaused}");
            
            currentTurn = 1;
            curPlayerName = PlayerName.Lili; // 重置为第一个玩家
            isGamePaused = false;
            
            Debug.Log($"{debugPrefix} 重置后状态 - 回合: {currentTurn}, 玩家: {curPlayerName}, 暂停: {isGamePaused}");
            
            ChangeGameState(GameState.GameStarted);

            this.SendEvent<GameRestartedEvent>();
            Debug.Log($"{debugPrefix} 游戏重启完成，重启事件已发送");
        }

        #endregion

        #region 游戏逻辑

        /// <summary>
        /// 确定胜利者
        /// </summary>
        /// <returns>胜利者索引，-1表示平局</returns>
        private int DetermineWinner()
        {
            Debug.Log($"{debugPrefix} 确定胜利者 - 当前逻辑返回平局");
            // 这里可以根据具��游戏规则来确定胜利者
            // 暂时返回-1表示平局
            return -1;
        }

        /// <summary>
        /// 检查游戏是否结束
        /// </summary>
        /// <returns>是否结束</returns>
        public bool IsGameEnded()
        {
            bool isEnded = currentGameState == GameState.GameEnded;
            Debug.Log($"{debugPrefix} 检查游戏是否结束: {isEnded} (当前状态: {currentGameState})");
            return isEnded;
        }

        /// <summary>
        /// 获取当前玩家
        /// </summary>
        /// <returns>当前玩家索引</returns>
        public PlayerName GetCurrentPlayerName()
        {
            int playerIndex = (int)curPlayerName;
            Debug.Log($"{debugPrefix} 获取当前玩家索引: {playerIndex} (玩家名: {curPlayerName})");
            return curPlayerName;
        }

        /// <summary>
        /// 获取剩余回合时间
        /// </summary>
        /// <returns>剩余时间</returns>
        public float GetRemainingTurnTime()
        {
            Debug.Log($"{debugPrefix} 获取剩余回合时间: {currentTurnTimer:F2}秒");
            return currentTurnTimer;
        }

        #endregion

        #region QFramework接口实现

        public IArchitecture GetArchitecture()
        {
            return GameArchitecture.Interface;
        }

        protected override void OnInit()
        {
        }

        #endregion


    }

}
