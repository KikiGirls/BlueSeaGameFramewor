using QFramework;
namespace BlueSeaGameFramework.server.GameLoop{
/// <summary>
    /// 游戏架构 - QFramework核心架构
    /// </summary>
    public class GameArchitecture : Architecture<GameArchitecture>
    {
        protected override void Init()
        {
            Debug.Log($"GameArchitecture,QFramework系统初始化");
            this.RegisterSystem<GameSystem>(new GameSystem());
        }
    }
    
   
}
