using System.Net;
using System.Text;
using Server;
using ServerCore;
public class Test
{
    public string Message;
}
class Program
{
    public void RecvTest(ArraySegment<byte> buffer)
    {
        //ArraySegment<byte> buffer = segment;//new ArraySegment<byte>(packetBytes);

        int count = 0;
        ushort messageSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 4;
        ushort recvOpcode = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 4;
        byte[] dataBytes = new byte[messageSize];
        int offset = buffer.Offset + count;
        Array.Copy(buffer.Array, offset, dataBytes, 0, dataBytes.Length);

        // 추출한 바이트 배열을 문자열로 변환
        string dataString = Encoding.UTF8.GetString(dataBytes);

    }

    public void SendTest(int opcode, string jsonPacket)
    {
        byte[] opcodeBytes = BitConverter.GetBytes(opcode);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPacket);

        ushort size = (ushort)(2 + opcodeBytes.Length + jsonBytes.Length);
        byte[] sizeBytes = BitConverter.GetBytes(size);

        byte[] packetBytes = new byte[size];
        int offset = 0;
        Array.Copy(sizeBytes, 0, packetBytes, offset, sizeBytes.Length);
        offset += sizeBytes.Length;
        Array.Copy(opcodeBytes, 0, packetBytes, offset, opcodeBytes.Length);
        offset += opcodeBytes.Length;
        Array.Copy(jsonBytes, 0, packetBytes, offset, jsonBytes.Length);

        ArraySegment<byte> buffer = new ArraySegment<byte>(packetBytes);
    }

    static Listener _listener = new Listener();
    public static ChatRoom Room = new ChatRoom();
    static void FlushRoom()
    {
        Room.Push(() => Room.Flush());
        JobTimer.Instance.Push(FlushRoom, 250);
    }

    static void Main(string[] args)
    {

        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);


        ClientSession session = new ClientSession();
        session.SessionId = 0;
        _listener.Init(endPoint, session);
        Console.WriteLine("Listening...");

        JobTimer.Instance.Push(FlushRoom);

        while (true)
        {
            JobTimer.Instance.Flush();
        }
    }

}




