using Server;
using ServerCore;

class PacketHandler
{
    public static void C_LeaveGameHandler(PacketSession session, IPacket packet)
    {
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null)
            return;

        ChatRoom room = clientSession.Room;
        room.Push(() => room.Leave(clientSession));
    }

    public static void C_ChatHandler(PacketSession session, IPacket packet)
    {
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null)
            return;

        ChatRoom room = clientSession.Room;
        room.Push(() => room.Leave(clientSession));
    }

    public static void C_CreateRoomHandler(PacketSession session, IPacket packet)
    {

    }
    public static void C_EnterRoomHandler(PacketSession session, IPacket packet)
    {

    }
    public static void C_ExitRoomHandler(PacketSession session, IPacket packet)
    {

    }
    public static void C_KickUserHandler(PacketSession session, IPacket packet)
    {

    }
}