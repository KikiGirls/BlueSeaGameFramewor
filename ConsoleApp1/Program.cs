// See https://aka.ms/new-console-template for more information

using BlueSeaGameFramework.Network.Client;
using BlueSeaGameFramework.Network.Message;
using Example.People;
using Google.Protobuf;
Console.WriteLine("Hello, World");
People temp = new People();
NetworkManager.SetCog(8899,"192.168.0.1", 9900);
NetworkManager.Instance.Init();
NetworkManager.Instance.Send(MessageId.None, temp);
Console.WriteLine("Hello, World!");
Console.ReadLine();
