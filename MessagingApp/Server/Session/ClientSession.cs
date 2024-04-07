using System.Net;
using System.Runtime.InteropServices;
using ServerCore;

namespace Server
{
    public enum RoomState
    {
        Lobby,
    }
    public class ClientSession : PacketSession
    {
        public ChatRoom Room { get; set; }
        public int SessionId { get; set; }
        public string UserName { get; set; }
        public bool IsOwner { get; set; } = false;
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public override void OnConnected(EndPoint endPoint)
        {
            if (endPoint is IPEndPoint ipEndPoint)
            {
                string ipAddress = ipEndPoint.Address.ToString();
                int port = ipEndPoint.Port;
                Console.WriteLine($"ClientSession OnConnected : IP Address = {ipAddress}, Port = {port}");
            }
            else
            {
                Console.WriteLine($"ClientSession OnConnected : Unknown EndPoint Type");
            }
            Program.Room.Push(() => Program.Room.Connect(this));
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);
            if (Room != null)
            {
                ChatRoom room = Room;
                room.Push(() => room.Leave(this));
                Room = null;
            }

            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}