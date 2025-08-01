// See https://aka.ms/new-console-template for more informatio


using Scripts.Player;

namespace BlueSeaGameFramework.server;

public class Server
{
    static void Main(string[] args)
    {
        Console.WriteLine("启动服务器");

        var netmsg = new PlayerMOve()
        {
            Playerid = (int)PlayerName.Lili,
            x = 11,
            y = 0,
            z = 11
        };
        NetSystemInit();
        while (true)
        {
            if (Console.ReadKey().Key == ConsoleKey.A)
            {
                NetworkManager.Instance.Send(MessageId.None, netmsg,3);
            }
        }
        Console.ReadLine();
    }

    static void NetSystemInit()
    {
        NetworkManager.SetCog(8889);
        NetworkManager.Instance.Init();
        
        Console.WriteLine("启动网络模块");
    }
}
