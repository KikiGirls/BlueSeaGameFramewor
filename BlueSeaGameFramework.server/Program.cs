// See https://aka.ms/new-console-template for more informatio

using BlueSeaGameFramework.server.Network.Client;

namespace BlueSeaGameFramework.server;

public class Server
{
    static void Main(string[] args)
    {
        Console.WriteLine("启动服务器");

        NetSystemInit();
        
        Console.ReadLine();
    }

    static void NetSystemInit()
    {
        NetworkManager.Instance.Init();
        
        Console.WriteLine("启动网络模块");
    }
}