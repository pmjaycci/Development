using System;
using System.Net.Sockets;
using System.Text;
using MessagePack;
using Newtonsoft.Json;

public class ClientJson
{
    public void Run()
    {
        try
        {
            // 서버 주소와 포트 설정
            string serverAddress = "127.0.0.1";
            int port = 8080;

            // TcpClient 객체 생성
            TcpClient client = new TcpClient(serverAddress, port);

            // 클라이언트의 네트워크 스트림을 얻음
            NetworkStream clientStream = client.GetStream();

            // 메시지 전송
            var packet = new JsonUserObject1();
            packet!.Opcode = (int)Opcode.First;
            packet!.number = 1;
            var req = JsonConvert.SerializeObject(packet);
            byte[] messageBuffer = Encoding.UTF8.GetBytes(req);
            clientStream.Write(messageBuffer, 0, messageBuffer.Length);

            // 서버로부터 응답 받기
            byte[] responseBuffer = new byte[1024];
            int bytesRead = clientStream.Read(responseBuffer, 0, responseBuffer.Length);
            string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
            Console.WriteLine("서버 응답: " + response);

            // 클라이언트 종료
            client.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("에러: " + e.Message);
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

