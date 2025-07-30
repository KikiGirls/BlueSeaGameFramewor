// See https://aka.ms/new-console-template for more information

using System.Reflection.Metadata;
using BlueSeaGameFramework.Network.Client;
using BlueSeaGameFramework.Network.Transport;
using BlueSeaGameFramework.Network.Message;
using Example.People;
using Google.Protobuf;
using Google.Protobuf.Collections;

Console.WriteLine("Hello, World");
Person temp = new Person();
temp.Id = 1;

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
