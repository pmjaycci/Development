using Newtonsoft.Json;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace Server
{
    public class ChatRoom
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobQueue = new JobQueue();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        List<Room> Rooms = new List<Room>();
        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Flush()
        {
            foreach (ClientSession s in _sessions)
                s.Send(_pendingList);

            _pendingList.Clear();
        }

        public void Broadcast(ArraySegment<byte> segment)
        {
            _pendingList.Add(segment);
        }
        public void Connect(ClientSession session)
        {
            // 플레이어 추가하고
            _sessions.Add(session);
            session.Room = this;
            System.Console.WriteLine($"Connect ==> Add User Session");
        }

        //-- 접속하기
        public void Enter(ClientSession session, C_EnterGame enterUser)
        {
            ushort opcode = enterUser.Opcode;
            C_EnterGame.GamePacket packet = enterUser.Packet;
            string name = packet.UserName;

            session.UserName = packet.UserName;
            session.RoomId = (int)RoomState.Lobby;
            System.Console.WriteLine($"Enter Test\n==>Opcode[{opcode}] Name[{name}]");

            // 신입생 입장을 모두에게 알린다
            S_EnterGame enter = new S_EnterGame();
            enter.Packet.UserName = session.UserName;
            enter.Packet.RoomId = session.RoomId;
            session.Send(enter.Write());
            Broadcast(enter.Write());
            /*

            //-- 접속 유저에게 이미 접속중인 유저 목록 전송
            S_UserList userList = new S_UserList();
            foreach (ClientSession s in _sessions)
            {
                S_UserList.User user = new S_UserList.User();
                user.UserName = session.UserName;
                user.RoomState = (int)RoomState.Lobby;
                userList.Users.Add(user);
            }
            session.Send(userList.Write());
            //-- 접속 유저에게 기존 생성된 채팅방 목록 전송
            //S_CreatedRooms createdRooms = new S_CreatedRooms();
            //createdRooms.Packet.Rooms = Rooms;
            */
        }
        //-- 접속 종료
        public void Leave(ClientSession session)
        {
            // 플레이어 제거하고
            _sessions.Remove(session);

            // 모두에게 알린다
            S_LeaveGame leave = new S_LeaveGame();
            leave.Packet.UserName = session.UserName;
            Broadcast(leave.Write());
        }

        public void CreateRoom(ClientSession session)
        {
            S_CreateRoom room = new S_CreateRoom();
            room.Packet.RoomName = session.RoomName;

            //-- 생성된 방을 유저에게 알린다.
            Broadcast(room.Write());
        }
    }
}
