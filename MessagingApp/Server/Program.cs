using System.Net;
using System.Text;
using Server;
using ServerCore;

namespace Server
{
    class Program
    {
        static Listener _listener = new Listener();
        public static ChatRoom Room = new ChatRoom();
        static void FlushRoom()
        {
            Room.Push(() => Room.Flush());
            JobTimer.Instance.Push(FlushRoom, 250);
        }
        static void Test()
        {
            string json = "{'UserName':'Test'}";
            byte[] packetBytes = Encoding.UTF8.GetBytes(json);
            string decodedString = Encoding.UTF8.GetString(packetBytes);
            System.Console.WriteLine($"packetBytes[{packetBytes.Length}]");
            //System.Console.WriteLine($"json[{json}]/dJson[{decodedString}]");
        }
        static void Main(string[] args)
        {

            //string host = Dns.GetHostName();
            //IPHostEntry ipHost = Dns.GetHostEntry(host);
            // IP 주소를 로컬 호스트(127.0.0.1)로 설정
            IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
            //IPAddress ipAddr = ipHost.AddressList[0];

            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            ClientSession session = new ClientSession();
            session.SessionId = 0;
            _listener.Init(endPoint, session);
            Test();
            Console.WriteLine($"Listening...");

            JobTimer.Instance.Push(FlushRoom);

            while (true)
            {
                JobTimer.Instance.Flush();
            }
        }

    }




}

