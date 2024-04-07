using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;

        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            //-- 처리된 버퍼 길이
            int processLen = 0;
            //-- 처리한 패킷 갯수
            int packetCount = 0;
            while (true)
            {
                //-- 최소 헤더는 파싱할수 있는지
                if (buffer.Count < HeaderSize) break;


                ushort recvSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);

                if (buffer.Count < recvSize) break;

                //-- 컨텐츠 세션에서 패킷 처리
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, recvSize));
                //-- 처리된 패킷 갯수 증가
                packetCount++;
                //-- 완료된 버퍼 길이 증가
                processLen += recvSize;

                int offset = buffer.Offset + recvSize;
                int remainBufferSize = buffer.Count - recvSize;
                //-- 처리한 버퍼만큼 버퍼크기 줄이기
                buffer = new ArraySegment<byte>(buffer.Array, offset, remainBufferSize);


            }
            if (packetCount > 1)
            {
                System.Console.WriteLine($"처리 패킷 갯수 : {packetCount}");
            }
            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }
    public abstract class Session
    {
        int _disconnected = 0;
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
        Socket _socket;
        object _lock = new object();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        RecvBuffer _recvBuffer = new RecvBuffer(65535);

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);
        public void Start(Socket socket)
        {
            _socket = socket;
            _recvArgs = new SocketAsyncEventArgs();
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }
        public void Disconnect()
        {
            //-- 기존 _disconnected값 비교후 이미 종료된상태 (1) 이라면 종료
            if (Interlocked.Exchange(ref _disconnected, 1) == 1) return;

            OnDisconnected(_socket.RemoteEndPoint);

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            Clear();


        }
        void Clear()
        {
            lock (_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }

        #region 네트워크 통신 (수신)
        void RegisterRecv()
        {
            if (_disconnected == 1) return;

            //-- 처리된 버퍼 제거 및 수신 가능 버퍼 크기 확보
            _recvBuffer.Clean();

            //-- 수신 가능한 버퍼크기만큼의 segment 생성
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;

            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);
            try
            {
                bool pending = _socket.ReceiveAsync(_recvArgs);

                if (pending == false)
                {
                    OnRecvCompleted(null, _recvArgs);
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine($"RegisterRecv Failed\n==>{e}");
            }

        }
        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    //-- 수신한 버퍼 크기만큼 _writePos 증가
                    bool isAbleWrite = _recvBuffer.OnWrite(args.BytesTransferred);
                    if (isAbleWrite == false)
                    {
                        Disconnect();
                        return;
                    }

                    //-- 컨텐츠 처리 하는쪽에 데이터 넘겨주고 처리된 사이즈 가져오기
                    int processLen = OnRecv(_recvBuffer.ReadSegment);

                    //-- 수신한 버퍼 크기만큼 _readPos 증가
                    bool isAbleRead = _recvBuffer.OnRead(processLen);
                    if (isAbleRead == false)
                    {
                        Disconnect();
                        return;
                    }

                    //-- 수신할 버퍼 있는지 재귀
                    RegisterRecv();

                }
                catch (Exception e)
                {
                    System.Console.WriteLine($"OnRecvCompleted Error {e}");
                }
            }
        }
        #endregion

        #region 네트워크 통신 (송신)
        public void Send(List<ArraySegment<byte>> sendBuffList)
        {
            if (sendBuffList.Count == 0)
                return;

            lock (_lock)
            {
                foreach (ArraySegment<byte> sendBuff in sendBuffList)
                {
                    //-- 메시지는 일단 Queue에 담기
                    _sendQueue.Enqueue(sendBuff);
                }
                /* 
                _pendingList.Count 크기
                0    -> 송신할 데이터가 없으므로 송신 시작
                1이상 -> 송신할 데이터가 남아있다는 의미이므로 송신중이라 판단
                    이미 Queue에 담았으므로 송신중인 것이 송신 모두 완료되면 해당 메시지 처리
                 */
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }
        public void Send(ArraySegment<byte> sendBuff)
        {
            lock (_lock)
            {
                //-- 메시지는 일단 Queue에 담기
                _sendQueue.Enqueue(sendBuff);

                /* 
                _pendingList.Count 크기
                0    -> 송신할 데이터가 없으므로 송신 시작
                1이상 -> 송신할 데이터가 남아있다는 의미이므로 송신중이라 판단
                        이미 Queue에 담았으므로 송신중인 것이 송신 모두 완료되면 해당 메시지 처리
                */
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        void RegisterSend()
        {
            //-- 연결 종료시 종료
            if (_disconnected == 1) return;

            while (_sendQueue.Count > 0)
            {
                //-- Queue에 담긴 버퍼만큼 리스트에 담기 (패킷 모아 보내기)
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendingList.Add(buff);
            }
            //-- BufferList에 데이터 전달
            _sendArgs.BufferList = _pendingList;

            try
            {
                //-- 송신 처리 (송신중일 경우 true, 송신 완료시 처리할게 없으므로 false 반환)
                bool pending = _socket.SendAsync(_sendArgs);
                if (pending == false)
                    OnSendCompleted(null, _sendArgs);
            }
            catch (Exception e)
            {
                System.Console.WriteLine($"RegisterSend Error {e}");
            }
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        //-- 컨텐츠 처리 하는쪽에 넘겨서 송신 처리
                        OnSend(_sendArgs.BytesTransferred);

                        //-- Lock걸려 있는 동안 SendQueue에 담긴 데이터가 있을경우 재귀
                        if (_sendQueue.Count > 0)
                            RegisterSend();
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine($"OnSendCompleted Error {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }
        #endregion
    }
}