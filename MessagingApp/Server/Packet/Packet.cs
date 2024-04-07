using System.Data.Common;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using ServerCore;

public enum PacketOpcode
{
    C_LeaveGame,
    C_EnterGame,
    C_Chat,
    C_CreateRoom,
    C_EnterRoom,
    C_ExitRoom,
    C_KickUser,

    S_UserList,
    S_EnterGame,
    S_LeaveGame,
    S_Chat,
    S_CreatedRooms,
    S_CreateRoom,
    S_EnterRoom,
    S_ExitRoom,
    S_KickUser,
}
public interface IPacket
{
    ushort Opcode { get; }
    public class GamePacket { };
    void Read(ArraySegment<byte> segment);
    ArraySegment<byte> Write();
}

public class Room
{
    public int RoomId;
    public string RoomName;
    public string OwnerName;
}
/*
패킷 순서
[패킷 사이즈]-[Opcode]-[Message]
*/
#region ServerPacket
public class S_UserList : IPacket
{
    public ushort Opcode { get { return (ushort)PacketOpcode.S_UserList; } }
    public class GamePacket
    {
        public string UserName;
        public int RoomState;
    }
    public class User
    {
        public string UserName;
        public int RoomState;
        public void Read(ArraySegment<byte> segment, ref ushort count)
        {
            int offset = segment.Offset + count;
            offset += sizeof(ushort);
            offset += sizeof(ushort);
            string json = BitConverter.ToString(segment.Array, offset);
            var packet = JsonConvert.DeserializeObject<GamePacket>(json);
            UserName = packet.UserName;
            RoomState = packet.RoomState;
        }
        public bool Write(ArraySegment<byte> segment, ref ushort count)
        {
            bool isSuccess = true;
            byte[] opcodeBytes = BitConverter.GetBytes((ushort)PacketOpcode.S_EnterGame);
            byte[] userNameBytes = Encoding.UTF8.GetBytes(UserName);

            ushort size = (ushort)(sizeof(ushort) + opcodeBytes.Length + userNameBytes.Length);
            byte[] sizeBytes = BitConverter.GetBytes(size);

            ushort offset = (ushort)(segment.Offset + count);
            Array.Copy(sizeBytes, 0, segment.Array, offset, sizeBytes.Length);
            offset += (ushort)sizeBytes.Length;

            Array.Copy(sizeBytes, 0, segment.Array, offset, opcodeBytes.Length);
            offset += (ushort)opcodeBytes.Length;

            Array.Copy(sizeBytes, 0, segment.Array, offset, userNameBytes.Length);

            return isSuccess;
        }

    }
    public List<User> Users = new List<User>();

    public void Read(ArraySegment<byte> segment)
    {
        ushort offset = (ushort)segment.Offset;
        offset += sizeof(ushort);
        offset += sizeof(ushort);
        Users.Clear();
        ushort userCount = BitConverter.ToUInt16(segment.Array, offset);
        for (int i = 0; i < userCount; i++)
        {
            User user = new User();
            user.Read(segment, ref offset);
            Users.Add(user);
        }
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        byte[] opcodeBytes = BitConverter.GetBytes((ushort)PacketOpcode.S_UserList);
        byte[] userBytes = BitConverter.GetBytes((ushort)Users.Count);

        ushort size = (ushort)(sizeof(ushort) + opcodeBytes.Length + userBytes.Length);
        byte[] sizeBytes = BitConverter.GetBytes(size);

        ushort offset = 0;
        Array.Copy(sizeBytes, 0, segment.Array, offset, sizeBytes.Length);
        offset += (ushort)sizeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, opcodeBytes.Length);
        offset += (ushort)opcodeBytes.Length;
        foreach (User user in Users)
        {
            user.Write(segment, ref offset);
        }

        Array.Copy(sizeBytes, 0, segment.Array, offset, userBytes.Length);

