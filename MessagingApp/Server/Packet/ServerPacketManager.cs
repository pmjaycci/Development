using ServerCore;

public class PacketManager
{
    static PacketManager _instance = new PacketManager();
    public static PacketManager Instance { get { return _instance; } }

    PacketManager()
    {
        Register();
    }

    Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _makeFunc = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {
        _makeFunc.Add((ushort)PacketOpcode.C_EnterGame, MakePacket<C_EnterGame>);
        _makeFunc.Add((ushort)PacketOpcode.C_LeaveGame, MakePacket<C_LeaveGame>);
        _makeFunc.Add((ushort)PacketOpcode.C_Chat, MakePacket<C_Chat>);
        _makeFunc.Add((ushort)PacketOpcode.C_CreateRoom, MakePacket<C_CreateRoom>);
        _makeFunc.Add((ushort)PacketOpcode.C_EnterRoom, MakePacket<C_EnterRoom>);
        _makeFunc.Add((ushort)PacketOpcode.C_ExitRoom, MakePacket<C_ExitRoom>);
        _makeFunc.Add((ushort)PacketOpcode.C_KickUser, MakePacket<C_KickUser>);

        _handler.Add((ushort)PacketOpcode.C_EnterGame, PacketHandler.C_EnterGameHandler);
        _handler.Add((ushort)PacketOpcode.C_LeaveGame, PacketHandler.C_LeaveGameHandler);
        _handler.Add((ushort)PacketOpcode.C_Chat, PacketHandler.C_ChatHandler);
        _handler.Add((ushort)PacketOpcode.C_CreateRoom, PacketHandler.C_CreateRoomHandler);
        _handler.Add((ushort)PacketOpcode.C_EnterRoom, PacketHandler.C_EnterRoomHandler);
        _handler.Add((ushort)PacketOpcode.C_ExitRoom, PacketHandler.C_ExitRoomHandler);
        _handler.Add((ushort)PacketOpcode.C_KickUser, PacketHandler.C_KickUserHandler);

    }
    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> onRecvCallback = null)
    {
        ushort count = 0;
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += sizeof(ushort);
        ushort opcode = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);

        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;
        if (_makeFunc.TryGetValue(opcode, out func))
        {
            IPacket packet = func.Invoke(session, buffer);
            if (onRecvCallback != null)
                onRecvCallback.Invoke(session, packet);
            else
                HandlePacket(session, packet);
        }
    }

    T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        T pkt = new T();
        pkt.Read(buffer);
        return pkt;
    }

    public void HandlePacket(PacketSession session, IPacket packet)
    {
        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(packet.Opcode, out action))
            action.Invoke(session, packet);
    }
}