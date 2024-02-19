using System;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using MessagePack;

class TCPClient
{
    static void Main()
    {
        //var test = new ClientJson();
        //test.Run();
        var test2 = new ClientMessagePack();
        test2.Run();
    }
}


enum Opcode
{
    First,
    Second,
}