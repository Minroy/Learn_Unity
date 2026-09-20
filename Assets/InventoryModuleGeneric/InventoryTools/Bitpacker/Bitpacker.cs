using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace InventoryModule.Packer
{
    // Interface any custom type implements to self-serialize.
    public interface IEncoder
    {
        void Encode(ByteWriter writer);
    }

    public interface IDecoder
    {
        void Decode(ByteReader reader);
    }

    #region ByteWriter

    /// <summary>
    /// Byte-Aligned writing, with Zero-Allocations. 
    /// </summary>
    public sealed class ByteWriter : IDisposable
    {
        private byte[] _buffer;

        // Sentinel written for null strings so the reader
        // can distinguish null from empty.
        private const byte NULL_SENTINEL = 0xFF;

        public int Position { get; private set; }

        public ByteWriter(int capacity = 256)
        {
            // 1. Rent the initial buffer instead of 'new byte[]'
            _buffer = ArrayPool<byte>.Shared.Rent(capacity);
        }

        // ── Buffer management ──────────────────────────────────────────

        public void Reset() => Position = 0;

        public ReadOnlySpan<byte> AsSpan() => _buffer.AsSpan(0, Position);

        public byte[] ToArray()
        {
            var result = new byte[Position];
            _buffer.AsSpan(0, Position).CopyTo(result);
            return result;
        }


        public void EnsureCapacity(int bytesToWrite)
        {
            if (Position + bytesToWrite > _buffer.Length)
            {
                int newCapacity = Math.Max(_buffer.Length * 2, Position + bytesToWrite);

                byte[] newBuffer = ArrayPool<byte>.Shared.Rent(newCapacity);


                _buffer.AsSpan(0, Position).CopyTo(newBuffer);

                // 4. Return the OLD buffer back to the pool to prevent GC allocation
                ArrayPool<byte>.Shared.Return(_buffer);

                // 5. Swap the reference
                _buffer = newBuffer;
            }
        }
        public void Dispose()
        {
            if (_buffer != null)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
                _buffer = null;
            }
        }
        // ── Primitives ─────────────────────────────────────────────────

        public void Write(bool value)
        {
            EnsureCapacity(1);
            _buffer[Position++] = (byte)(value ? 1 : 0);
        }

        public void Write(byte value)
        {
            EnsureCapacity(1);
            _buffer[Position++] = value;
        }

        public void Write(sbyte value)
        {
            EnsureCapacity(1);
            _buffer[Position++] = (byte)value;
        }

        public void Write(short value)
        {
            EnsureCapacity(2);
            BinaryPrimitives.WriteInt16LittleEndian(_buffer.AsSpan(Position), value);
            Position += 2;
        }

        public void Write(ushort value)
        {
            EnsureCapacity(2);
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.AsSpan(Position), value);
            Position += 2;
        }

        public void Write(int value)
        {
            EnsureCapacity(4);
            BinaryPrimitives.WriteInt32LittleEndian(_buffer.AsSpan(Position), value);
            Position += 4;
        }

        public void Write(uint value)
        {
            EnsureCapacity(4);
            BinaryPrimitives.WriteUInt32LittleEndian(_buffer.AsSpan(Position), value);
            Position += 4;
        }

        public void Write(long value)
        {
            EnsureCapacity(8);
            BinaryPrimitives.WriteInt64LittleEndian(_buffer.AsSpan(Position), value);
            Position += 8;
        }

        public void Write(ulong value)
        {
            EnsureCapacity(8);
            BinaryPrimitives.WriteUInt64LittleEndian(_buffer.AsSpan(Position), value);
            Position += 8;
        }

        public void Write(float value) => Write(Unsafe.As<float, int>(ref value));
        public void Write(double value) => Write(Unsafe.As<double, long>(ref value));

        public void Write(decimal value)
        {
            EnsureCapacity(16);
            ReadOnlySpan<byte> bytes = MemoryMarshal.CreateReadOnlySpan(
                ref Unsafe.As<decimal, byte>(ref value), 16);
            bytes.CopyTo(_buffer.AsSpan(Position));
            Position += 16;
        }

        public void Write(char value)
        {
            EnsureCapacity(2);
            BinaryPrimitives.WriteInt16LittleEndian(_buffer.AsSpan(Position), (short)value);
            Position += 2;
        }

        // ── Strings ────────────────────────────────────────────────────

        public void Write(string value)
        {
            if (value == null) { Write(NULL_SENTINEL); return; }

            int byteCount = Encoding.UTF8.GetByteCount(value);
            Write(byteCount);
            EnsureCapacity(byteCount);
            Encoding.UTF8.GetBytes(value, 0, value.Length, _buffer, Position);
            Position += byteCount;
        }

        public void Write(IEncoder encoder)
        {
            encoder.Encode(this);
        }

        /// <summary>
        /// Can only write, unmanged-Types. 
        /// </summary>
        public void Write<T>(T value) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();
            EnsureCapacity(size);

            // Writes the raw memory of the enum directly into the buffer array
            Unsafe.WriteUnaligned(ref _buffer[Position], value);
            Position += size;
        }

        public void Write<T>(List<T> list) where T : unmanaged
        {
            Write(list.Count);
            if (list.Count <= 0) return;

            for (int i = 0; i < list.Count; i++)
            {
                Write(list[i]);
            }
        }

        public void Write(List<IEncoder> list)
        {
            Write(list.Count);
            if (list.Count <= 0) return;

            for (int i = 0; i < list.Count; i++)
            {
                Write(list[i]);
            }
        }

        public void Write<T>(T[] items) where T : unmanaged
        {
            var size = items.Length;
            Write(size);
            for (int i = 0; i < items.Length; i++)
            {
                Write(items[i]);
            }
        }
        public void Write(IEncoder[] items)
        {
            var size = items.Length;
            Write(size);
            for (int i = 0; i < items.Length; i++)
            {
                Write(items[i]);
            }
        }

        /// <summary>
        /// Can only write, unmanged-Types. 
        /// </summary>
        public void Write<T>(ReadOnlySpan<T> items) where T : unmanaged
        {
            Write(items.Length);
            var bytes = MemoryMarshal.AsBytes(items);
            EnsureCapacity(bytes.Length);
            bytes.CopyTo(_buffer.AsSpan(Position));
            Position += bytes.Length;
        }

        public void Write(ReadOnlySpan<IEncoder> items)
        {
            Write(items.Length);
            for (int i = 0; i > items.Length; i++)
            {

            }
        }
    }
}

    #endregion

