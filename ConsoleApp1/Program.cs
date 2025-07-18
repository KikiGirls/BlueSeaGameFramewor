// See https://aka.ms/new-console-template for more information

using System.Reflection.Metadata;
using BlueSeaGameFramework.Network.Client;
using BlueSeaGameFramework.Network.Transport;
using BlueSeaGameFramework.Network.Message;
using Example.People;
using Google.Protobuf;
Console.WriteLine("Hello, World");
People temp = new People();
NetworkManager.SetCog(8899,"127.0.0.1", 8890);
NetworkManager.Instance.Init();
NetworkManager.Instance.Connect("192.168.0.144", 8890);

Console.WriteLine("Hello, World!");

NetworkManager.Instance.Send(MessageId.None, temp);
Console.ReadLine();
