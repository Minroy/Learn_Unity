using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InventoryModule.Packer
{
    #region Bit Writer

    /// <summary>
    /// uses Byte-Aligned Packing for Converting Data to bits
    /// </summary>
    public class ByteWriter : IDisposable /* IAsyncDisposable*/
    {
        private byte[] buffer;

        /// <summary>
        /// Current write position in bytes.
        /// </summary>
        public int Position { get; private set; }


        public ByteWriter(int capacity = 256)
        {
            buffer = new byte[capacity];
            Position = 0;
        }
        // resets position to Zero. 
        public void Reset()
        {
            Position = 0;
        }

        public ReadOnlySpan<byte> AsSpan() => buffer.AsSpan(0, Position);

        public byte[] ToArray()
        {
            byte[] result = new byte[Position];
            Array.Copy(buffer, 0, result, 0, Position);
            return result;
        }

        // checks if the Current buffer has enough buffer bytes left to Write too.
        //if not then it just resizes the array
        public void EnsureCapacity(int bytesToWrite)
        {
            if (Position + bytesToWrite > buffer.Length)
            {
                int newCapacity = Math.Max(buffer.Length * 2, Position + bytesToWrite);
                Array.Resize(ref buffer, newCapacity);
            }
        }
        public void Write(bool value)
        {
            EnsureCapacity(1);
            buffer[Position++] = (byte)(value ? 1 : 0);
        }

        #region INTS_BITPACKING
        public void Write(byte value)
        {
            EnsureCapacity(1);
            buffer[Position++] = value;
        }
        public void Write(sbyte value)
        {
            EnsureCapacity(1);
            buffer[Position++] = (byte)value;
        }

        public void Write(short value)
        {
            EnsureCapacity(2);
            BinaryPrimitives.WriteInt16LittleEndian(buffer.AsSpan(Position), value);
            Position += 2;
        }
        public void Write(ushort value)
        {
            EnsureCapacity(2);
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(Position), value);
            Position += 2;
        }

        public void Write(int value)
        {
            EnsureCapacity(4);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(Position), value);
            Position += 4;
        }
        public void Write(uint value)
        {
            EnsureCapacity(4);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(Position), value);
            Position += 4;
        }

        public void Write(long value)
        {
            EnsureCapacity(8);
            BinaryPrimitives.WriteInt64LittleEndian(buffer.AsSpan(Position), value);
            Position += 8;
        }

        public void Write(ulong value)
        {
            EnsureCapacity(8);
            BinaryPrimitives.WriteUInt64LittleEndian(buffer.AsSpan(Position), value);
            Position += 8;
        }
        #endregion

        public void Write(float value)
        {
            int IntValue = BitConverter.SingleToInt32Bits(value);
            Write(IntValue);
        }

        public void Write(double value)
        {
            long LongValue = BitConverter.DoubleToInt64Bits(value);
            Write(LongValue);
        }

        public void Write(decimal value)
        {
            int[] bits = decimal.GetBits(value);
            for (int i = 0; i < bits.Length; i++)
            {
                Write(bits[i]);
            }
        }
        public void Write(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Write(0); // Length prefix = 0
                return;
            }

            int byteCount = Encoding.UTF8.GetByteCount(value);
            Write(byteCount); // 4-byte length prefix

            EnsureCapacity(byteCount);
            Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, Position);
            Position += byteCount;
        }

        public void Write(char value)
        {
            EnsureCapacity(2);
            BinaryPrimitives.WriteInt16LittleEndian(buffer.AsSpan(Position), (short)value);
            Position += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNull()
        {
            Write(false);
        }

        //Todo
        public void Write<TEnum>(TEnum value) where TEnum : struct, Enum
        {

        }


        public void Write<T>(IList<T> list)
        {
            if (list == null)
            {
                Write((int)-1);
                return;
            }

            Write(list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                // Direct method dispatch for primitives
                WriteItem(list[i]);
            }
        }

        public void Write(IList<int> list)
        {
            if (list == null) { Write(-1); return; }
            Write(list.Count);
            for (int i = 0; i < list.Count; i++) Write(list[i]);
        }

        public void Write(IList<float> list)
        {
            if (list == null) { Write(-1); return; }
            Write(list.Count);
            for (int i = 0; i < list.Count; i++) Write(list[i]);
        }

        public void Write(IList<string> list)
        {
            if (list == null) { Write(-1); return; }
            Write(list.Count);
            for (int i = 0; i < list.Count; i++) Write(list[i]);
        }
        private void WriteItem<T>(T item)
        {
            // Pattern matching directly on generic T (no boxing occurs!)
            switch (item)
            {
                case bool v: Write(v); break;
                case byte v: Write(v); break;
                case sbyte v: Write(v); break;
                case short v: Write(v); break;
                case ushort v: Write(v); break;
                case int v: Write(v); break;
                case uint v: Write(v); break;
                case long v: Write(v); break;
                case ulong v: Write(v); break;
                case float v: Write(v); break;
                case double v: Write(v); break;
                case decimal v: Write(v); break;
                case char v: Write(v); break;
                case string v: Write(v); break;
                default:
                    throw new NotSupportedException($"Type {typeof(T)} is not supported for serialization.");
            }
        }

        public void ClearBufferData()
        {
            buffer = Array.Empty<byte>();
            Array.Resize(ref buffer, 256);
            Position = 0;
        }
        public void Dispose()
        {

        }

        //public ValueTask DisposeAsync()
        //{
        //    throw new NotImplementedException();
        //}
    }

    #endregion

    #region Bit Reader

    public sealed class BitReader : IDisposable, IAsyncDisposable
    {
        public void Dispose()
        {

        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
