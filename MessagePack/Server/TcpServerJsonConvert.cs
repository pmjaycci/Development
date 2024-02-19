using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

public class TcpServerJsonConvert
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
                string message = Encoding.UTF8.GetString(messageBuffer, 0, bytesRead);
                // 수신된 데이터를 문자열로 변환
                var packet = JsonConvert.DeserializeObject<JsonPacket>(message);
                switch (packet!.Opcode)
                {
                    case (int)Opcode.First:
                        {
                            var req = JsonConvert.DeserializeObject<JsonUserObject1>(message);
                            Console.WriteLine("Read Number:" + req!.number);

                            var res = new UserObject1();
                            res!.Opcode = packet.Opcode;
                            res.number = 2;
                            Console.WriteLine("Send Number:" + res.number);

                            var response = JsonConvert.SerializeObject(res);
                            byte[] responseBuffer = Encoding.UTF8.GetBytes(response);
                            Console.WriteLine($"Response Size:{response.Length} / Buffer Size: {responseBuffer.Length}");

                            clientStream.Write(responseBuffer, 0, responseBuffer.Length);
                            Console.WriteLine("응답 보냄");
                            timeCheck.Stop();
                            Console.WriteLine("경과 시간: " + timeCheck.ElapsedMilliseconds + "밀리초");
                        }
                        break;
                    case (int)Opcode.Second:
                        {
                            var req = JsonConvert.DeserializeObject<JsonUserObject2>(message);
                            Console.WriteLine("Read UserName:" + req!.userName);

                            var res = new UserObject2();
                            res!.Opcode = packet.Opcode;
                            res.userName = "이름 바뀜";
                            Console.WriteLine("Send UserName:" + res.userName);

                            var response = JsonConvert.SerializeObject(res);
                            byte[] responseBuffer = Encoding.UTF8.GetBytes(response);

                            Console.WriteLine($"Response Size:{response.Length} / Buffer Size: {responseBuffer.Length}");

                            clientStream.Write(responseBuffer, 0, responseBuffer.Length);
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
public class JsonUserObject1 : Packet
{
    public int number { get; set; }
}
public class JsonUserObject2 : Packet
{
    public string? userName { get; set; }
}
public class JsonPacket
{
    public int Opcode { get; set; }
}