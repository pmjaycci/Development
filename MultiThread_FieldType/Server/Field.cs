using System.Collections.Concurrent;
using System.Diagnostics;
using MessagePack;
public class Field
{
    private ConcurrentDictionary<string, UserInfo>? users = new ConcurrentDictionary<string, UserInfo>();
    private ManualResetEvent manualResetEvent = new ManualResetEvent(false);

    public void InUser(UserInfo userInfo, int opcode)
    {
        users!.TryAdd(userInfo.name!, userInfo);
        var packet = new ServerPacket.Start();
        packet!.Opcode = opcode;
        packet.name = userInfo.name;
        packet.state = userInfo.state;
        packet.fieldId = userInfo.fieldId;
        packet.message = userInfo.message;
        ThreadPool.QueueUserWorkItem(_ => BroadcastMessage(packet));
        manualResetEvent.WaitOne();

    }
    public UserInfo OutUser(string name)
    {
        var userInfo = users![name];
        users.TryRemove(name, out _);
        var packet = new ServerPacket.Delete();
        packet!.Opcode = (int)Opcode.Delete;
        packet.name = name;
        Console.WriteLine($"유저 Field 탈출 name:{name}");
        ThreadPool.QueueUserWorkItem(_ => BroadcastMessage(packet));
        //manualResetEvent.WaitOne();

        return userInfo;
    }
    public UserInfo GetUser(string name)
    {
        return users![name];
    }
    public void SetUser(UserInfo userInfo)
    {
        users![userInfo.name!] = userInfo;
        var user = users![userInfo.name!];

        var packet = new ServerPacket.Update();
        packet!.Opcode = (int)Opcode.Update;
        packet.name = user.name;
        packet.state = user.state;
        packet.message = user.message;
        ThreadPool.QueueUserWorkItem(_ => BroadcastMessage(packet));
        //manualResetEvent.WaitOne();

    }
    private void BroadcastMessage(object packet)
    {
        //var message = MessagePackSerializer.Serialize(packet);

        byte[] message = MessagePackSerializer.Serialize(packet);

        // 데이터의 길이를 구하고 전송
        int dataLength = message.Length;

        byte[] lengthBytes = BitConverter.GetBytes(dataLength);


        foreach (var user in users!.Values)
        {
            if (user.client!.Connected)
            {
                var stream = user.client!.GetStream();
                stream.Write(lengthBytes, 0, lengthBytes.Length);

                // 실제 데이터를 전송
                stream.Write(message, 0, message.Length);
            }
        }
        //Console.WriteLine("메시지 보냄");
        //Console.WriteLine($"Broadcast 처리시간 {watch.ElapsedMilliseconds} 밀리초");
        //
        //watch.Restart();
        //manualResetEvent.Set();

    }

}