#region ByteReader

public sealed class ByteReader : IDisposable
{
    private byte[] _buffer;
    private int _position;
    private byte NULL_SENTINEL = 0xFF;

    public ByteReader(byte[] buffer)
    {
        _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        _position = 0;
    }

    public ByteReader(ReadOnlySpan<byte> buffer)
    {
        _buffer = buffer.ToArray();
    }

    public void Dispose()
    {
        if (_buffer != null)
        {
            ArrayPool<byte>.Shared.Return(_buffer, false);
            _buffer = null;
        }
    }


// ── Buffer management ──────────────────────────────────────────

[MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckBounds(int bytesNeeded)
    {
        if (_position + bytesNeeded > _buffer.Length)
            throw new InvalidOperationException(
                $"ByteReader out of bounds: need {bytesNeeded} byte(s) at position " +
                $"{_position}, buffer length {_buffer.Length}.");
    }

    // ── Primitives ─────────────────────────────────────────────────

    public void Read(out bool value) { CheckBounds(1); value = _buffer[_position++] != 0; }
    public void Read(out byte value) { CheckBounds(1); value = _buffer[_position++]; }
    public void Read(out sbyte value) { CheckBounds(1); value = unchecked((sbyte)_buffer[_position++]); }

    public void Read(out short value)
    {
        CheckBounds(2);
        value = BinaryPrimitives.ReadInt16LittleEndian(_buffer.AsSpan(_position));
        _position += 2;
    }

    public void Read(out ushort value)
    {
        CheckBounds(2);
        value = BinaryPrimitives.ReadUInt16LittleEndian(_buffer.AsSpan(_position));
        _position += 2;
    }

    public void Read(out int value)
    {
        CheckBounds(4);
        value = BinaryPrimitives.ReadInt32LittleEndian(_buffer.AsSpan(_position));
        _position += 4;
    }

    public void Read(out uint value)
    {
        CheckBounds(4);
        value = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.AsSpan(_position));
        _position += 4;
    }

    public void Read(out long value)
    {
        CheckBounds(8);
        value = BinaryPrimitives.ReadInt64LittleEndian(_buffer.AsSpan(_position));
        _position += 8;
    }

    public void Read(out ulong value)
    {
        CheckBounds(8);
        value = BinaryPrimitives.ReadUInt64LittleEndian(_buffer.AsSpan(_position));
        _position += 8;
    }

    public void Read(out float value)
    {
        CheckBounds(4);
        Read(out int bits);
        value = BitConverter.Int32BitsToSingle(bits);
    }

    public void Read(out double value)
    {
        CheckBounds(8);
        Read(out long bits);
        value = BitConverter.Int64BitsToDouble(bits);
    }

    public void Read(out decimal value)
    {
        CheckBounds(16);
        value = default;
        ref byte src = ref _buffer[_position];
        value = Unsafe.ReadUnaligned<decimal>(ref src);
        _position += 16;
    }

    public void Read(out char value)
    {
        Read(out short bits);
        value = (char)bits;
    }

    // ── Strings ────────────────────────────────────────────────────

    public void Read(out string value)
    {
        Read(out int byteCount);

        if (byteCount == NULL_SENTINEL) { value = null; return; }
        if (byteCount == 0) { value = string.Empty; return; }

        CheckBounds(byteCount);
        value = Encoding.UTF8.GetString(_buffer, _position, byteCount);
        _position += byteCount;
    }
}

#endregion
