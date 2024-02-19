using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MessagePack;

public class TCPServerMessagePack
{
    public Dictionary<string, TcpClient> Users = new Dictionary<string, TcpClient>();
    static Stopwatch timeCheck = new Stopwatch();
    public void Run()
    {
        TcpListener server = null!;
        try
        {
            // 서버 주소와 포트 설정
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 8080;

            // TcpListener 객체 생성
            server = new TcpListener(ipAddress, port);

            // 서버 시작
            server.Start();
            Console.WriteLine("서버 시작. 대기 중...");

            while (true)
            {
                // 클라이언트 연결 대기
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("클라이언트 연결됨!");

                ThreadPool.QueueUserWorkItem(HandleClient!, client);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("에러: " + e.Message);
        }
        finally
        {
            server?.Stop();
        }
    }

    static void HandleClient(object obj)
    {
        TcpClient tcpClient = (TcpClient)obj;

        // 클라이언트의 네트워크 스트림을 얻음
        NetworkStream clientStream = tcpClient.GetStream();

        // 버퍼 크기 설정
        byte[] messageBuffer = new byte[1024];
        int bytesRead;
        try
        {
            while ((bytesRead = clientStream.Read(messageBuffer, 0, messageBuffer.Length)) != 0)
            {
                timeCheck.Start();
                // 수신된 데이터를 문자열로 변환
                var packet = MessagePackSerializer.Deserialize<Packet>(messageBuffer);
                byte[] response;
                switch (packet.Opcode)
                {
                    case (int)Opcode.First:
                        {
                            var req = MessagePackSerializer.Deserialize<UserObject1>(messageBuffer);
                            Console.WriteLine("Read Number:" + req.number);

                            var res = new UserObject1();
                            res!.Opcode = packet.Opcode;
                            res.number = 2;
                            Console.WriteLine("Send Number:" + res.number);
                            response = MessagePackSerializer.Serialize(res);

                            clientStream.Write(response, 0, response.Length);
                            Console.WriteLine("응답 보냄");
                            timeCheck.Stop();
                            Console.WriteLine("경과 시간: " + timeCheck.ElapsedMilliseconds + "밀리초");

                        }
                        break;
                    case (int)Opcode.Second:
                        {
                            var req = MessagePackSerializer.Deserialize<UserObject2>(messageBuffer);
                            Console.WriteLine("Read UserName:" + req.userName);

                            var res = new UserObject2();
                            res!.Opcode = packet.Opcode;
                            res.userName = "이름 바뀜";
                            Console.WriteLine("Send UserName:" + res.userName);
                            response = MessagePackSerializer.Serialize(res);
                            clientStream.Write(response, 0, response.Length);
                            Console.WriteLine("응답 보냄");
                        }
                        break;
                }


            }
        }
        catch (Exception e)
        {
            Console.WriteLine("클라이언트와의 연결 종료: " + e.Message);
        }
        finally
        {
            tcpClient.Close();
        }
    }
}
[MessagePackObject]
public class UserObject1 : Packet
{
    [Key(1)]
    public int number { get; set; }
}
[MessagePackObject]
public class UserObject2 : Packet
{
    [Key(1)]
    public string? userName { get; set; }
}
[MessagePackObject]
public class Packet
{
    [Key(0)]
    public int Opcode { get; set; }
}


enum Opcode
{
    First,
    Second,
}