        return SendBufferHelper.Close(size);
    }


}
public class S_EnterGame : IPacket
{
    public ushort Opcode { get { return (ushort)PacketOpcode.S_EnterGame; } }
    public class GamePacket
    {
        public string UserName;
        public int RoomId;
    }
    public GamePacket Packet = new GamePacket();
    public void Read(ArraySegment<byte> segment)
    {
        ushort size = 0;

        size += sizeof(ushort); //-- 메시지 사이즈
        size += sizeof(ushort); //-- 옵코드
        ushort offset = size;
        byte[] bytes = new byte[segment.Count - size];
        Array.Copy(segment.Array, offset, bytes, 0, bytes.Length);
        string json = Encoding.UTF8.GetString(bytes);
        Packet = JsonConvert.DeserializeObject<GamePacket>(json);
        System.Console.WriteLine($"S_EnterGame Read:{json}");
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        byte[] opcodeBytes = BitConverter.GetBytes((ushort)PacketOpcode.S_EnterGame);

        string json = JsonConvert.SerializeObject(Packet);
        System.Console.WriteLine($"Write Json : {json}");
        byte[] packetBytes = Encoding.UTF8.GetBytes(json);

        ushort size = (ushort)(sizeof(ushort) + opcodeBytes.Length + packetBytes.Length);
        byte[] sizeBytes = BitConverter.GetBytes(size);

        int offset = 0;
        Array.Copy(sizeBytes, 0, segment.Array, offset, sizeBytes.Length);
        offset += sizeBytes.Length;

        Array.Copy(opcodeBytes, 0, segment.Array, offset, opcodeBytes.Length);
        offset += opcodeBytes.Length;

        Array.Copy(packetBytes, 0, segment.Array, offset, packetBytes.Length);

        System.Console.WriteLine($"Write Size[{size}] Size Size[{sizeBytes.Length}] Opcode Size[{opcodeBytes.Length}] Packet Size[{packetBytes.Length}]");
        return SendBufferHelper.Close(size);
    }
}
public class S_LeaveGame : IPacket
{
    public ushort Opcode { get { return (ushort)PacketOpcode.S_LeaveGame; } }
    public class GamePacket
    {
        public string UserName;

    }
    public GamePacket Packet = new GamePacket();
    public void Read(ArraySegment<byte> segment)
    {
        int offset = segment.Offset;
        offset += sizeof(ushort);
        offset += sizeof(ushort);
        string json = BitConverter.ToString(segment.Array, offset);
        Packet = JsonConvert.DeserializeObject<GamePacket>(json);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        byte[] opcodeBytes = BitConverter.GetBytes((ushort)PacketOpcode.S_LeaveGame);

        string json = JsonConvert.SerializeObject(Packet);
        byte[] packetBytes = Encoding.UTF8.GetBytes(json);

        ushort size = (ushort)(sizeof(ushort) + opcodeBytes.Length + packetBytes.Length);
        byte[] sizeBytes = BitConverter.GetBytes(size);

        int offset = 0;
        Array.Copy(sizeBytes, 0, segment.Array, offset, sizeBytes.Length);
        offset += sizeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, opcodeBytes.Length);
        offset += opcodeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, packetBytes.Length);

        return SendBufferHelper.Close(size);
    }
}
public class S_Chat : IPacket
{
    public ushort Opcode { get { return (ushort)PacketOpcode.S_Chat; } }
    public class GamePacket
    {
        public string UserName;
        public string Message;
    }
    public GamePacket Packet = new GamePacket();
    public void Read(ArraySegment<byte> segment)
    {
        int offset = segment.Offset;
        offset += sizeof(ushort);
        offset += sizeof(ushort);
        string json = BitConverter.ToString(segment.Array, offset);
        Packet = JsonConvert.DeserializeObject<GamePacket>(json);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        byte[] opcodeBytes = BitConverter.GetBytes((ushort)PacketOpcode.S_Chat);

        string json = JsonConvert.SerializeObject(Packet);
        byte[] packetBytes = Encoding.UTF8.GetBytes(json);

        ushort size = (ushort)(sizeof(ushort) + opcodeBytes.Length + packetBytes.Length);
        byte[] sizeBytes = BitConverter.GetBytes(size);

        int offset = 0;
        Array.Copy(sizeBytes, 0, segment.Array, offset, sizeBytes.Length);
        offset += sizeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, opcodeBytes.Length);
        offset += opcodeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, packetBytes.Length);

        return SendBufferHelper.Close(size);
    }
}
public class S_CreatedRooms : IPacket
{
    public ushort Opcode { get { return (ushort)PacketOpcode.S_CreatedRooms; } }
    public class GamePacket
    {
        public List<Room> Rooms;
    }

