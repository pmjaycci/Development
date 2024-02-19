
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using MessagePack;

public class ServerConnect
{
    private UserInfo? user;
    public ServerConnect(string name)
    {
        user = new UserInfo();
        user!.name = name;
        user.state = (int)State.Idle;
        user.fieldId = 0;
        Console.WriteLine($"ServerConnect User[{name}]");

    }
    public void Run()
    {
        Console.WriteLine($"Run User[{user!.name}]");
        Random rnd = new Random();
        Stopwatch watch = new Stopwatch();

        try
        {

            // 서버의 IP 주소와 포트 번호
            string serverIp = "localhost";//"127.0.0.1"; // 서버의 IP 주소
            int serverPort = 8888; // 서버의 포트 번호
            var client = new TcpClient(serverIp, serverPort);
            // TcpClient 인스턴스 생성

            Console.WriteLine($"Connected to {serverIp}:{serverPort}");

            // NetworkStream을 통해 데이터 전송
            NetworkStream stream = client.GetStream();

            Connect(stream);

            while (true)
            {
                byte[] lengthBytes = new byte[4];

                // 데이터의 길이를 읽음
                int lengthRead = stream.Read(lengthBytes, 0, 4);

                int dataLength = BitConverter.ToInt32(lengthBytes, 0);

                // 실제 데이터를 읽음
                byte[] buffer = new byte[dataLength];

                int bufferRead = stream.Read(buffer, 0, buffer.Length);

                //var stream = client.GetStream();
                if (bufferRead > 0)
                {

                    try
                    {
                        var opcodePacket = MessagePackSerializer.Deserialize<ServerPacket.Packet>(buffer);
                        var opcode = opcodePacket.Opcode;
                        Console.WriteLine("Receieve Opcode:" + opcode);
                        watch.Restart();

                        switch (opcode)
                        {
                            case (int)Opcode.Start:
                                {
                                    var packet = MessagePackSerializer.Deserialize<ServerPacket.Start>(buffer);
                                    Console.WriteLine($"[Start Receive] Name:{user!.name}");
                                    Console.WriteLine($"Message: UserName[{packet.name}] State[{packet.state}] Message[{packet.message}]");
                                    if (packet.name == user.name)
                                    {
                                        user.state = packet.state;
                                        user.fieldId = packet.fieldId;
                                        Console.WriteLine($"본인 상태 변경");
                                    }
                                    var update = new ClientPacket.Update();
                                    update!.Opcode = (int)Opcode.Update;
                                    update.state = user.state;
                                    int newFieldId = rnd.Next(0, 2);
                                    int newState = 0;//rnd.Next(0, 2);
                                    update.state = newState;
                                    update.fieldId = newFieldId;
                                    update.message = "Update 시작!";


                                    byte[] dataBytes = MessagePackSerializer.Serialize(update);

                                    // 데이터의 길이를 구하고 전송
                                    int sendDataLength = dataBytes.Length;

                                    byte[] byteLength = BitConverter.GetBytes(sendDataLength);
                                    stream.Write(byteLength, 0, byteLength.Length);

                                    // 실제 데이터를 전송
                                    stream.Write(dataBytes, 0, dataBytes.Length);

                                }
                                break;
                            case (int)Opcode.Update:
                                {
                                    var packet = MessagePackSerializer.Deserialize<ServerPacket.Update>(buffer);
                                    Console.WriteLine($"[Update Receive] Name:{user!.name}");
                                    Console.WriteLine($"Message: UserName[{packet.name}] State[{packet.state}] Message[{packet.message}]");
                                    if (packet.name == user!.name)
                                    {
                                        user.state = packet.state;
                                        user.fieldId = packet.fieldId;
                                        Console.WriteLine($"본인 상태 변경");
                                    }
                                    var update = new ClientPacket.Update();
                                    update!.Opcode = (int)Opcode.Update;
                                    update.state = user.state;
                                    int newFieldId = 0;//rnd.Next(0, 2);
                                    int newState = 0;//rnd.Next(0, 2);
                                    update.state = newState;
                                    update.fieldId = newFieldId;
                                    update.message = $"Update FieldId[{update.fieldId}] State[{update.state}] 요청!";

                                    byte[] dataBytes = MessagePackSerializer.Serialize(update);

                                    // 데이터의 길이를 구하고 전송
                                    int sendDataLength = dataBytes.Length;

                                    byte[] byteLength = BitConverter.GetBytes(sendDataLength);
                                    stream.Write(byteLength, 0, byteLength.Length);

                                    // 실제 데이터를 전송
                                    stream.Write(dataBytes, 0, dataBytes.Length);
                                    Thread.Sleep(1000);
                                }
                                break;
                        };

                        Console.WriteLine($"결과 시간 : {watch.ElapsedMilliseconds} 밀리초");
                    }
                    catch (MessagePackSerializationException e)
                    {
                        Console.WriteLine($"MessagePackSerializationException Error!!: {e.Message}");
                        Console.WriteLine("--------------------");
                        string message = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        Console.WriteLine(message);
                    }



                }
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void Connect(NetworkStream stream)
    {

        var start = new ClientPacket.Start();
        start!.Opcode = (int)Opcode.Start;
        start.name = user!.name;
        start.message = $"Start!! {user!.name}";

        byte[] dataBytes = MessagePackSerializer.Serialize(start);

        // 데이터의 길이를 구하고 전송
        int dataLength = dataBytes.Length;

        byte[] lengthBytes = BitConverter.GetBytes(dataLength);
        stream.Write(lengthBytes, 0, lengthBytes.Length);

        // 실제 데이터를 전송
        stream.Write(dataBytes, 0, dataBytes.Length);
    }
}
