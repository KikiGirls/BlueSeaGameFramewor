// See https://aka.ms/new-console-template for more information

using System.Net.Mime;
using System.Reflection.Metadata;
using BlueSeaGameFramework.Network.Client;
using BlueSeaGameFramework.Network.Transport;
using BlueSeaGameFramework.Network.Message;

Console.WriteLine("Hello, World");
QText temp = new QText();

NetworkManager.SetCog(8891,"127.0.0.1", 8890);
NetworkManager.Instance.Init();
NetworkManager.Instance.Connect();

Console.WriteLine("Hello, World!");
while (true)
{
    if (Console.ReadKey().Key == ConsoleKey.A)
    {
        NetworkManager.Instance.Send(MessageId.None, temp);
    }
}

Console.ReadLine();
