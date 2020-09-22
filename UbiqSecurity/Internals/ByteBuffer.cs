using System;

namespace UbiqSecurity.Internals
{
    internal class ByteBuffer
    {
        private byte[] _buffer;      // never null, but can be empty

        internal ByteBuffer(byte[] data = null)
        {
            _buffer = data ?? new byte[0];
        }

        internal int Length => _buffer.Length;

        // quick + dirty: a stronger impl would return a cloned *copy* of the buffer 
        internal byte[] Peek() => _buffer;

        internal void Enqueue(byte[] data, int offset, int count)
        {
            if ((offset + count) > data.Length)
            {
                throw new ArgumentException("offset + count would cause overflow");
            }

            if (count > 0)
            {
                var newBuffer = new byte[_buffer.Length + count];
                Array.Copy(_buffer, newBuffer, _buffer.Length);
                Array.Copy(data, offset, newBuffer, _buffer.Length, count);
                _buffer = newBuffer;
            }
        }

        internal byte[] Dequeue(int count)
        {
            if (count > _buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count exceeds Length");
            }

            // copy bytes from front of original buffer
            var dequeuedBytes = new byte[count];
            Array.Copy(_buffer, dequeuedBytes, dequeuedBytes.Length);

            // strip dequeued bytes from front of original buffer
            var newBuffer = new byte[_buffer.Length - count];
            Array.Copy(_buffer, count, newBuffer, 0, newBuffer.Length);
            _buffer = newBuffer;

            return dequeuedBytes;
        }
    }
}
