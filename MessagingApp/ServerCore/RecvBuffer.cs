using System.Net.Sockets;

namespace ServerCore
{
    public class RecvBuffer
    {
        ArraySegment<byte> _buffer;
        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        /*
        클라이언트로부터 수신된 메시지의 크기만큼 _writePos는 증가한다.
        서버에서 처리된 메시지의 크기만큼 _readPos는 증가한다.
        */
        //-- 작성된 포지션
        int _writePos;
        //-- 현재까지 처리된 포지션
        int _readPos;

        //-- 수신된 메시지크기에서 읽을 수 있는 남은 데이터 사이즈 가져오기
        public int RemainingReadDataSize { get { return _writePos - _readPos; } }

        //-- 설정된 총 버퍼사이즈에서 삽입 가능한 데이터 사이즈 가져오기
        public int RemainingRecvDataSize { get { return _buffer.Count - _writePos; } }
        public string RecvMessage(NetworkStream stream)
        {
            byte[] dataLengthBytes = new byte[4];
            stream.Read(dataLengthBytes, 0, 4);
            int dataLength = BitConverter.ToInt32(dataLengthBytes, 0);

            byte[] data = new byte[dataLength];
            int bytesRead = 0;
            int totalBytesRead = 0;

            while (totalBytesRead < dataLength)
            {
                bytesRead = stream.Read(data, totalBytesRead, dataLength - totalBytesRead);
                totalBytesRead += bytesRead;
            }

            return "";
        }


        public ArraySegment<byte> GetReadSegment
        {
            get
            {
                int readOffset = _buffer.Offset + _readPos;
                return new ArraySegment<byte>(_buffer.Array, readOffset, RemainingReadDataSize);
            }
        }
        public ArraySegment<byte> GetWriteSegment
        {
            get
            {
                int writeOffset = _buffer.Offset + _writePos;
                return new ArraySegment<byte>(_buffer.Array, writeOffset, RemainingRecvDataSize);
            }
        }

        ///<summary>
        ///RecvBuffer 사이즈 확보하기 (처리가 안된 메시지 크기를 제외하고 사용완료된 메시지크기만큼 확보)
        ///</summary>
        public void Clean()
        {
            //-- 남은 데이터 사이즈
            int dataSize = RemainingReadDataSize;
            if (dataSize == 0)
            {
                //-- 남은 데이터가 없으면 복사 하지 않고 초기화
                _readPos = _writePos = 0;
            }
            else
            {

                int copyArrayOffset = _buffer.Offset + _readPos; //-- 처리되지 않은 버퍼의 위치부터 (시작점) 복사
                Array.Copy(_buffer.Array, copyArrayOffset, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        ///<summary>
        ///RecvBuffer에 읽기 여유공간 체크하기 (있을 경우 읽을 공간 확보후 True 없을 경우 False 반환)
        ///</summary>
        public bool OnRead(int readSize)
        {
            //-- 읽기 가능한 사이즈보다 크면 종료
            if (readSize > RemainingReadDataSize) return false;

            _readPos += readSize;
            return true;
        }

        /// <summary>
        /// RecvBuffer에 작성 여유공간 체크하기 (있을 경우 작성 공간 확보후 True 없을 경우 False 반환)
        /// </summary>
        public bool OnWrite(int writeSize)
        {
            //-- 작성 가능한 사이즈보다 크면 종료
            if (writeSize > RemainingRecvDataSize) return false;

            _writePos += writeSize;
            return true;
        }






    }
}