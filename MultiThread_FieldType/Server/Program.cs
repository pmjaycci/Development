
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MessagePack;
using MessagePack.ImmutableCollection;

class Program
{
    static void Main()
    {
        var server = new Server();
        server.Run();
    }
}
public class Server
{
    //-- Index는 Field의 Id로 사용
    public List<Field>? fields = new List<Field>();
    private static object lockObject = new object();
    public void Run()
    {
        TcpListener? server = null;

        for (int i = 0; i < 2; i++)
        {
            var field = new Field();
            fields!.Add(field);
        }

        try
        {
            // 서버 소켓 생성 및 설정
            server = new TcpListener(IPAddress.Any, 8888);
            server.Start();

            Console.WriteLine("서버 시작. 클라이언트 연결 대기 중...");

            while (true)
            {
                // 클라이언트 연결 대기
                TcpClient client = server.AcceptTcpClient();

                // 클라이언트 처리를 위한 스레드 할당
                ThreadPool.QueueUserWorkItem(_ => HandleClient(client, fields));
            }


        }
        catch (Exception ex)
        {
            Console.WriteLine("오류: " + ex.Message);
        }
        finally
        {
            // 서버 소켓 닫기
            server?.Stop();
        }
    }

    static void HandleClient(TcpClient client, List<Field>? fields)
    {
        NetworkStream stream = client.GetStream();
        int startFieldId = 0;
        List<Field>? serverFields = fields;
        var userInfo = new UserInfo();
        userInfo!.client = client;
        Stopwatch watch = new Stopwatch();
        try
        {
            byte[] lengthBytes = new byte[4];
            while (true)
            {
                // 데이터의 길이를 읽음
                int test = stream.Read(lengthBytes, 0, 4);
                int dataLength = BitConverter.ToInt32(lengthBytes, 0);
                // 실제 데이터를 읽음
                byte[] buffer = new byte[dataLength];
                int test2 = stream.Read(buffer, 0, dataLength);
                if (test2 > 0)
                {
                    watch.Start();
                    try
                    {
                        var opcodePacket = MessagePackSerializer.Deserialize<ClientPacket.Packet>(buffer);
                        int opcode = opcodePacket.Opcode;
                        switch (opcode)
                        {
                            case (int)Opcode.Start:
                                {
                                    var packet = MessagePackSerializer.Deserialize<ClientPacket.Start>(buffer);
                                    userInfo.name = packet.name;
                                    userInfo.state = (int)State.Idle;
                                    userInfo.message = $"{packet.message} (완료)";
                                    userInfo.fieldId = startFieldId;
                                    userInfo.client = client;
                                    serverFields![startFieldId].InUser(userInfo!, (int)Opcode.Start);


                                    watch.Stop();
                                    Console.WriteLine($"Opcode[{opcode}][{userInfo.name}] Message[{userInfo.message}] 처리시간 : {watch.ElapsedMilliseconds} 밀리초");
                                    watch.Reset();
                                }
                                break;
                            case (int)Opcode.Update:
                                {
                                    var packet = MessagePackSerializer.Deserialize<ClientPacket.Update>(buffer);
                                    userInfo.state = packet.state;
                                    userInfo.message = $"{packet.message} (완료)";

                                    //-- 필드아이디가 동일한지?
                                    if (userInfo.fieldId == packet.fieldId)
                                    {
                                        var fieldId = userInfo.fieldId;
                                        serverFields![fieldId].SetUser(userInfo);
                                    }
                                    else
                                    {
                                        var newFieldId = packet.fieldId;
                                        var oldFieldId = userInfo.fieldId;
                                        userInfo.fieldId = newFieldId;
                                        //-- 새로운 필드로 유저정보 올리기

                                        serverFields![newFieldId].InUser(userInfo, (int)Opcode.Update);
                                        serverFields![oldFieldId].OutUser(userInfo.name!);

                                    }
                                    watch.Stop();
                                    Console.WriteLine($"Opcode[{opcode}][{userInfo.name}] Message[{userInfo.message}] 처리시간 : {watch.ElapsedMilliseconds} 밀리초");
                                    watch.Reset();
                                }
                                break;
                            case (int)Opcode.Delete:
                                {
                                    //var packet = MessagePackSerializer.Deserialize<ClientPacket.Update>(buffer);
                                }
                                return;
                        }
                    }
                    catch (MessagePackSerializationException ex)
                    {
                        Console.WriteLine($"Deserialize Error!\n{ex.Message}");
                    }


                }

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("클라이언트와의 연결이 종료되었습니다. 오류: " + ex.Message);
        }
        finally
        {
            Console.WriteLine("클라이언트와의 연결이 종료 마무리");
            serverFields![userInfo.fieldId].OutUser(userInfo.name!);

            // 클라이언트 소켓과 스트림 닫기
            client.Close();
            stream.Close();
        }
    }
}