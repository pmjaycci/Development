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
            throw new NotImplementedException();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            throw new NotImplementedException();
        }
    }
}