    public GamePacket Packet = new GamePacket();
    public void Read(ArraySegment<byte> segment)
    {
        int offset = segment.Offset;
        offset += sizeof(ushort);
        offset += sizeof(ushort);
        string json = BitConverter.ToString(segment.Array, offset);
        Packet = JsonConvert.DeserializeObject<GamePacket>(json);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        byte[] opcodeBytes = BitConverter.GetBytes((ushort)PacketOpcode.S_CreatedRooms);

        string json = JsonConvert.SerializeObject(Packet);
        byte[] packetBytes = Encoding.UTF8.GetBytes(json);

        ushort size = (ushort)(sizeof(ushort) + opcodeBytes.Length + packetBytes.Length);
        byte[] sizeBytes = BitConverter.GetBytes(size);

        int offset = 0;
        Array.Copy(sizeBytes, 0, segment.Array, offset, sizeBytes.Length);
        offset += sizeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, opcodeBytes.Length);
        offset += opcodeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, packetBytes.Length);

        return SendBufferHelper.Close(size);
    }
}
public class S_CreateRoom : IPacket
{
    public ushort Opcode { get { return (ushort)PacketOpcode.S_CreateRoom; } }
    public class GamePacket
    {
        public string UserName;
        public string RoomName;
    }
    public GamePacket Packet = new GamePacket();
    public void Read(ArraySegment<byte> segment)
    {
        int offset = segment.Offset;
        offset += sizeof(ushort);
        offset += sizeof(ushort);
        string json = BitConverter.ToString(segment.Array, offset);
        Packet = JsonConvert.DeserializeObject<GamePacket>(json);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        byte[] opcodeBytes = BitConverter.GetBytes((ushort)PacketOpcode.S_CreateRoom);

        string json = JsonConvert.SerializeObject(Packet);
        byte[] packetBytes = Encoding.UTF8.GetBytes(json);

        ushort size = (ushort)(sizeof(ushort) + opcodeBytes.Length + packetBytes.Length);
        byte[] sizeBytes = BitConverter.GetBytes(size);

        int offset = 0;
        Array.Copy(sizeBytes, 0, segment.Array, offset, sizeBytes.Length);
        offset += sizeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, opcodeBytes.Length);
        offset += opcodeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, packetBytes.Length);

        return SendBufferHelper.Close(size);
    }

}
public class S_EnterRoom : IPacket
{
    public ushort Opcode { get { return (ushort)PacketOpcode.S_EnterRoom; } }
    public class GamePacket
    {
        public string UserName;
        public string RoomId;
    }
    public GamePacket Packet = new GamePacket();
    public void Read(ArraySegment<byte> segment)
    {
        int offset = segment.Offset;
        offset += sizeof(ushort);
        offset += sizeof(ushort);
        string json = BitConverter.ToString(segment.Array, offset);
        Packet = JsonConvert.DeserializeObject<GamePacket>(json);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        byte[] opcodeBytes = BitConverter.GetBytes((ushort)PacketOpcode.S_EnterRoom);

        string json = JsonConvert.SerializeObject(Packet);
        byte[] packetBytes = Encoding.UTF8.GetBytes(json);

        ushort size = (ushort)(sizeof(ushort) + opcodeBytes.Length + packetBytes.Length);
        byte[] sizeBytes = BitConverter.GetBytes(size);

        int offset = 0;
        Array.Copy(sizeBytes, 0, segment.Array, offset, sizeBytes.Length);
        offset += sizeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, opcodeBytes.Length);
        offset += opcodeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, packetBytes.Length);

        return SendBufferHelper.Close(size);
    }

}
public class S_ExitRoom : IPacket
{
    public ushort Opcode { get { return (ushort)PacketOpcode.S_ExitRoom; } }
    public class GamePacket
    {
        public string UserName;
        public string RoomId;
    }
    public GamePacket Packet = new GamePacket();
    public void Read(ArraySegment<byte> segment)
    {
        int offset = segment.Offset;
        offset += sizeof(ushort);
        offset += sizeof(ushort);
        string json = BitConverter.ToString(segment.Array, offset);
        Packet = JsonConvert.DeserializeObject<GamePacket>(json);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        byte[] opcodeBytes = BitConverter.GetBytes((ushort)PacketOpcode.S_ExitRoom);

        string json = JsonConvert.SerializeObject(Packet);
        byte[] packetBytes = Encoding.UTF8.GetBytes(json);

        ushort size = (ushort)(sizeof(ushort) + opcodeBytes.Length + packetBytes.Length);
        byte[] sizeBytes = BitConverter.GetBytes(size);

        int offset = 0;
        Array.Copy(sizeBytes, 0, segment.Array, offset, sizeBytes.Length);
        offset += sizeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, opcodeBytes.Length);
        offset += opcodeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, packetBytes.Length);

        return SendBufferHelper.Close(size);
    }
}
public class S_KickUser : IPacket
{
    public ushort Opcode { get { return (ushort)PacketOpcode.S_KickUser; } }
    public class GamePacket
    {
        public string UserName;
        public string KickUserName;
    }
    public GamePacket Packet = new GamePacket();
    public void Read(ArraySegment<byte> segment)
    {
        int offset = segment.Offset;
        offset += sizeof(ushort);
        offset += sizeof(ushort);
        string json = BitConverter.ToString(segment.Array, offset);
        Packet = JsonConvert.DeserializeObject<GamePacket>(json);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        byte[] opcodeBytes = BitConverter.GetBytes((ushort)PacketOpcode.S_KickUser);

        string json = JsonConvert.SerializeObject(Packet);
        byte[] packetBytes = Encoding.UTF8.GetBytes(json);

        ushort size = (ushort)(sizeof(ushort) + opcodeBytes.Length + packetBytes.Length);
        byte[] sizeBytes = BitConverter.GetBytes(size);

        int offset = 0;
        Array.Copy(sizeBytes, 0, segment.Array, offset, sizeBytes.Length);
        offset += sizeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, opcodeBytes.Length);
        offset += opcodeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, packetBytes.Length);

        return SendBufferHelper.Close(size);
    }
}
#endregion
#region ClientPacket
public class C_EnterGame : IPacket
{
    public ushort Opcode { get { return (ushort)PacketOpcode.C_EnterGame; } }
    public class GamePacket
    {
        public string UserName;
    }
    public GamePacket Packet = new GamePacket();
    public void Read(ArraySegment<byte> segment)
    {
        ushort size = 0;

        size += sizeof(ushort); //-- 메시지 사이즈
        size += sizeof(ushort); //-- 옵코드
        ushort offset = size;
        byte[] bytes = new byte[segment.Count - size];
        Array.Copy(segment.Array, offset, bytes, 0, bytes.Length);
        string json = Encoding.UTF8.GetString(bytes);
        Packet = JsonConvert.DeserializeObject<GamePacket>(json);
        System.Console.WriteLine($"Read:{json}");
    }
    public ArraySegment<byte> Write()
    {
        System.Console.WriteLine("TESTSSS");
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        byte[] opcodeBytes = BitConverter.GetBytes((ushort)PacketOpcode.C_EnterGame);

        string json = JsonConvert.SerializeObject(Packet);
        byte[] packetBytes = Encoding.UTF8.GetBytes(json);

        ushort size = (ushort)(sizeof(ushort) + opcodeBytes.Length + packetBytes.Length);
        byte[] sizeBytes = BitConverter.GetBytes(size);

        int offset = 0;
        Array.Copy(sizeBytes, 0, segment.Array, offset, sizeBytes.Length);
        offset += sizeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, opcodeBytes.Length);
        offset += opcodeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, packetBytes.Length);

        return SendBufferHelper.Close(size);
    }
}
public class C_LeaveGame : IPacket
{
    public ushort Opcode { get { return (ushort)PacketOpcode.C_LeaveGame; } }
    public class GamePacket
    {
        public string UserName;
    }
    public GamePacket Packet = new GamePacket();
    public void Read(ArraySegment<byte> segment)
    {
        int offset = segment.Offset;
        offset += sizeof(ushort);
        offset += sizeof(ushort);
        string json = BitConverter.ToString(segment.Array, offset);
        Packet = JsonConvert.DeserializeObject<GamePacket>(json);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        byte[] opcodeBytes = BitConverter.GetBytes((ushort)PacketOpcode.C_LeaveGame);

        string json = JsonConvert.SerializeObject(Packet);
        byte[] packetBytes = Encoding.UTF8.GetBytes(json);

        ushort size = (ushort)(sizeof(ushort) + opcodeBytes.Length + packetBytes.Length);
        byte[] sizeBytes = BitConverter.GetBytes(size);

        int offset = 0;
        Array.Copy(sizeBytes, 0, segment.Array, offset, sizeBytes.Length);
        offset += sizeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, opcodeBytes.Length);
        offset += opcodeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, packetBytes.Length);

        return SendBufferHelper.Close(size);
    }
}
public class C_Chat : IPacket
{
    public ushort Opcode { get { return (ushort)PacketOpcode.C_Chat; } }
    public class GamePacket
    {
        public string Message;
    }
    public GamePacket Packet = new GamePacket();
    public void Read(ArraySegment<byte> segment)
    {
        int offset = segment.Offset;
        offset += sizeof(ushort);
        offset += sizeof(ushort);
        string json = BitConverter.ToString(segment.Array, offset);
        Packet = JsonConvert.DeserializeObject<GamePacket>(json);

    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        byte[] opcodeBytes = BitConverter.GetBytes((ushort)PacketOpcode.C_Chat);

        string json = JsonConvert.SerializeObject(Packet);
        byte[] packetBytes = Encoding.UTF8.GetBytes(json);

        ushort size = (ushort)(sizeof(ushort) + opcodeBytes.Length + packetBytes.Length);
        byte[] sizeBytes = BitConverter.GetBytes(size);

        int offset = 0;
        Array.Copy(sizeBytes, 0, segment.Array, offset, sizeBytes.Length);
        offset += sizeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, opcodeBytes.Length);
        offset += opcodeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, packetBytes.Length);

        return SendBufferHelper.Close(size);
    }

}
public class C_CreateRoom : IPacket
{
    public ushort Opcode { get { return (ushort)PacketOpcode.C_CreateRoom; } }
    public class GamePacket
    {
        public string UserName;
        public string Message;
    }
    public GamePacket Packet = new GamePacket();
    public void Read(ArraySegment<byte> segment)
    {
        int offset = segment.Offset;
        offset += sizeof(ushort);
        offset += sizeof(ushort);
        string json = BitConverter.ToString(segment.Array, offset);
        Packet = JsonConvert.DeserializeObject<GamePacket>(json);

    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        byte[] opcodeBytes = BitConverter.GetBytes((ushort)PacketOpcode.C_CreateRoom);

        string json = JsonConvert.SerializeObject(Packet);
        byte[] packetBytes = Encoding.UTF8.GetBytes(json);

        ushort size = (ushort)(sizeof(ushort) + opcodeBytes.Length + packetBytes.Length);
        byte[] sizeBytes = BitConverter.GetBytes(size);

        int offset = 0;
        Array.Copy(sizeBytes, 0, segment.Array, offset, sizeBytes.Length);
        offset += sizeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, opcodeBytes.Length);
        offset += opcodeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, packetBytes.Length);

        return SendBufferHelper.Close(size);
    }

}
public class C_EnterRoom : IPacket
{
    public ushort Opcode { get { return (ushort)PacketOpcode.C_EnterRoom; } }
    public class GamePacket
    {
        public string UserName;
        public string RoomId;
    }
    public GamePacket Packet = new GamePacket();
    public void Read(ArraySegment<byte> segment)
    {
        int offset = segment.Offset;
        offset += sizeof(ushort);
        offset += sizeof(ushort);
        string json = BitConverter.ToString(segment.Array, offset);
        Packet = JsonConvert.DeserializeObject<GamePacket>(json);

    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        byte[] opcodeBytes = BitConverter.GetBytes((ushort)PacketOpcode.C_EnterRoom);

        string json = JsonConvert.SerializeObject(Packet);
        byte[] packetBytes = Encoding.UTF8.GetBytes(json);

        ushort size = (ushort)(sizeof(ushort) + opcodeBytes.Length + packetBytes.Length);
        byte[] sizeBytes = BitConverter.GetBytes(size);

        int offset = 0;
        Array.Copy(sizeBytes, 0, segment.Array, offset, sizeBytes.Length);
        offset += sizeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, opcodeBytes.Length);
        offset += opcodeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, packetBytes.Length);

        return SendBufferHelper.Close(size);
    }
}
public class C_ExitRoom : IPacket
{
    public ushort Opcode { get { return (ushort)PacketOpcode.C_ExitRoom; } }
    public class GamePacket
    {
        public string UserName;
        public string RoomId;
    }
    public GamePacket Packet = new GamePacket();
    public void Read(ArraySegment<byte> segment)
    {
        int offset = segment.Offset;
        offset += sizeof(ushort);
        offset += sizeof(ushort);
        string json = BitConverter.ToString(segment.Array, offset);
        Packet = JsonConvert.DeserializeObject<GamePacket>(json);

    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        byte[] opcodeBytes = BitConverter.GetBytes((ushort)PacketOpcode.C_ExitRoom);

        string json = JsonConvert.SerializeObject(Packet);
        byte[] packetBytes = Encoding.UTF8.GetBytes(json);

        ushort size = (ushort)(sizeof(ushort) + opcodeBytes.Length + packetBytes.Length);
        byte[] sizeBytes = BitConverter.GetBytes(size);

        int offset = 0;
        Array.Copy(sizeBytes, 0, segment.Array, offset, sizeBytes.Length);
        offset += sizeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, opcodeBytes.Length);
        offset += opcodeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, packetBytes.Length);

        return SendBufferHelper.Close(size);
    }
}
public class C_KickUser : IPacket
{
    public ushort Opcode { get { return (ushort)PacketOpcode.C_KickUser; } }
    public class GamePacket
    {
        public string UserName;
        public string KickUserName;
    }
    public GamePacket Packet = new GamePacket();
    public void Read(ArraySegment<byte> segment)
    {
        int offset = segment.Offset;
        offset += sizeof(ushort);
        offset += sizeof(ushort);
        string json = BitConverter.ToString(segment.Array, offset);
        Packet = JsonConvert.DeserializeObject<GamePacket>(json);

    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        byte[] opcodeBytes = BitConverter.GetBytes((ushort)PacketOpcode.C_KickUser);

        string json = JsonConvert.SerializeObject(Packet);
        byte[] packetBytes = Encoding.UTF8.GetBytes(json);

        ushort size = (ushort)(sizeof(ushort) + opcodeBytes.Length + packetBytes.Length);
        byte[] sizeBytes = BitConverter.GetBytes(size);

        int offset = 0;
        Array.Copy(sizeBytes, 0, segment.Array, offset, sizeBytes.Length);
        offset += sizeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, opcodeBytes.Length);
        offset += opcodeBytes.Length;

        Array.Copy(sizeBytes, 0, segment.Array, offset, packetBytes.Length);

        return SendBufferHelper.Close(size);
    }
}
#endregion