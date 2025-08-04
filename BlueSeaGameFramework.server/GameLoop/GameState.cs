

    
        #region 游戏状态枚举
    
        /// <summary>
        /// 游戏状态枚举
        /// </summary>
        public enum GameState
        {
            WaitingForStart,    // 等待开始
            GameStarted,        // 游戏开始
            PlayerTurn,         // 玩家回合
            GameEnded           // 游戏结束
        }

        public enum PlayerTurnState
        {
            None,               // 无状态
            FirstPhaseAction,    // 第一阶段行动阶段
            SecondPhaseBattle,   // 第二阶段战斗阶段
            SecondPhaseAction    // 第二阶段行动阶段
        }
    
        #endregion
    
