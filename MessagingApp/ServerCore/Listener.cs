using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ServerCore
{
    public class Listener
    {
        Socket _listenSocket;
        Session _session;
        string line = "---------------------------------------";
        public void Init(IPEndPoint endPoint, Session session, int register = 1, int backlog = 100)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _session = session;
            _listenSocket.Bind(endPoint);

            _listenSocket.Listen(backlog);
            for (int i = 0; i < register; i++)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += OnAcceptCompleted;//new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
                RegisterAccept(args);
            }

        }
        public void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            //-- 소켓 비동기 대기 대기중일 경우 true / 대기가 끝났을 경우 false
            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false)
                OnAcceptCompleted(null, args);
        }

        public void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {

            if (args.SocketError == SocketError.Success)
            {
                _session.Start(args.AcceptSocket);
                _session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
                System.Console.WriteLine($"OnAcceptCompleted Error!\n{line}\n==>{args.SocketError}\n{line}");

            RegisterAccept(args);
        }

    }
}