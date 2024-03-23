using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;

namespace ServerCore
{
    public class SendBufferHelper
    {
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });
        public static int ChunkSize { get; set; } = 65535 * 10;

        public static ArraySegment<byte> Open(int reserveSize)
        {
            if (CurrentBuffer.Values == null)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            if (CurrentBuffer.Value.RemainingBufferSize < reserveSize)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }
    public class SendBuffer
    {
        byte[] _buffer;
        int _usedSize = 0;

        public int RemainingBufferSize { get { return _buffer.Length - _usedSize; } }
        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }

        public ArraySegment<byte> Open(int reserveSize)
        {
            if (reserveSize > RemainingBufferSize) return null;

            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }

        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);

            _usedSize += usedSize;
            return segment;
        }





    }

}