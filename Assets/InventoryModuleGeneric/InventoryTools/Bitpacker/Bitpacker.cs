using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace InventoryModule.Packer
{
    #region Bit Writer

    /// <summary>
    /// uses Byte-Aligned Packing for Converting Data to bits
    /// </summary>
    public class ByteWriter
    {
        private byte[] buffer;

        /// <summary>
        /// Current write position in bytes.
        /// </summary>
        public int Position { get; private set; }

        private const byte NULL_MARKER = 0xFF;
        private const byte DICK_MARKER = 0xAA;

        /// <summary>
        /// The starting size of the internal buffer;
        /// </summary>
        /// <param name="capacity"> internal buffer capacity</param>
        public ByteWriter(int capacity = 256)
        {
            buffer = new byte[capacity];
            Position = 0;
        }

        /// <summary>
        /// Lazy-Auto setup
        /// </summary>
        public ByteWriter()
        {
            buffer = new byte[256];
            Position = 0;
        }

        // resets position to Zero. 
        public bool Reset()
        {
            Position = 0;
            return true;
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
            int IntValue = Unsafe.As<float, int>(ref value);
            Write(IntValue);
        }

        public void Write(double value)
        {
            long LongValue = Unsafe.As<double, long>(ref value);
            Write(LongValue);
        }

        public void Write(decimal value)
        {
            Span<int> bits = stackalloc int[4];
            var bitArray = decimal.GetBits(value);
            for (int i = 0; i < 4; i++)
            {
                bits[i] = bitArray[i];
            }

            Write(bits[0]);
            Write(bits[1]);
            Write(bits[2]);
            Write(bits[3]);
        }
        public void Write(string value)
        {
            if (value == null)
            {
                Write(-1);
                return;
            }

            int byteCount = Encoding.UTF8.GetByteCount(value);

            Write(byteCount);

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


        /// <summary>
        /// Writes an Enum value using unsafe casting for zero-overhead serialization, supports all types. 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Enum value)
        {
            var underlyingType = Enum.GetUnderlyingType(value.GetType());

            switch (underlyingType.Name)
            {
                case nameof(Byte):
                    Write(Unsafe.As<Enum, byte>(ref value));
                    break;
                case nameof(SByte):
                    Write(Unsafe.As<Enum, sbyte>(ref value));
                    break;
                case nameof(UInt16):
                    Write(Unsafe.As<Enum, ushort>(ref value));
                    break;
                case nameof(Int16):
                    Write(Unsafe.As<Enum, short>(ref value));
                    break;
                case nameof(UInt32):
                    Write(Unsafe.As<Enum, uint>(ref value));
                    break;
                case nameof(Int32):
                    Write(Unsafe.As<Enum, int>(ref value));
                    break;
                case nameof(UInt64):
                    Write(Unsafe.As<Enum, ulong>(ref value));
                    break;
                case nameof(Int64):
                    Write(Unsafe.As<Enum, long>(ref value));
                    break;
            }
        }



        public void Write<T>(IList<T> list)
        {
            if (list == null)
            {
                Write((sbyte)-1);
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

            for (int i = 0; i < list.Count; i++)
                Write(list[i]);
        }

        public void Write(IList<float> list)
        {
            if (list == null) { Write(-1); return; }
            Write(list.Count);

            for (int i = 0; i < list.Count; i++)
                Write(list[i]);
        }

        public void Write(IList<string> list)
        {
            if (list == null) { Write(-1); return; }
            Write(list.Count);

            for (int i = 0; i < list.Count; i++)
                Write(list[i]);
        }


        public void Write(Array array)
        {
            if (array == null)
            {
                Write((sbyte)-1);
                return;
            }

            Write(array.Length);

            foreach (var item in array)
            {
                WriteItem(item);
            }
        }

        private void WriteItem<T>(T item)
        {
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
                case Enum v: Write(v); break;
                default:
                    throw new NotSupportedException($"Type {typeof(T)} is not supported for serialization.");
            }
        }

        public void ClearBufferData()
        {
            Array.Clear(buffer, 0, Position);
            Position = 0;
        }
    }

    #endregion

    #region Bit Reader

    public sealed class ByteReader
    {
        private readonly byte[] _readBuffer;
        private int _position;

        public int Position => _position;

        public ByteReader(byte[] buffer)
        {
            _readBuffer = buffer;
            _position = 0;
        }

        // ── Primitives ────────────────────────────────────────────────

        public void Read(out bool value)
            => value = _readBuffer[_position++] != 0;

        public void Read(out byte value)
            => value = _readBuffer[_position++];

        public void Read(out sbyte value)
            => value = unchecked((sbyte)_readBuffer[_position++]);

        public void Read(out short value)
        {
            value = BinaryPrimitives.ReadInt16LittleEndian(_readBuffer.AsSpan(_position));
            _position += 2;
        }

        public void Read(out ushort value)
        {
            value = BinaryPrimitives.ReadUInt16LittleEndian(_readBuffer.AsSpan(_position));
            _position += 2;
        }

        public void Read(out int value)
        {
            value = BinaryPrimitives.ReadInt32LittleEndian(_readBuffer.AsSpan(_position));
            _position += 4;
        }

        public void Read(out uint value)
        {
            value = BinaryPrimitives.ReadUInt32LittleEndian(_readBuffer.AsSpan(_position));
            _position += 4;
        }

        public void Read(out long value)
        {
            value = BinaryPrimitives.ReadInt64LittleEndian(_readBuffer.AsSpan(_position));
            _position += 8;
        }

        public void Read(out ulong value)
        {
            value = BinaryPrimitives.ReadUInt64LittleEndian(_readBuffer.AsSpan(_position));
            _position += 8;
        }

        public void Read(out float value)
        {
            Read(out int bits);
            value = Unsafe.As<int, float>(ref bits);
        }

        public void Read(out double value)
        {
            Read(out long bits);
            value = Unsafe.As<long, double>(ref bits);
        }

        public void Read(out decimal value)
        {
            Span<int> bits = stackalloc int[4];
            Read(out bits[0]);
            Read(out bits[1]);
            Read(out bits[2]);
            Read(out bits[3]);
            value = new decimal(bits.ToArray());
        }

        public void Read(out char value)
        {
            Read(out short bits);
            value = (char)bits;
        }

        // ── String ───────────────────────────────────────────────────

        private const int StackStringThreshold = 256;

        public void Read(out string value)
        {
            Read(out int byteCount);

            if (byteCount == -1)
            {
                value = null;
                return;
            }

            // Small strings: decode from stack — zero heap alloc for the byte buffer
            if (byteCount <= StackStringThreshold)
            {
                Span<byte> slice = stackalloc byte[byteCount];
                _readBuffer.AsSpan(_position, byteCount).CopyTo(slice);
                value = Encoding.UTF8.GetString(slice);
            }
            else
            {
                // Large strings: decode directly from the buffer span — still no temp array
                value = Encoding.UTF8.GetString(_readBuffer, _position, byteCount);
            }

            _position += byteCount;
        }

        // ── Enums ────────────────────────────────────────────────────

        public void Read(Enum enumType, out Enum value)
        {
            var underlyingType = Enum.GetUnderlyingType(enumType.GetType());
            value = null;

            switch (underlyingType.Name)
            {
                case nameof(Byte):
                    Read(out byte b);
                    value = Unsafe.As<byte, Enum>(ref b);
                    break;
                case nameof(SByte):
                    Read(out sbyte sb);
                    value = Unsafe.As<sbyte, Enum>(ref sb);
                    break;
                case nameof(UInt16):
                    Read(out ushort us);
                    value = Unsafe.As<ushort, Enum>(ref us);
                    break;
                case nameof(Int16):
                    Read(out short s);
                    value = Unsafe.As<short, Enum>(ref s);
                    break;
                case nameof(UInt32):
                    Read(out uint ui);
                    value = Unsafe.As<uint, Enum>(ref ui);
                    break;
                case nameof(Int32):
                    Read(out int i);
                    value = Unsafe.As<int, Enum>(ref i);
                    break;
                case nameof(UInt64):
                    Read(out ulong ul);
                    value = Unsafe.As<ulong, Enum>(ref ul);
                    break;
                case nameof(Int64):
                    Read(out long l);
                    value = Unsafe.As<long, Enum>(ref l);
                    break;
            }
        }


        // ── Lists ────────────────────────────────────────────────────

        public void Read(out List<int> value)
        {
            Read(out int count);
            if (count == -1) { value = null; return; }

            value = new List<int>(count);
            for (int i = 0; i < count; i++) { Read(out int item); value.Add(item); }
        }

        public void Read(out List<float> value)
        {
            Read(out int count);
            if (count == -1) { value = null; return; }

            value = new List<float>(count);
            for (int i = 0; i < count; i++) { Read(out float item); value.Add(item); }
        }

        public void Read(out List<string> value)
        {
            Read(out int count);
            if (count == -1) { value = null; return; }

            value = new List<string>(count);
            for (int i = 0; i < count; i++) { Read(out string item); value.Add(item); }
        }
    }
    #endregion
}
