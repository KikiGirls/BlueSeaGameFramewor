// See https://aka.ms/new-console-template for more informatio


using BlueSeaGameFramework.server.GameLoop;
namespace BlueSeaGameFramework.server.Network;

public class Server
{
    static void Main(string[] args)
    {
        Console.WriteLine("启动服务器");
        NetSystemInit();
        GameArchitecture.InitArchitecture();
        

        Console.ReadLine();
    }

    static void NetSystemInit()
    {
        NetworkManager.SetConfig(8889);
        NetworkManager.Instance.Init();
        
        Console.WriteLine("启动网络模块");
    }
}
